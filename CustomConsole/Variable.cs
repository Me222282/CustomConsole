namespace CustomConsole
{
    public delegate void VariableSetter(object obj);
    public delegate object VariableGetter();

    public class Variable
    {
        public Variable(string name, IVarType type, VariableGetter get, VariableSetter set)
        {
            Name = name;
            Type = type;
            Getter = get;
            Setter = set;
        }
        public Variable(string name, IVarType type, object value)
        {
            Name = name;
            Type = type;
            _value = value;
            Getter = () => value;
            Setter = obj => value = obj;
        }

        public string Name { get; }

        public IVarType Type { get; }

        private object _value;

        public VariableGetter Getter { get; set; }
        public VariableSetter Setter { get; set; }
    }
}
