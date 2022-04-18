namespace CustomConsole
{
    public delegate void VariableSetter(object obj);
    public delegate object VariableGetter();

    public class Variable
    {
        public Variable(string name, StringConverter paramConverter, VariableGetter get, VariableSetter set)
        {
            Name = name;

            ParamConverter = paramConverter;

            Getter = get;
            Setter = set;
        }

        public string Name { get; }

        public StringConverter ParamConverter { get; }

        public VariableGetter Getter { get; set; }
        public VariableSetter Setter { get; set; }
    }
}
