using UnityEngine;

namespace BMD
{
    public static class CharacterModuleValidator
    {
        public static bool CheckModuleCompatibility(GameObject target, Component addedComponent)
        {
            if (addedComponent is not ICharacterModule) return true;

            var modules = target.GetComponents<Component>();

            foreach (var other in modules)
            {
                if (other == addedComponent) continue;

                if (other is not ICharacterModule) continue;

                if (CharacterModuleCompatibility.TryGetConflict(addedComponent.GetType(), other.GetType(), out var warnType, out var warnMessage))
                {
                    return !EmitWarning(warnType, warnMessage, addedComponent, other);
                }
            }

            return true;
        }

        static bool EmitWarning(IncompatibleWarnType type, string warnMessage, Component added, Component other)
        {
            string msg = $"{added.GetType().Name} conflicts with {other.GetType().Name}. {warnMessage}";
            bool isError = false;
            switch (type)
            {
                case IncompatibleWarnType.Info:
                    Debug.Log(msg, added);
                    break;

                case IncompatibleWarnType.Warning:
                    Debug.LogWarning(msg, added);
                    break;

                case IncompatibleWarnType.Error:
                    Debug.LogError(msg, added);
                    isError = true;
                    break;
            }

            return isError;
        }
    }

}
