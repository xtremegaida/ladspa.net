using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   [StructLayout(LayoutKind.Sequential)]
   internal struct LadspaPortRangeHintsStruct
   {
      public LadspaPortRangeHintsEnum Hints;
      public float LowerBound;
      public float UpperBound;
   }
}
