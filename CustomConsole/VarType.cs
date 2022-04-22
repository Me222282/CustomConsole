using System;
using System.Linq;

namespace CustomConsole
{
    public class VarType : IVarType
    {
        private static object _nextId = (uint)0;

        public VarType(IVarType[] implicits)
        {
            lock (_nextId)
            {
                Id = (uint)_nextId;
                _nextId = Id + 1;
            }

            ImplicitTo = implicits;
        }

        public uint Id { get; }
        /// <summary>
        /// All types this type can be implicitly converted to.
        /// </summary>
        public IVarType[] ImplicitTo { get; }

        public bool Compatible(IVarType type)
        {
            if (type is AnyType) { return true; }

            // Equal types are compatible
            if (Equals(type)) { return true; }

            // Can this Type be casted to "type" Type
            return ImplicitTo != null &&
                type != null &&
                ImplicitTo.Contains(type);
        }
        public override bool Equals(object obj) => obj is VarType vt && vt.Id == Id;
        bool IVarType.Equals(IVarType type) => Equals(type);

        public override int GetHashCode() => HashCode.Combine(Id, ImplicitTo);

        public static VarType Void { get; } = null;

        public static IVarType NonVoid { get; } = new AnyType(false);
        public static IVarType Any { get; } = new AnyType(true);

        public static VarType Double { get; } = new VarType(null);
        public static VarType Float { get; } = new VarType(new IVarType[] { Double });
        public static VarType Int { get; } = new VarType(new IVarType[] { Float, Double });

        public static VarType Bool { get; } = new VarType(null);

        public static VarType String { get; } = new VarType(null);
        public static VarType Char { get; } = new VarType(new IVarType[] { String });

        public static VarType Vector2 { get; } = new VarType(null);
        public static VarType Vector3 { get; } = new VarType(new IVarType[] { Vector2 });
        public static VarType Vector4 { get; } = new VarType(new IVarType[] { Vector3, Vector2 });

        private class AnyType : IVarType
        {
            public AnyType(bool @void)
            {
                IncludeVoid = @void;
            }

            public bool IncludeVoid { get; }
            public IVarType[] ImplicitTo => null;

            public bool Compatible(IVarType type)
            {
                return !(type == null && !IncludeVoid);
            }

            public override bool Equals(object obj) => obj is AnyType at && at.IncludeVoid == IncludeVoid;
            bool IVarType.Equals(IVarType type) => Equals(type);

            public override int GetHashCode() => HashCode.Combine(IncludeVoid);
        }
    }
}
