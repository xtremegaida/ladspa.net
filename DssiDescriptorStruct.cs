using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   [StructLayout(LayoutKind.Sequential)]
   internal struct DssiDescriptorStruct
   {
      public uint APIVersion;
      public IntPtr LadspaDescriptor;         // LadspaDescriptorStruct
      public IntPtr Configure;                // string (IntPtr handle, string key, string value)
      public IntPtr GetProgram;               // IntPtr (IntPtr handle, uint index)
      public IntPtr SelectProgram;            // void (IntPtr handle, uint bank, uint program)
      public IntPtr GetMidiControllerForPort; // int (IntPtr handle, uint port)
      public IntPtr RunSynth;                 // void (IntPtr thisPtr, uint sampleCount, IntPtr events, uint eventCount)
      public IntPtr RunSynthAdding;           // void (IntPtr thisPtr, uint sampleCount, IntPtr events, uint eventCount)
      public IntPtr RunMultiSynth;            // void (uint thisCount, IntPtr thisPtrs, uint sampleCount, IntPtr events, IntPtr eventCounts)
      public IntPtr RunMultiSynthAdding;      // void (uint thisCount, IntPtr thisPtrs, uint sampleCount, IntPtr events, IntPtr eventCounts)
   }

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate string DssiDescriptorConfigureCallback(IntPtr thisPtr, string key, string value);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate IntPtr DssiDescriptorGetProgramCallback(IntPtr thisPtr, uint index);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void DssiDescriptorSelectProgramCallback(IntPtr thisPtr, uint bank, uint program);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate int DssiDescriptorGetMidiControllerForPortCallback(IntPtr thisPtr, uint port);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void DssiDescriptorRunSynthCallback(IntPtr thisPtr, uint sampleCount, IntPtr events, uint eventCount);
   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void DssiDescriptorRunMultiSynthCallback(uint thisCount, IntPtr thisPtrs, uint sampleCount, IntPtr events, IntPtr eventCounts);
}
