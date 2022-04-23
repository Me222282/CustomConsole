using System;

namespace CustomConsole
{
    public class TypedVar : IVarType
    {
        public TypedVar(uint id)
        {
            Id = id;
            Name = $"Type T:{id}";
        }

        public uint Id { get; }
        public bool Nullable => false;
        public string Name { get; }
        public IVarType[] ImplicitTo => null;

        public bool Compatible(IVarType type)
        {
            if (type == VarType.Any || type == VarType.NonVoid) { return true; }

            return Equals(type);
        }

        bool IVarType.Equals(IVarType type) => Equals(type);
        public override bool Equals(object obj)
        {
            return obj is TypedVar tv &&
                Id == tv.Id;
        }

        public override int GetHashCode() => HashCode.Combine(Id);

        public static TypedVar T1 { get; } = new TypedVar(0);
        public static TypedVar T2 { get; } = new TypedVar(1);
        public static TypedVar T3 { get; } = new TypedVar(2);
    }
}
