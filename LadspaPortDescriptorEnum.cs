using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   [Flags]
   public enum LadspaPortDescriptorEnum : int
   {
      LADSPA_PORT_INPUT = 0x1,
      LADSPA_PORT_OUTPUT = 0x2,
      LADSPA_PORT_CONTROL = 0x4,
      LADSPA_PORT_AUDIO = 0x8
   }
}
