using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   [StructLayout(LayoutKind.Sequential)]
   public struct DssiProgramDescriptor
   {
      public uint Bank;
      public uint Program;
      public string Name;
   }
}
