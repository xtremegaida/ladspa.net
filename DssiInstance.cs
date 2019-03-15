using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   public class DssiInstance : LadspaInstance
   {
      public readonly DssiDescriptor DssiDescriptor;

      protected readonly List<DssiMidiEvent> eventList = new List<DssiMidiEvent>();
      protected IntPtr eventBuffer;
      protected int eventBufferSize;
      protected int eventBufferCount;

      internal DssiInstance(DssiDescriptor descriptor, IntPtr handle, int sampleRate, int bufferSize)
         : base(descriptor.LadspaDescriptor, handle, sampleRate, bufferSize)
      {
         DssiDescriptor = descriptor;
      }

      protected void PrepareEvents()
      {
         int i, offset, count = eventList.Count;
         int size = count * 28;
         if (eventBuffer == IntPtr.Zero || eventBufferSize < size)
         {
            if (eventBuffer != IntPtr.Zero) { Marshal.FreeHGlobal(eventBuffer); eventBuffer = IntPtr.Zero; }
            eventBuffer = Marshal.AllocHGlobal(eventBufferSize = size + 140);
         }
         for (i = 0, offset = 0, eventBufferCount = 0; i < count; i++)
         {
            DssiMidiEvent e = eventList[i];
            int command = e.MidiCommand & 0xf0;
            switch (command)
            {
               case 0x80: // Note Off
               case 0x90: // Note On
               case 0xA0: // Aftertouch
                  int code = (command == 0x80 || (e.MidiData1 == 0 && command == 0x90)) ? 7 : command == 0x90 ? 6 : 8;
                  Marshal.WriteInt32(eventBuffer, offset, code); offset += 4; // Type code
                  Marshal.WriteInt32(eventBuffer, offset, e.SampleIndex); offset += 4; // Ticks
                  Marshal.WriteInt64(eventBuffer, offset, 0); offset += 8; // Realtime timestamp + addresses (not used)
                  Marshal.WriteByte(eventBuffer, offset, (byte)(e.MidiCommand & 0x0f)); offset++; // Channel
                  Marshal.WriteByte(eventBuffer, offset, e.MidiData0); offset++; // Pitch
                  Marshal.WriteByte(eventBuffer, offset, e.MidiData1 == 0 ? (byte)64 : e.MidiData1); offset++; // Velocity
                  Marshal.WriteByte(eventBuffer, offset, 0); offset++; // Off Velocity  (not used)
                  Marshal.WriteInt64(eventBuffer, offset, 0); offset += 8; // Duration (not used)
                  eventBufferCount++;
                  break;
               
               case 0xB0: // Controller
                  Marshal.WriteInt32(eventBuffer, offset, 10); offset += 4; // Type code
                  Marshal.WriteInt32(eventBuffer, offset, e.SampleIndex); offset += 4; // Ticks
                  Marshal.WriteInt64(eventBuffer, offset, 0); offset += 8; // Realtime timestamp + addresses (not used)
                  Marshal.WriteInt32(eventBuffer, offset, e.MidiCommand & 0x0f); offset += 4; // Channel + Unused
                  Marshal.WriteInt32(eventBuffer, offset, e.MidiData0); offset += 4; // Control parameter
                  Marshal.WriteInt32(eventBuffer, offset, e.MidiData1); offset += 4; // Value
                  eventBufferCount++;
                  break;

               case 0xC0: // Change program
                  SelectProgram((uint)(e.MidiCommand & 0x0f), e.MidiData0);
                  break;

               case 0xD0: // Channel pressure
                  Marshal.WriteInt32(eventBuffer, offset, 12); offset += 4; // Type code
                  Marshal.WriteInt32(eventBuffer, offset, e.SampleIndex); offset += 4; // Ticks
                  Marshal.WriteInt64(eventBuffer, offset, 0); offset += 8; // Realtime timestamp + addresses (not used)
                  Marshal.WriteInt32(eventBuffer, offset, e.MidiCommand & 0x0f); offset += 4; // Channel + Unused
                  Marshal.WriteInt32(eventBuffer, offset, 0); offset += 4; // Control parameter
                  Marshal.WriteInt32(eventBuffer, offset, e.MidiData0); offset += 4; // Value
                  eventBufferCount++;
                  break;

               case 0xE0: // Pitchbend
                  Marshal.WriteInt32(eventBuffer, offset, 13); offset += 4; // Type code
                  Marshal.WriteInt32(eventBuffer, offset, e.SampleIndex); offset += 4; // Ticks
                  Marshal.WriteInt64(eventBuffer, offset, 0); offset += 8; // Realtime timestamp + addresses (not used)
                  Marshal.WriteInt32(eventBuffer, offset, e.MidiCommand & 0x0f); offset += 4; // Channel + Unused
                  Marshal.WriteInt32(eventBuffer, offset, 0); offset += 4; // Control parameter
                  Marshal.WriteInt32(eventBuffer, offset, (short)(((int)e.MidiData0 << 2) + ((int)e.MidiData1 << 9)) >> 2); offset += 4; // Value
                  eventBufferCount++;
                  break;
            }
         }
         eventList.Clear();
      }

      public string Configure(string key, string value)
      {
         if (handle == IntPtr.Zero || DssiDescriptor.Configure == null) { return (null); }
         return (DssiDescriptor.Configure(handle, key, value));
      }

      public void SelectProgram(uint bank, uint program)
      {
         if (handle == IntPtr.Zero || DssiDescriptor.SelectProgram == null) { return; }
         DssiDescriptor.SelectProgram(handle, bank, program);
      }

      public int GetMidiControllerForPort(uint port)
      {
         if (handle == IntPtr.Zero || DssiDescriptor.GetMidiControllerForPort == null) { return (0); }
         return (DssiDescriptor.GetMidiControllerForPort(handle, port));
      }

      public void AddSynthEvents(params DssiMidiEvent[] events)
      {
         if (events != null && events.Length > 0)
         {
            eventList.AddRange(events);
         }
      }

      public bool Run(float[][] outputs)
      {
         if (handle != IntPtr.Zero)
         {
            if (DssiDescriptor.RunSynth != null)
            {
               PrepareEvents();
               DssiDescriptor.RunSynth(handle, (uint)bufferSize, eventBuffer, (uint)eventBufferCount);
               CopyOutputs(outputs);
               return (true);
            }
            else if (LadspaDescriptor.Run != null)
            {
               LadspaDescriptor.Run(handle, (uint)bufferSize);
               CopyOutputs(outputs);
               return (true);
            }
         }
         if (eventList.Count > 0) { eventList.Clear(); }
         return (false);
      }

      public bool RunAdding(float[][] outputs)
      {
         if (handle != IntPtr.Zero)
         {
            if (DssiDescriptor.RunSynthAdding != null)
            {
               PrepareEvents();
               DssiDescriptor.RunSynthAdding(handle, (uint)bufferSize, eventBuffer, (uint)eventBufferCount);
               CopyOutputs(outputs);
               return (true);
            }
            else if (LadspaDescriptor.RunAdding != null)
            {
               LadspaDescriptor.RunAdding(handle, (uint)bufferSize);
               CopyOutputs(outputs);
               return (true);
            }
         }
         if (eventList.Count > 0) { eventList.Clear(); }
         return (false);
      }

      public override bool Run(float[][] inputs, float[][] outputs)
      {
         if (handle != IntPtr.Zero) { CopyInputs(inputs); }
         return (Run(outputs));
      }

      public override bool RunAdding(float[][] inputs, float[][] outputs)
      {
         if (handle != IntPtr.Zero) { CopyInputs(inputs); }
         return (RunAdding(outputs));
      }

      public override LadspaInstance Clone(int sampleRate = 0, int bufferSize = 0)
      {
         if (sampleRate == 0) { sampleRate = this.sampleRate; }
         if (bufferSize == 0) { bufferSize = this.bufferSize; }
         DssiInstance instance = DssiDescriptor.CreateInstance(sampleRate, bufferSize);
         for (int i = 0; i < LadspaDescriptor.Ports.Length; i++) { instance.SetParameter(i, GetParameter(i)); }
         return (instance);
      }

      public override void Dispose()
      {
         base.Dispose();
         if (eventBuffer != IntPtr.Zero)
         {
            Marshal.FreeHGlobal(eventBuffer);
            eventBuffer = IntPtr.Zero;
         }
      }
   }
}
