using System;

namespace CustomConsole
{
    public class DoubleSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Number) };
        public VariableType ReturnType => VariableType.Double;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 1 &&
                code[0].Type == KeyWordType.Number;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 1;

            if (code.Length == 0) { return null; }

            if (double.TryParse(code[0].Word, out double d))
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, objs =>
                {
                    return d;
                });
            }

            return null;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            if (code.Length != 1) { return null; }

            return CorrectSyntax(code, type, out _);
        }
    }
}
