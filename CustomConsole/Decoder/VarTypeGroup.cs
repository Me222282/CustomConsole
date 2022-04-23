using System;
using System.Linq;
using System.Text;

namespace CustomConsole
{
    public class VarTypeGroup : IVarType
    {
        public VarTypeGroup(params IVarType[] types)
        {
            PossibleTypes = types ?? new IVarType[0];

            StringBuilder name = new StringBuilder();

            for (int i = 0; i < PossibleTypes.Length; i++)
            {
                name.Append(PossibleTypes[i].Name);

                // Not last iteration of loop
                if (PossibleTypes.Length != (i + 1))
                {
                    name.Append(' ');
                    continue;
                }

                break;
            }

            Name = name.ToString();
        }

        public IVarType[] PossibleTypes { get; }

        public bool Nullable => false;
        public string Name { get; }
        IVarType[] IVarType.ImplicitTo => PossibleTypes;

        public bool Compatible(IVarType type)
        {
            if (type == VarType.Any) { return true; }

            if (type is VarTypeGroup vtg && Equals(vtg)) { return true; }

            for (int i = 0; i < PossibleTypes.Length; i++)
            {
                if (!PossibleTypes[i].Compatible(type)) { continue; }

                return true;
            }

            return false;
        }
        public override bool Equals(object obj)
        {
            if (obj is VarTypeGroup vtg)
            {
                return PossibleTypes == vtg.PossibleTypes;
            }
            if (obj is IVarType vt)
            {
                return PossibleTypes.Contains(vt);
            }

            return false;
        }
        bool IVarType.Equals(IVarType type) => Equals(type);

        public override int GetHashCode() => HashCode.Combine(PossibleTypes);
    }
}
