using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LADSPA.NET
{
   public struct DssiMidiEvent
   {
      public int SampleIndex;
      public byte MidiCommand;
      public byte MidiData0;
      public byte MidiData1;
      public byte MidiData2;
   }
}
