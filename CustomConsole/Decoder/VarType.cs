using System;
using System.Linq;

namespace CustomConsole
{
    public class VarType : IVarType
    {
        private static object _nextId = (uint)0;

        public VarType(IVarType[] implicits, string name, bool nullable)
        {
            lock (_nextId)
            {
                Id = (uint)_nextId;
                _nextId = Id + 1;
            }

            ImplicitTo = implicits;
            Nullable = nullable;
            Name = name;
        }

        public bool Nullable { get; }
        public string Name { get; }

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

        public static AnyType NonVoid { get; } = new AnyType(false, "NonVoid");
        public static AnyType Any { get; } = new AnyType(true, "Any");

        public static VarType Double { get; } = new VarType(null, "double", false);
        public static VarType Float { get; } = new VarType(new IVarType[] { Double }, "float", false);
        public static VarType Int { get; } = new VarType(new IVarType[] { Float, Double }, "int", false);

        public static VarType Bool { get; } = new VarType(null, "bool", false);

        public static VarType String { get; } = new VarType(null, "string", true);
        public static VarType Char { get; } = new VarType(new IVarType[] { String }, "char", false);

        public static VarType Vector2 { get; } = new VarType(null, "Vector2", false);
        public static VarType Vector3 { get; } = new VarType(new IVarType[] { Vector2 }, "Vector3", false);
        public static VarType Vector4 { get; } = new VarType(new IVarType[] { Vector3, Vector2 }, "Vector4", false);

        public static VarType Type { get; } = new VarType(null, "Type", true);
        public static VarType Variable { get; } = new VarType(null, "Variable", true);

        public class AnyType : IVarType
        {
            public AnyType(bool @void, string name)
            {
                IncludeVoid = @void;
                Name = name;
            }

            public string Name { get; }

            public bool Nullable => true;
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
