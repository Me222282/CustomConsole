using System;

namespace CustomConsole
{
    public class FloatSyntax : ISyntax
    {
        private static readonly string[] _preChars = new string[] { ".", "f" };
        private static readonly string[] _postChars = new string[] { "." };

        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Number) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.Float;
        public ICodeFormat DisplayFormat { get; } = new CodeFormat(_preChars, _postChars);

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 2 &&
                code[0].Type == KeyWordType.Number &&
                code[1].Word == "f";
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Type == KeyWordType.Number &&
                    code.Length > (i + 1) && code[i + 1].Word == "f")
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, out int index, bool fill)
        {
            index = 2;

            if (code.Length < 2) { return null; }
            if (code[1].Word != "f") { return null; }

            if (float.TryParse(code[0].Word, out float f))
            {
                return new Executable(this, new KeyWord[] { code[0], code[1] }, null, _ =>
                {
                    return f;
                }, VarType.Float);
            }

            return null;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            if (code.Length != 2) { return null; }

            return CorrectSyntax(code, type, source, out _, true);
        }
    }
}
