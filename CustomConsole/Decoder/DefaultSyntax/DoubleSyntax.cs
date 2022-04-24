using System;

namespace CustomConsole
{
    public sealed class DoubleSyntax : ISyntax
    {
        private static readonly string[] _preChars = new string[] { ".", "d" };
        private static readonly string[] _postChars = new string[] { "." };

        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Number) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.Double;
        public ICodeFormat DisplayFormat { get; } = new CodeFormat(_preChars, _postChars);

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            if (code.Length == 2)
            {
                return code[0].Type == KeyWordType.Number &&
                    code[1].Word == "d";
            }

            return code.Length == 1 &&
                code[0].Type == KeyWordType.Number;
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Type == KeyWordType.Number)
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = 1;

            if (code.Length == 0) { return null; }

            if (code.Length >= 2 && code[1].Word == "d")
            {
                index = 2;
            }

            if (double.TryParse(code[0].Word, out double d))
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, _ =>
                {
                    return d;
                }, VarType.Double);
            }

            return null;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            if (code.Length != 1 && code.Length != 2) { return null; }

            return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
        }
    }
}
