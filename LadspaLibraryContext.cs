using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
    public class LadspaLibraryContext : IDisposable
    {
       [DllImport("Kernel32.dll")]
       private static extern IntPtr LoadLibrary(string path);

       [DllImport("Kernel32.dll")]
       private static extern void FreeLibrary(IntPtr hModule);

       [DllImport("Kernel32.dll")]
       private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

       [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
       private delegate IntPtr LadspaDescriptorCallback(uint index);

       private IntPtr library;

       public readonly LadspaDescriptor[] LadspaDescriptors;
       public readonly DssiDescriptor[] DssiDescriptors;

       public LadspaLibraryContext(string fileName)
       {
          library = LoadLibrary(fileName);
          if (library == IntPtr.Zero) { throw new Exception("Failed to load library - file not found or not a valid library."); }
          try
          {
             IntPtr func = GetProcAddress(library, "ladspa_descriptor");
             IntPtr dssiFunc = GetProcAddress(library, "dssi_descriptor");
             if (func == IntPtr.Zero && dssiFunc == IntPtr.Zero)
             {
                throw new Exception("Not a LADSPA plugin: ladspa_descriptor not found.");
             }
             List<LadspaDescriptor> descriptors = new List<LadspaDescriptor>();
             List<DssiDescriptor> dssiDescriptors = new List<DssiDescriptor>();
             if (func != IntPtr.Zero)
             {
                LadspaDescriptorCallback callback = (LadspaDescriptorCallback)Marshal.GetDelegateForFunctionPointer(func, typeof(LadspaDescriptorCallback));
                for (uint index = 0; true; index++)
                {
                   IntPtr data = callback(index);
                   if (data == IntPtr.Zero) { break; }
                   descriptors.Add(new LadspaDescriptor(this, index, data));
                }
             }
             if (dssiFunc != IntPtr.Zero)
             {
                LadspaDescriptorCallback callback = (LadspaDescriptorCallback)Marshal.GetDelegateForFunctionPointer(dssiFunc, typeof(LadspaDescriptorCallback));
                for (uint index = 0; true; index++)
                {
                   IntPtr data = callback(index);
                   if (data == IntPtr.Zero) { break; }
                   dssiDescriptors.Add(new DssiDescriptor(this, index, data));
                }
             }
             LadspaDescriptors = descriptors.ToArray();
             DssiDescriptors = dssiDescriptors.ToArray();
          }
          catch
          {
             FreeLibrary(library);
             throw;
          }
       }

       public void Dispose()
       {
          if (library == IntPtr.Zero) { return; }
          FreeLibrary(library);
          library = IntPtr.Zero;
       }
    }
}
