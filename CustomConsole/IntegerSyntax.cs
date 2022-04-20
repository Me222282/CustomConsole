using System;

namespace CustomConsole
{
    public class IntegerSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Number) };
        public VariableType ReturnType => VariableType.Int;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 1 &&
                code[0].Type == KeyWordType.Number &&
                !code[0].Word.Contains('.');
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, out int index, object param = null)
        {
            index = 1;

            if (code.Length == 0) { return null; }

            if (int.TryParse(code[0].Word, out int i))
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, objs =>
                {
                    return i;
                });
            }

            return null;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code)
        {
            if (code.Length != 1) { return null; }

            if (int.TryParse(code[0].Word, out int i))
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, objs =>
                {
                    return i;
                });
            }

            return null;
        }
    }
}
