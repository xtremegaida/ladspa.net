using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   [StructLayout(LayoutKind.Sequential)]
   internal struct LadspaDescriptorStruct
   {
      public uint UniqueID;
      public string Label;
      public LadspaPropertiesEnum Properties;
      public string Name;
      public string Maker;
      public string Copyright;
      public uint PortCount;
      public IntPtr PortDescriptors;  // LadspaPortDescriptorEnum[]
      public IntPtr PortNames;        // string[]
      public IntPtr PortRangeHints;   // LadspaPortRangeHintsStruct[]
      public IntPtr ImplementationData;
      public IntPtr Instantiate;      // IntPtr (LadspaDescriptorStruct* this, uint sampleRate)
      public IntPtr ConnectPort;      // void (IntPtr handle, uint port, float* data)
      public IntPtr Activate;         // void (IntPtr handle)
      public IntPtr Run;              // void (IntPtr handle, uint sampleCount)
      public IntPtr RunAdding;        // void (IntPtr handle, uint sampleCount)
      public IntPtr SetRunAddingGain; // void (IntPtr handle, float gain)
      public IntPtr Deactivate;       // void (IntPtr handle)
      public IntPtr Cleanup;          // void (IntPtr handle)
   }

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate IntPtr LadspaDescriptorInstantiateCallback(IntPtr thisPtr, uint sampleRate);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void LadspaDescriptorConnectPortCallback(IntPtr handle, uint port, IntPtr data);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void LadspaDescriptorCallback(IntPtr handle);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void LadspaDescriptorRunCallback(IntPtr handle, uint sampleCount);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void LadspaDescriptorSetRunGainCallback(IntPtr handle, float gain);
}
