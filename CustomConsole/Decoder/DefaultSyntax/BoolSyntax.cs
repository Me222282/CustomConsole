using System;

namespace CustomConsole
{
    public sealed class BoolSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Word) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.Bool;
        public ICodeFormat DisplayFormat { get; } = new DefaultCodeFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 1 &&
                (code[0].Word == "true" || code[0].Word == "false");
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Word == "true" || code[i].Word == "false") { return true; }
            }

            return false;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = 1;

            if (code.Length < 1) { return null; }

            if (bool.TryParse(code[0].Word, out bool b))
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, _ =>
                {
                    return b;
                }, VarType.Bool);
            }

            return null;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            if (code.Length != 1) { return null; }

            return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
        }
    }
}
