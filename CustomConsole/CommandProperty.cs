using System;

namespace CustomConsole
{
    public struct CommandProperty
    {
        public CommandProperty(string name)
        {
            Name = name ?? "";
            DataType = VarType.Bool;
        }
        public CommandProperty(string name, IVarType dataType)
        {
            Name = name ?? "";
            DataType = dataType;
        }

        public string Name { get; }
        public IVarType DataType { get; }

        public override bool Equals(object obj)
        {
            return obj is CommandProperty cp &&
                Name == cp.Name &&
                DataType == cp.DataType;
        }
        public override int GetHashCode() => HashCode.Combine(Name, DataType);

        public static bool operator ==(CommandProperty l, CommandProperty r) => l.Equals(r);
        public static bool operator !=(CommandProperty l, CommandProperty r) => !l.Equals(r);
    }
}
