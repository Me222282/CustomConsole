using System;

namespace CustomConsole
{
    public interface ISyntax
    {
        public KeyWord[] Keywords { get; }
        public int InputCount { get; }
        public IVarType ReturnType { get; }
        public ICodeFormat DisplayFormat { get; }

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code);
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code);
        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill);

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source);
    }
}
