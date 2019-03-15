using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   [Flags]
   public enum LadspaPropertiesEnum : int
   {
      LADSPA_PROPERTY_REALTIME = 0x1,
      LADSPA_PROPERTY_INPLACE_BROKEN = 0x2,
      LADSPA_PROPERTY_HARD_RT_CAPABLE = 0x4
   }
}
