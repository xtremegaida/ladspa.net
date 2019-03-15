using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LADSPA.NET
{
   public class LadspaPort
   {
      public readonly LadspaDescriptor Descriptor;
      public readonly int Index;
      public readonly LadspaPortDescriptorEnum Type;
      public readonly LadspaPortRangeHintsEnum Hints;
      public readonly string Name;

      public readonly LadspaPortDirection PortDirection;
      public readonly LadspaPortType PortType;
      public readonly float DefaultValue;
      public readonly float MinimumValue;
      public readonly float MaximumValue;
      public readonly bool IsLogarithmic;
      public readonly bool IsInteger;
      public readonly bool IsSampleRateMultiple;
      public readonly bool IsToggled;

      internal LadspaPort(LadspaDescriptor descriptor, int index,
         LadspaPortDescriptorEnum type, LadspaPortRangeHintsEnum hints, string name,
         float minBound, float maxBound)
      {
         Descriptor = descriptor;
         Index = index;
         Type = type;
         Name = name;
         Hints = hints;
         PortType = type.HasFlag(LadspaPortDescriptorEnum.LADSPA_PORT_CONTROL) ? LadspaPortType.Control :
            type.HasFlag(LadspaPortDescriptorEnum.LADSPA_PORT_AUDIO) ? LadspaPortType.Audio : LadspaPortType.Unknown;
         PortDirection = type.HasFlag(LadspaPortDescriptorEnum.LADSPA_PORT_INPUT) ? LadspaPortDirection.Input :
            type.HasFlag(LadspaPortDescriptorEnum.LADSPA_PORT_OUTPUT) ? LadspaPortDirection.Output : LadspaPortDirection.Unknown;
         IsLogarithmic = hints.HasFlag(LadspaPortRangeHintsEnum.LADSPA_HINT_LOGARITHMIC);
         IsInteger = hints.HasFlag(LadspaPortRangeHintsEnum.LADSPA_HINT_INTEGER);
         IsSampleRateMultiple = hints.HasFlag(LadspaPortRangeHintsEnum.LADSPA_HINT_SAMPLE_RATE);
         IsToggled = hints.HasFlag(LadspaPortRangeHintsEnum.LADSPA_HINT_TOGGLED);
         if (hints.HasFlag(LadspaPortRangeHintsEnum.LADSPA_HINT_BOUNDED_BELOW)) { MinimumValue = minBound; } else { MinimumValue = 0; }
         if (hints.HasFlag(LadspaPortRangeHintsEnum.LADSPA_HINT_BOUNDED_ABOVE)) { MaximumValue = maxBound; } else { MaximumValue = 1; }
         switch (hints & LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_MASK)
         {
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_MINIMUM: DefaultValue = MinimumValue; break;
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_LOW:
               DefaultValue = IsLogarithmic ? (float)Math.Exp((Math.Log(MinimumValue) * 3 + Math.Log(MaximumValue)) * 0.25) :
                  (MinimumValue * 3 + MaximumValue) * 0.25f;
               break;
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_MIDDLE:
               DefaultValue = IsLogarithmic ? (float)Math.Exp((Math.Log(MinimumValue) + Math.Log(MaximumValue)) * 0.5) :
                  (MinimumValue + MaximumValue) * 0.5f;
               break;
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_HIGH:
               DefaultValue = IsLogarithmic ? (float)Math.Exp((Math.Log(MinimumValue) + Math.Log(MaximumValue) * 3) * 0.25) :
                  (MinimumValue + MaximumValue * 3) * 0.25f;
               break;
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_MAXIMUM: DefaultValue = MaximumValue; break;
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_0: DefaultValue = 0; break;
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_1: DefaultValue = 1; break;
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_100: DefaultValue = 100; break;
            case LadspaPortRangeHintsEnum.LADSPA_HINT_DEFAULT_440: DefaultValue = 440; break;
            default: break;
         }
      }
   }

   public enum LadspaPortDirection
   {
      Unknown, Input, Output
   }

   public enum LadspaPortType
   {
      Unknown, Audio, Control
   }
}
