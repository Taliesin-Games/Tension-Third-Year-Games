using System;
namespace BMD
{
    public readonly struct ModulePair : IEquatable<ModulePair>
    {
        public readonly Type A;
        public readonly Type B;

        public ModulePair(Type a, Type b)
        {
            if (a == b)
            {
                A = a;
                B = b;
            }
            else if (a.FullName.CompareTo(b.FullName) < 0)
            {
                A = a;
                B = b;
            }
            else
            {
                A = b;
                B = a;
            }
        }

        public bool Equals(ModulePair other) => A == other.A && B == other.B;

        public override int GetHashCode() => HashCode.Combine(A, B);
    }
}
