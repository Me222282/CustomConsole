using System;

namespace CustomConsole
{
    public class BoolSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Word) };
        public VariableType[] InputTypes => null;
        public VariableType ReturnType => VariableType.Bool;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 1 &&
                (code[0].Word == "true" || code[0].Word == "false");
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 1;

            if (code.Length < 1) { return null; }

            if (bool.TryParse(code[0].Word, out bool b))
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, objs =>
                {
                    return b;
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
