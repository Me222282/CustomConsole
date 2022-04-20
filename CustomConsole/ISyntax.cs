using System;

namespace CustomConsole
{
    public interface ISyntax
    {
        public KeyWord[] Keywords { get; }
        public VariableType ReturnType { get; }
        public ICodeFormat DisplayFormat { get; }

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code);

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code);
    }
}
