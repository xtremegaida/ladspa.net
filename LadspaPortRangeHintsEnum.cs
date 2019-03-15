using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   [Flags]
   public enum LadspaPortRangeHintsEnum : int
   {
      LADSPA_HINT_BOUNDED_BELOW = 0x1,
      LADSPA_HINT_BOUNDED_ABOVE = 0x2,
      LADSPA_HINT_TOGGLED = 0x4,
      LADSPA_HINT_SAMPLE_RATE = 0x8,
      LADSPA_HINT_LOGARITHMIC = 0x10,
      LADSPA_HINT_INTEGER = 0x20,
      LADSPA_HINT_DEFAULT_MASK = 0x3C0,
      LADSPA_HINT_DEFAULT_NONE = 0x0,
      LADSPA_HINT_DEFAULT_MINIMUM = 0x40,
      LADSPA_HINT_DEFAULT_LOW = 0x80,
      LADSPA_HINT_DEFAULT_MIDDLE = 0xC0,
      LADSPA_HINT_DEFAULT_HIGH = 0x100,
      LADSPA_HINT_DEFAULT_MAXIMUM = 0x140,
      LADSPA_HINT_DEFAULT_0 = 0x200,
      LADSPA_HINT_DEFAULT_1 = 0x240,
      LADSPA_HINT_DEFAULT_100 = 0x280,
      LADSPA_HINT_DEFAULT_440 = 0x2C0
   }
}
