using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomConsole
{
    public struct DefaultFormat : ICodeFormat
    {
        private static readonly string[] _preChars = new string[] { ")", "]", "}", "\"", "\'" };
        private static readonly string[] _postChars = new string[] { "(", "[", "{", "\"", "\'" };

        public string[] NoPreSpaces => _preChars;
        public string[] NoPostSpaces => _postChars;
    }

    public static class Extensions
    {
        public static KeyWord[] FindKeyWords(this string code)
        {
            code = code.Trim();

            List<KeyWord> keywords = new List<KeyWord>(code.Length);

            int bracket = 0;
            int bracketSquare = 0;
            int bracketCurle = 0;

            bool newWord = true;
            bool inNumber = false;
            bool inText = false;
            bool inSpecial = false;

            bool inString = false;
            bool inChar = false;

            KeyWordType type = 0;

            StringBuilder word = new StringBuilder();

            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];

                if (newWord)
                {
                    if (c == '(')
                    {
                        keywords.Add(new KeyWord("(", KeyWordType.BracketOpen, bracket));
                        bracket++;
                        continue;
                    }
                    if (c == '[')
                    {
                        keywords.Add(new KeyWord("[", KeyWordType.BracketOpen, bracketSquare));
                        bracketSquare++;
                        continue;
                    }
                    if (c == '{')
                    {
                        keywords.Add(new KeyWord("{", KeyWordType.BracketOpen, bracketCurle));
                        bracketCurle++;
                        continue;
                    }
                    if (char.IsNumber(c))
                    {
                        newWord = false;
                        inNumber = true;
                        type = KeyWordType.Number;

                        word.Append(c);

                        continue;
                    }
                    if (c == '_' || char.IsLetter(c))
                    {
                        newWord = false;
                        inText = true;
                        type = KeyWordType.Word;

                        word.Append(c);

                        continue;
                    }
                    if (c == '\"')
                    {
                        newWord = false;
                        inString = true;
                        type = KeyWordType.String;

                        keywords.Add(new KeyWord("\"", KeyWordType.String));

                        continue;
                    }
                    if (c == '\'')
                    {
                        newWord = false;
                        inChar = true;
                        type = KeyWordType.Char;

                        keywords.Add(new KeyWord("\'", KeyWordType.Char));

                        continue;
                    }
                    if (!char.IsWhiteSpace(c))
                    {
                        newWord = false;
                        inSpecial = true;
                        type = KeyWordType.Special;

                        word.Append(c);

                        continue;
                    }

                    continue;
                }

                if (!inString && !inChar && c == ')')
                {
                    newWord = true;
                    inNumber = false;
                    inText = false;
                    inSpecial = false;

                    keywords.Add(new KeyWord(word.ToString(), type));
                    word.Clear();

                    bracket--;
                    keywords.Add(new KeyWord(")", KeyWordType.BracketClosed, bracket));
                    continue;
                }
                if (!inString && !inChar && c == ']')
                {
                    newWord = true;
                    inNumber = false;
                    inText = false;
                    inSpecial = false;

                    keywords.Add(new KeyWord(word.ToString(), type));
                    word.Clear();
                    
                    bracketSquare--;
                    keywords.Add(new KeyWord("]", KeyWordType.BracketClosed, bracketSquare));
                    continue;
                }
                if (!inString && !inChar && c == '}')
                {
                    newWord = true;
                    inNumber = false;
                    inText = false;
                    inSpecial = false;

                    keywords.Add(new KeyWord(word.ToString(), type));
                    word.Clear();

                    bracketCurle--;
                    keywords.Add(new KeyWord("}", KeyWordType.BracketClosed, bracketCurle));
                    continue;
                }

                if (inNumber && c != '.' && !char.IsNumber(c))
                {
                    newWord = true;
                    inNumber = false;

                    keywords.Add(new KeyWord(word.ToString(), KeyWordType.Number));
                    word.Clear();

                    i--;
                    continue;
                }

                if (inText && c != '_' && !char.IsNumber(c) && !char.IsLetter(c))
                {
                    newWord = true;
                    inText = false;

                    keywords.Add(new KeyWord(word.ToString(), KeyWordType.Word));
                    word.Clear();

                    i--;
                    continue;
                }

                if (inSpecial && (char.IsWhiteSpace(c) || char.IsNumber(c) || char.IsLetter(c)))
                {
                    newWord = true;
                    inSpecial = false;

                    keywords.Add(new KeyWord(word.ToString(), KeyWordType.Special));
                    word.Clear();

                    i--;
                    continue;
                }

                if (inString && c == '\"' && code[i - 1] == '\\')
                {
                    newWord = true;
                    inString = false;

                    keywords.Add(new KeyWord("\"", KeyWordType.String));

                    keywords.Add(new KeyWord(word.ToString(), KeyWordType.String));
                    word.Clear();

                    continue;
                }
                if (inChar && c == '\'' && code[i - 1] == '\\')
                {
                    newWord = true;
                    inChar = false;

                    keywords.Add(new KeyWord("\'", KeyWordType.Char));

                    keywords.Add(new KeyWord(word.ToString(), KeyWordType.Char));
                    word.Clear();

                    continue;
                }

                word.Append(c);
            }

            if (word.Length > 0)
            {
                keywords.Add(new KeyWord(word.ToString(), type));
            }

            return keywords.ToArray();
        }
        public static string CreateCode(this KeyWord[] keywords, ICodeFormat format = null)
        {
            if (format == null)
            {
                format = new DefaultFormat();
            }

            StringBuilder str = new StringBuilder();

            for (int i = 0; i < keywords.Length; i++)
            {
                string word = keywords[i].Word;

                if (word == "")
                {
                    throw new ConsoleException("Incomplete syntax");
                }

                str.Append(word);

                // No spaces after specified format keywords
                if (format.NoPostSpaces.Contains(word))
                {
                    continue;
                }

                // No spaces before specified format keywords
                string nextWord = keywords[i + 1].Word;
                if (format.NoPreSpaces.Contains(nextWord))
                {
                    continue;
                }

                // End of loop - break before adding final space
                if (keywords.Length >= (i + 1)) { break; }

                str.Append(' ');
            }

            return str.ToString();
        }
        public static string CreateCode(this Executable executable)
            => CreateCode(executable.CompleteSyntax, executable.Source.DisplayFormat);
    }
}
