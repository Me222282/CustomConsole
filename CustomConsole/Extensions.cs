using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zene.Structs;

namespace CustomConsole
{
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

            bool inString = false;
            bool inChar = false;

            KeyWordType type = 0;

            StringBuilder word = new StringBuilder();

            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];

                if (!inString && !inChar && c == ')')
                {
                    newWord = true;
                    inNumber = false;
                    inText = false;

                    if (word.Length > 0)
                    {
                        keywords.Add(new KeyWord(word.ToString(), type));
                        word.Clear();
                    }

                    bracket--;
                    keywords.Add(new KeyWord(")", KeyWordType.BracketClosed, bracket));
                    continue;
                }
                if (!inString && !inChar && c == ']')
                {
                    newWord = true;
                    inNumber = false;
                    inText = false;

                    if (word.Length > 0)
                    {
                        keywords.Add(new KeyWord(word.ToString(), type));
                        word.Clear();
                    }

                    bracketSquare--;
                    keywords.Add(new KeyWord("]", KeyWordType.BracketClosed, bracketSquare));
                    continue;
                }
                if (!inString && !inChar && c == '}')
                {
                    newWord = true;
                    inNumber = false;
                    inText = false;

                    if (word.Length > 0)
                    {
                        keywords.Add(new KeyWord(word.ToString(), type));
                        word.Clear();
                    }

                    bracketCurle--;
                    keywords.Add(new KeyWord("}", KeyWordType.BracketClosed, bracketCurle));
                    continue;
                }

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
                        keywords.Add(new KeyWord(c.ToString(), KeyWordType.Special));

                        continue;
                    }

                    continue;
                }

                // Underscores can be insinde numbers
                if (inNumber && c == '_' &&
                    // not at end of string
                    code.Length > (i + 10) &&
                    char.IsNumber(code[i + 1]))
                {
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

                if (inString && c == '\"' && code[i - 1] != '\\')
                {
                    newWord = true;
                    inString = false;

                    keywords.Add(new KeyWord(word.ToString(), KeyWordType.String));
                    word.Clear();

                    keywords.Add(new KeyWord("\"", KeyWordType.String));

                    continue;
                }
                if (inChar && c == '\'' && code[i - 1] != '\\')
                {
                    newWord = true;
                    inChar = false;

                    keywords.Add(new KeyWord(word.ToString(), KeyWordType.Char));
                    word.Clear();

                    keywords.Add(new KeyWord("\'", KeyWordType.Char));

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
                format = new DefaultCodeFormat();
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

        public static IVarType GetHigherType(this IVarType a, IVarType b)
        {
            if (a.Equals(b)) { return a; }

            if (a == VarType.Any) { return b; }
            if (b == VarType.Any) { return a; }

            if (a == VarType.NonVoid && b != VarType.Void) { return b; }
            if (b == VarType.NonVoid && a != VarType.Void) { return a; }

            if (a.ImplicitTo != null && a.ImplicitTo.Contains(b)) { return b; }
            if (b.ImplicitTo != null && b.ImplicitTo.Contains(a)) { return a; }

            throw new ConsoleException("Invalid comparison");
        }

        public static bool Contains<T>(this ReadOnlySpan<T> source, T value)
        {
            // Code sourced from dotnet - System.Linq.Enumerable.Contains<TSource>(IEnumerable<TSource>, TSource, IEqualityComparer<TSource>)
            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
            foreach (T element in source)
                if (comparer.Equals(element, value)) return true;
            return false;
        }
        public static int FindLastIndex<T>(this ReadOnlySpan<T> source, Predicate<T> match)
        {
            // Code sourced from dotnet - System.Collections.Generic.List<T>.FindLastIndex(int, int, Predicate<T>)
            for (int i = source.Length - 1; i >= 0; i--)
            {
                if (match(source[i]))
                {
                    return i;
                }
            }

            return source.Length;
        }

        public static bool EqualKeyWords(this ISyntax l, ISyntax r)
        {
            // Not equal lengths
            if (l.Keywords.Length != r.Keywords.Length) { return false; }

            for (int i = 0; i < l.Keywords.Length; i++)
            {
                // Only equal keywords
                if (l.Keywords[i].Word != r.Keywords[i].Word) { return false; }
            }

            return true;
        }

        public static IVarType GetVarType(this object obj)
        {
            if (obj == null) { return VarType.Void; }

            return obj switch
            {
                int => VarType.Int,
                float => VarType.Float,
                double => VarType.Double,
                bool => VarType.Bool,
                char => VarType.Char,
                string => VarType.String,
                Vector2 => VarType.Vector2,
                Vector3 => VarType.Vector3,
                Vector4 => VarType.Vector4,
                Variable => VarType.Variable,
                _ => throw new Exception("Unknown or unsupported type")
            };
        }
    }
}
