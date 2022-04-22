using System;

namespace CustomConsole
{
    public class IntegerSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Number) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.Int;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 1 &&
                code[0].Type == KeyWordType.Number &&
                !code[0].Word.Contains('.');
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Type == KeyWordType.Number &&
                    !code[i].Word.Contains('.'))
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, out int index, object param = null)
        {
            index = 1;

            if (code.Length == 0) { return null; }

            if (int.TryParse(code[0].Word, out int i))
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, _ =>
                {
                    return i;
                }, VarType.Int);
            }

            return null;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type)
        {
            if (code.Length != 1) { return null; }

            return CorrectSyntax(code, type, out _);
        }
    }
}
