using System;

namespace CustomConsole
{
    public class StringSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[3]
        {
            new KeyWord("\"", KeyWordType.String),
            new KeyWord(null, KeyWordType.String),
            new KeyWord("\"", KeyWordType.String)
        };
        public int InputCount => 0;
        public VariableType ReturnType => VariableType.String;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 3 &&
                code[0].Word == "\"" &&
                code[2].Word == "\"";
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Contains(new KeyWord("\"", KeyWordType.Char));
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 3;

            if (code.Length < 2) { return null; }

            // Not valid string
            if (code[0].Word != "\"" || code[2].Word != "\"")
            {
                return null;
            }

            string text = code[1].Word;

            FormatStringInput(ref text);

            return new Executable(this, new KeyWord[]
                {
                    new KeyWord("\"", KeyWordType.String),
                    new KeyWord(text, KeyWordType.String),
                    new KeyWord("\"", KeyWordType.String)
                }, null, _ => text, null);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            if (code.Length != 3) { return null; }

            return CorrectSyntax(code, type, out _);
        }

        public static void FormatStringInput(ref string str)
        {
            str = str.Replace("\\\\", "\\");
            str = str.Replace("\\\"", "\"");
            str = str.Replace("\\\'", "\'");
            str = str.Replace("\\n", "\n");
            str = str.Replace("\\r", "\r");
        }
    }
}
