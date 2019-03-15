using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   public class DssiDescriptor
   {
      public readonly LadspaLibraryContext Library;
      public readonly LadspaDescriptor LadspaDescriptor;
      public readonly bool IsSynth;

      internal readonly IntPtr DataHandle;
      internal readonly DssiDescriptorConfigureCallback Configure;
      internal readonly DssiDescriptorGetProgramCallback GetProgram;
      internal readonly DssiDescriptorSelectProgramCallback SelectProgram;
      internal readonly DssiDescriptorGetMidiControllerForPortCallback GetMidiControllerForPort;
      internal readonly DssiDescriptorRunSynthCallback RunSynth;
      internal readonly DssiDescriptorRunSynthCallback RunSynthAdding;
      internal readonly DssiDescriptorRunMultiSynthCallback RunMultiSynth;
      internal readonly DssiDescriptorRunMultiSynthCallback RunMultiSynthAdding;

      private DssiProgramDescriptor[] _programs;
      public DssiProgramDescriptor[] Programs
      {
         get
         {
            lock (this)
            {
               if (_programs == null)
               {
                  if (LadspaDescriptor.Instantiate != null && LadspaDescriptor.Cleanup != null)
                  {
                     IntPtr handle = LadspaDescriptor.Instantiate(LadspaDescriptor.DataHandle, 44100);
                     if (handle != IntPtr.Zero)
                     {
                        try { _programs = LoadProgramsFromHandle(handle); }
                        finally { LadspaDescriptor.Cleanup(handle); }
                     }
                  }
                  if (_programs == null) { _programs = new DssiProgramDescriptor[0]; }
               }
               return (_programs);
            }
         }
      }

      internal DssiDescriptor(LadspaLibraryContext library, uint index, IntPtr dataPtr)
      {
         DssiDescriptorStruct data = (DssiDescriptorStruct)Marshal.PtrToStructure(dataPtr, typeof(DssiDescriptorStruct));
         Library = library;
         DataHandle = dataPtr;
         LadspaDescriptor = new LadspaDescriptor(library, index, data.LadspaDescriptor);
         if (data.Configure != IntPtr.Zero) { Configure = (DssiDescriptorConfigureCallback)Marshal.GetDelegateForFunctionPointer(data.Configure, typeof(DssiDescriptorConfigureCallback)); }
         if (data.GetProgram != IntPtr.Zero) { GetProgram = (DssiDescriptorGetProgramCallback)Marshal.GetDelegateForFunctionPointer(data.GetProgram, typeof(DssiDescriptorGetProgramCallback)); }
         if (data.SelectProgram != IntPtr.Zero) { SelectProgram = (DssiDescriptorSelectProgramCallback)Marshal.GetDelegateForFunctionPointer(data.SelectProgram, typeof(DssiDescriptorSelectProgramCallback)); }
         if (data.GetMidiControllerForPort != IntPtr.Zero) { GetMidiControllerForPort = (DssiDescriptorGetMidiControllerForPortCallback)Marshal.GetDelegateForFunctionPointer(data.GetMidiControllerForPort, typeof(DssiDescriptorGetMidiControllerForPortCallback)); }
         if (data.RunSynth != IntPtr.Zero) { RunSynth = (DssiDescriptorRunSynthCallback)Marshal.GetDelegateForFunctionPointer(data.RunSynth, typeof(DssiDescriptorRunSynthCallback)); }
         if (data.RunSynthAdding != IntPtr.Zero) { RunSynthAdding = (DssiDescriptorRunSynthCallback)Marshal.GetDelegateForFunctionPointer(data.RunSynthAdding, typeof(DssiDescriptorRunSynthCallback)); }
         if (data.RunMultiSynth != IntPtr.Zero) { RunMultiSynth = (DssiDescriptorRunMultiSynthCallback)Marshal.GetDelegateForFunctionPointer(data.RunMultiSynth, typeof(DssiDescriptorRunMultiSynthCallback)); }
         if (data.RunMultiSynthAdding != IntPtr.Zero) { RunMultiSynthAdding = (DssiDescriptorRunMultiSynthCallback)Marshal.GetDelegateForFunctionPointer(data.RunMultiSynthAdding, typeof(DssiDescriptorRunMultiSynthCallback)); }
         IsSynth = data.RunSynth != null;
      }

      public DssiInstance CreateInstance(int sampleRate, int bufferSize = 1024)
      {
         IntPtr handle = LadspaDescriptor.Instantiate != null ? LadspaDescriptor.Instantiate(LadspaDescriptor.DataHandle, (uint)sampleRate) : IntPtr.Zero;
         if (handle == IntPtr.Zero) { throw new Exception("Failed to create instance."); }
         lock (this) { if (_programs == null) { _programs = LoadProgramsFromHandle(handle); } }
         return (new DssiInstance(this, handle, sampleRate, bufferSize));
      }

      private DssiProgramDescriptor[] LoadProgramsFromHandle(IntPtr handle)
      {
         List<DssiProgramDescriptor> programs = new List<DssiProgramDescriptor>();
         if (GetProgram != null)
         {
            for (uint index = 0; true; index++)
            {
               IntPtr programPtr = GetProgram(handle, index);
               if (programPtr == IntPtr.Zero) { break; }
               programs.Add((DssiProgramDescriptor)Marshal.PtrToStructure(programPtr, typeof(DssiProgramDescriptor)));
            }
         }
         return (programs.ToArray());
      }
   }
}
