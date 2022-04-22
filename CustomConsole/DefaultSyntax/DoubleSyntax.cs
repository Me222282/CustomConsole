using System;

namespace CustomConsole
{
    public class DoubleSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Number) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.Double;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            if (code.Length == 2)
            {
                return code[0].Type == KeyWordType.Number &&
                    code[1].Word == "d";
            }

            return code[0].Type == KeyWordType.Number;
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

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, out int index, object param = null)
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
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type)
        {
            if (code.Length != 1 && code.Length != 2) { return null; }

            return CorrectSyntax(code, type, out _);
        }
    }
}
