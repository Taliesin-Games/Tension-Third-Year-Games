using System;
using System.Collections.Generic;
namespace BMD
{
    public static class CharacterModuleCompatibility
    {
        static readonly Dictionary<ModulePair, (IncompatibleWarnType, string)> _rules
            = new()
            {
                {
                    new ModulePair(typeof(CharacterSimpleFlightModule), typeof(CharacterFlightModule)), 
                    (IncompatibleWarnType.Error, "Simple flight modile incompatible with flight module, please remove this first.")
                },
                {
                    new ModulePair(typeof(CharacterSimpleFlightModule), typeof(CharacterMovementModule)), 
                    (IncompatibleWarnType.Warning, "Simple flight modile incompatible with movement module, custom state management will be required.")
                },
                {
                    new ModulePair(typeof(CharacterSimpleFlightModule), typeof(CharacterAnimatorModule)), 
                    (IncompatibleWarnType.Warning, "Simple flight modile does not drive animations, and may be inconsistent with the animator module.")
                }
            };

        public static bool TryGetConflict(Type a,Type b, out IncompatibleWarnType warnType, out string warnMessage)
        {
            if (_rules.TryGetValue(new ModulePair(a, b), out var result))
            {
                warnType = result.Item1;
                warnMessage = result.Item2;
                return true;
            }

            warnType = default;
            warnMessage = null;
            return false;
        }
    }
}
