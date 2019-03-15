using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   public class LadspaDescriptor
   {
      public readonly LadspaLibraryContext Library;
      public readonly uint Index;
      public readonly uint UniqueID;
      public readonly LadspaPropertiesEnum Properties;
      public readonly string Label;
      public readonly string Name;
      public readonly string Maker;
      public readonly string Copyright;
      public readonly LadspaPort[] Ports;

      internal readonly IntPtr DataHandle;
      internal readonly LadspaDescriptorInstantiateCallback Instantiate;
      internal readonly LadspaDescriptorConnectPortCallback ConnectPort;
      internal readonly LadspaDescriptorCallback Activate;
      internal readonly LadspaDescriptorRunCallback Run;
      internal readonly LadspaDescriptorRunCallback RunAdding;
      internal readonly LadspaDescriptorSetRunGainCallback SetRunAddingGain;
      internal readonly LadspaDescriptorCallback Deactivate;
      internal readonly LadspaDescriptorCallback Cleanup;

      internal LadspaDescriptor(LadspaLibraryContext library, uint index, IntPtr dataPtr)
      {
         LadspaDescriptorStruct data = (LadspaDescriptorStruct)Marshal.PtrToStructure(dataPtr, typeof(LadspaDescriptorStruct));
         Library = library;
         DataHandle = dataPtr;
         Index = index;
         UniqueID = data.UniqueID;
         Properties = data.Properties;
         Label = data.Label;
         Name = data.Name;
         Maker = data.Maker;
         Copyright = data.Copyright;
         Ports = new LadspaPort[data.PortCount];
         int hintsSize = Marshal.SizeOf(typeof(LadspaPortRangeHintsStruct));
         int[] types = new int[data.PortCount];
         IntPtr[] names = new IntPtr[data.PortCount];
         if (data.PortDescriptors != IntPtr.Zero) { Marshal.Copy(data.PortDescriptors, types, 0, types.Length); }
         if (data.PortNames != IntPtr.Zero) { Marshal.Copy(data.PortNames, names, 0, names.Length); }
         for (int i = 0; i < data.PortCount; i++)
         {
            string name = names[i] != IntPtr.Zero ? Marshal.PtrToStringAnsi(names[i]) : null;
            LadspaPortRangeHintsStruct hints = data.PortRangeHints == IntPtr.Zero ? new LadspaPortRangeHintsStruct() :
               (LadspaPortRangeHintsStruct)Marshal.PtrToStructure(IntPtr.Add(data.PortRangeHints, hintsSize * (int)i), typeof(LadspaPortRangeHintsStruct));
            Ports[i] = new LadspaPort(this, i, (LadspaPortDescriptorEnum)types[i], hints.Hints, name, hints.LowerBound, hints.UpperBound);
         }
         if (data.Instantiate != IntPtr.Zero) { Instantiate = (LadspaDescriptorInstantiateCallback)Marshal.GetDelegateForFunctionPointer(data.Instantiate, typeof(LadspaDescriptorInstantiateCallback)); }
         if (data.ConnectPort != IntPtr.Zero) { ConnectPort = (LadspaDescriptorConnectPortCallback)Marshal.GetDelegateForFunctionPointer(data.ConnectPort, typeof(LadspaDescriptorConnectPortCallback)); }
         if (data.Activate != IntPtr.Zero) { Activate = (LadspaDescriptorCallback)Marshal.GetDelegateForFunctionPointer(data.Activate, typeof(LadspaDescriptorCallback)); }
         if (data.Run != IntPtr.Zero) { Run = (LadspaDescriptorRunCallback)Marshal.GetDelegateForFunctionPointer(data.Run, typeof(LadspaDescriptorRunCallback)); }
         if (data.RunAdding != IntPtr.Zero) { RunAdding = (LadspaDescriptorRunCallback)Marshal.GetDelegateForFunctionPointer(data.RunAdding, typeof(LadspaDescriptorRunCallback)); }
         if (data.SetRunAddingGain != IntPtr.Zero) { SetRunAddingGain = (LadspaDescriptorSetRunGainCallback)Marshal.GetDelegateForFunctionPointer(data.SetRunAddingGain, typeof(LadspaDescriptorSetRunGainCallback)); }
         if (data.Deactivate != IntPtr.Zero) { Deactivate = (LadspaDescriptorCallback)Marshal.GetDelegateForFunctionPointer(data.Deactivate, typeof(LadspaDescriptorCallback)); }
         if (data.Cleanup != IntPtr.Zero) { Cleanup = (LadspaDescriptorCallback)Marshal.GetDelegateForFunctionPointer(data.Cleanup, typeof(LadspaDescriptorCallback)); }
      }

      public LadspaInstance CreateInstance(int sampleRate, int bufferSize = 1024)
      {
         IntPtr handle = Instantiate != null ? Instantiate(DataHandle, (uint)sampleRate) : IntPtr.Zero;
         if (handle == IntPtr.Zero) { throw new Exception("Failed to create instance."); }
         return (new LadspaInstance(this, handle, sampleRate, bufferSize));
      }
   }
}
