using System;

namespace CustomConsole
{
    public interface ISyntax
    {
        public KeyWord[] Keywords { get; }
        public VariableType[] InputTypes { get; }
        public VariableType ReturnType { get; }
        public ICodeFormat DisplayFormat { get; }

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code);
        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null);

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type);
    }
}
