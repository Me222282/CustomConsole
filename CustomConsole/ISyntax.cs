namespace CustomConsole
{
    public interface ISyntax
    {
        public KeyWord[] Keywords { get; }
        public VariableType ReturnType { get; }

        public bool PotentialMatch(KeyWord[] code);

        public Executable CreateInstance(KeyWord[] code);
    }
}
