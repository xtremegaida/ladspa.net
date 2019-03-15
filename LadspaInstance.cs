using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   public class LadspaInstance : IDisposable
   {
      [DllImport("kernel32.dll")]
      protected static extern void RtlZeroMemory(IntPtr dst, int length);

      public readonly LadspaDescriptor LadspaDescriptor;

      protected IntPtr handle;
      protected IntPtr data;
      protected bool active;
      protected int sampleRate;
      protected readonly int[] dataOffsets;
      protected readonly int[] inputOffsets;
      protected readonly int[] outputOffsets;
      protected readonly float[] paramBuffer;
      protected readonly int bufferSize;
      protected float[] mixBuffer;

      public readonly int InputCount;
      public readonly int OutputCount;

      internal LadspaInstance(LadspaDescriptor descriptor, IntPtr handle, int sampleRate, int bufferSize)
      {
         if (bufferSize < 512) { bufferSize = 512; }
         LadspaDescriptor = descriptor;
         this.handle = handle;
         this.bufferSize = bufferSize;
         this.sampleRate = sampleRate;
         int inputCount = 0, outputCount = 0;
         for (int i = 0; i < descriptor.Ports.Length; i++)
         {
            if (descriptor.Ports[i].PortType != LadspaPortType.Audio) { continue; }
            LadspaPortDirection dir = descriptor.Ports[i].PortDirection;
            if (dir == LadspaPortDirection.Input) { inputCount++; }
            else if (dir == LadspaPortDirection.Output) { outputCount++; }
         }
         InputCount = inputCount;
         OutputCount = outputCount;
         dataOffsets = new int[descriptor.Ports.Length];
         inputOffsets = new int[inputCount];
         outputOffsets = new int[outputCount];
         paramBuffer = new float[1];
         if (descriptor.ConnectPort != null)
         {
            int dataSize = 0, blockSize = (int)(bufferSize * 4);
            inputCount = 0; outputCount = 0;
            for (int i = 0; i < descriptor.Ports.Length; i++)
            {
               dataOffsets[i] = dataSize;
               if (descriptor.Ports[i].PortType != LadspaPortType.Audio) { dataSize += 4; }
               else
               {
                  LadspaPortDirection dir = descriptor.Ports[i].PortDirection;
                  if (dir == LadspaPortDirection.Input) { inputOffsets[inputCount++] = dataSize; }
                  else if (dir == LadspaPortDirection.Output) { outputOffsets[outputCount++] = dataSize; }
                  dataSize += blockSize;
               }
            }
            if (dataSize > 0)
            {
               data = Marshal.AllocHGlobal(dataSize);
               RtlZeroMemory(data, dataSize);
               ResetParameters();
               for (uint i = 0; i < descriptor.Ports.Length; i++)
               {
                  descriptor.ConnectPort(handle, i, IntPtr.Add(data, dataOffsets[i]));
               }
            }
         }
      }

      protected void CopyInputs(float[][] inputs)
      {
         if (data == IntPtr.Zero) { return; }
         for (int i = 0; i < inputOffsets.Length; i++)
         {
            int copy = inputs != null && inputs.Length > i && inputs[i] != null ? Math.Min(inputs[i].Length, bufferSize) : 0;
            if (copy > 0) { Marshal.Copy(inputs[i], 0, IntPtr.Add(data, inputOffsets[i]), copy); }
            if (copy < bufferSize) { RtlZeroMemory(IntPtr.Add(data, inputOffsets[i]), (bufferSize - copy) * 4); }
         }
      }

      protected void CopyOutputs(float[][] outputs)
      {
         if (data == IntPtr.Zero) { return; }
         if (outputs == null) { return; }
         int outMax = Math.Min(outputOffsets.Length, outputs.Length);
         for (int i = 0; i < outMax; i++)
         {
            if (outputs[i] == null) { continue; }
            int copy = Math.Min(outputs[i].Length, bufferSize);
            if (copy > 0) { Marshal.Copy(IntPtr.Add(data, outputOffsets[i]), outputs[i], 0, copy); }
            if (copy < outputs.Length) { Array.Clear(outputs, copy, outputs.Length - copy); }
         }
         if (outputs.Length == 1 && outputOffsets.Length > 1 && outputs[0] != null)
         {
            if (mixBuffer == null || mixBuffer.Length < bufferSize) { mixBuffer = new float[bufferSize]; }
            Marshal.Copy(IntPtr.Add(data, outputOffsets[1]), mixBuffer, 0, bufferSize);
            float[] buffer = outputs[0];
            int copy = Math.Min(buffer.Length, bufferSize);
            for (int i = 0; i < copy; i++) { buffer[i] = (buffer[i] + mixBuffer[i]) * 0.5f; }
         }
         else
         {
            for (int i = outMax; i < outputs.Length; i++)
            {
               if (outputs[i] == null) { continue; }
               if (i == 1 && outputs[0] != null)
               {
                  Array.Copy(outputs[0], outputs[1], Math.Min(outputs[0].Length, outputs[1].Length));
               }
               else
               {
                  Array.Clear(outputs[i], 0, outputs[i].Length);
               }
            }
         }
      }

      public int GetSampleRate()
      {
         return (sampleRate);
      }

      public int GetBufferSize()
      {
         return (bufferSize);
      }

      public virtual LadspaInstance Clone(int sampleRate = 0, int bufferSize = 0)
      {
         if (sampleRate == 0) { sampleRate = this.sampleRate; }
         if (bufferSize == 0) { bufferSize = this.bufferSize; }
         LadspaInstance instance = LadspaDescriptor.CreateInstance(sampleRate, bufferSize);
         for (int i = 0; i < LadspaDescriptor.Ports.Length; i++) { instance.SetParameter(i, GetParameter(i)); }
         return (instance);
      }

      public void Activate()
      {
         if (handle == IntPtr.Zero) { return; }
         if (active) { return; }
         if (LadspaDescriptor.Activate != null) { LadspaDescriptor.Activate(handle); }
         active = true;
      }

      public void Deactivate()
      {
         if (handle == IntPtr.Zero) { return; }
         if (!active) { return; }
         if (LadspaDescriptor.Deactivate != null) { LadspaDescriptor.Deactivate(handle); }
         active = false;
      }

      public void ResetParameters()
      {
         if (handle == IntPtr.Zero || data == IntPtr.Zero) { return; }
         for (int i = 0; i < LadspaDescriptor.Ports.Length; i++)
         {
            if (LadspaDescriptor.Ports[i].PortType != LadspaPortType.Control) { continue; }
            paramBuffer[0] = LadspaDescriptor.Ports[i].DefaultValue;
            Marshal.Copy(paramBuffer, 0, IntPtr.Add(data, dataOffsets[i]), 1);
         }
      }

      public float GetParameter(int index)
      {
         if (handle == IntPtr.Zero) { return (0); }
         if (index < 0 || index >= dataOffsets.Length) { return (0); }
         if (LadspaDescriptor.Ports[index].PortType != LadspaPortType.Control) { return (0); }
         Marshal.Copy(IntPtr.Add(data, dataOffsets[index]), paramBuffer, 0, 1);
         return (paramBuffer[0]);
      }

      public void SetParameter(int index, float value)
      {
         if (handle == IntPtr.Zero) { return; }
         if (index < 0 || index >= dataOffsets.Length) { return; }
         if (LadspaDescriptor.Ports[index].PortType != LadspaPortType.Control) { return; }
         paramBuffer[0] = value;
         Marshal.Copy(paramBuffer, 0, IntPtr.Add(data, dataOffsets[index]), 1);
      }

      public void SetRunAddingGain(float gain)
      {
         if (handle == IntPtr.Zero) { return; }
         if (LadspaDescriptor.SetRunAddingGain == null) { return; }
         LadspaDescriptor.SetRunAddingGain(handle, gain);
      }

      public virtual bool Run(float[][] inputs, float[][] outputs)
      {
         if (handle == IntPtr.Zero) { return (false); }
         if (LadspaDescriptor.Run == null) { return (false); }
         if (!active) { Activate(); }
         CopyInputs(inputs);
         LadspaDescriptor.Run(handle, (uint)bufferSize);
         CopyOutputs(outputs);
         return (true);
      }

      public virtual bool RunAdding(float[][] inputs, float[][] outputs)
      {
         if (handle == IntPtr.Zero) { return (false); }
         if (LadspaDescriptor.RunAdding == null) { return (false); }
         if (!active) { Activate(); }
         CopyInputs(inputs);
         LadspaDescriptor.RunAdding(handle, (uint)bufferSize);
         CopyOutputs(outputs);
         return (true);
      }

      public virtual void Dispose()
      {
         if (handle == IntPtr.Zero) { return; }
         Deactivate();
         if (LadspaDescriptor.Cleanup != null) { LadspaDescriptor.Cleanup(handle); }
         if (data != IntPtr.Zero) { Marshal.FreeHGlobal(data); }
         data = IntPtr.Zero;
         handle = IntPtr.Zero;
      }
   }
}
