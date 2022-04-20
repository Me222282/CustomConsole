using System;

namespace CustomConsole
{
    public class CharSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[3]
        {
            new KeyWord("\'", KeyWordType.Char),
            new KeyWord(null, KeyWordType.Char),
            new KeyWord("\'", KeyWordType.Char)
        };
        public VariableType[] InputTypes => null;
        public VariableType ReturnType => VariableType.Char;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 3 &&
                code[0].Word == "\'" &&
                code[2].Word == "\'";
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 3;

            if (code.Length < 2) { return null; }

            // Not valid char
            if (code[0].Word != "\'" || code[2].Word != "\'")
            {
                return null;
            }

            string text = code[1].Word;

            StringSyntax.FormatStringInput(ref text);

            // Chars only have one character
            if (text.Length > 1) { return null; }

            return new Executable(this, new KeyWord[]
                {
                    new KeyWord("\'", KeyWordType.Char),
                    new KeyWord(text, KeyWordType.Char),
                    new KeyWord("\'", KeyWordType.Char)
                }, null, objs => text[0]);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            if (code.Length != 3) { return null; }

            return CorrectSyntax(code, type, out _);
        }
    }
}
