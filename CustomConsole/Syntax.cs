using System;
using System.Collections.Generic;
using Zene.Structs;

namespace CustomConsole
{
    public class Syntax : ISyntax
    {
        public Syntax(KeyWord[] keywords, VariableType returnType, ExecuteHandle handle)
        {
            Keywords = keywords;
            ReturnType = returnType;
            Handle = handle;

            int inputIndex = 0;
            List<VariableType> inputs = new List<VariableType>();

            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i] == KeyWord.UnknownInput)
                {
                    inputs.Add(keywords[i].InputType);
                    inputIndex++;
                }
            }

            InputTypes = inputs.ToArray();
        }

        public KeyWord[] Keywords { get; }
        public VariableType[] InputTypes { get; }
        public ExecuteHandle Handle { get; }
        public VariableType ReturnType { get; }
        public int InputCount => InputTypes.Length;

        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            if (code.Length == 0) { return false; }

            int wordIndex = 0;

            for (int i = 0; i < code.Length; i++)
            {
                // Input text - need to find next keyword
                if (Keywords.Length > wordIndex &&
                    Keywords[wordIndex].Word == "") { wordIndex++; }

                if (Keywords.Length > wordIndex &&
                    code[i].Word == Keywords[wordIndex].Word)
                {
                    // Next found keyword left no space for input syntax,
                    // the current keyword match is incorrect
                    if ((i == 0) && (wordIndex > 0))
                    {
                        continue;
                    }

                    // Reached end of this syntaxes keywords but not the end of the code,
                    // the current keyword match is incorrect
                    if (code.Length != (i + 1) &&
                        Keywords.Length == (wordIndex + 1))
                    {
                        continue;
                    }

                    wordIndex++;
                    continue;
                }

                // Empty string means input syntax - it could be any length
                if ((wordIndex - 1 >= 0) && Keywords[wordIndex - 1].Word == "")
                {
                    if (code[i].Word == "(")
                    {
                        int end = FindClosingBracket(code, i);

                        if (end == -1)
                        {
                            throw new ConsoleException("Invalid syntax - no closing bracket");
                        }

                        i = end;
                    }

                    continue;
                }

                // Reaching this point means that the code doesn't match this syntaxes Keywords
                return false;
            }

            // Reached end of this syntaxes keywords
            return wordIndex == Keywords.Length;
        }
        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param)
        {
            bool fill = param is bool b && b;

            index = 0;

            if (code.Length == 0) { return null; }

            int inputIndex = 0;
            Executable[] subExes = new Executable[InputCount];
            /*
            if (Keywords.Length > 1 &&
                // First keyword is input
                Keywords[0].Word == "" &&
                Keywords[0].Word == )
            {

            }*/

            for (int i = 0; i < Keywords.Length; i++)
            {
                // Input
                if (Keywords[i].Type == KeyWordType.Input)
                {
                    ReadOnlySpan<KeyWord> slice = code[index..];

                    KeyWord nextKW = new KeyWord();
                    if (Keywords.Length > (i + 1))
                    {
                        nextKW = Keywords[i + 1];

                        int lastI = slice.FindLastIndex(k => k.Word == nextKW.Word);
                        slice = slice.Slice(0,
                            lastI + 1 > slice.Length ? lastI : (lastI + 1));
                    }

                    Executable e = FindCorrectSyntax(slice, this, Keywords[i].InputType, nextKW, fill && Keywords.Length == (i + 1), out int addIndex);
                    index += addIndex;

                    // No valid syntax to match input could be found
                    if (e == null) { return null; }

                    subExes[inputIndex] = e;
                    inputIndex++;
                    continue;
                }

                // Syntax doesn't match
                if (code[index].Word != Keywords[i].Word)
                {
                    return null;
                }

                index++;
            }

            return new Executable(this, Keywords, subExes, Handle);
        }

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            Executable e = CorrectSyntax(code, type, out int i, true);

            // Not correct syntax
            if (code.Length != i) { return null; }

            return e;
        }

        public static List<ISyntax> Syntaxes { get; }
        public static Executable Decode(ReadOnlySpan<KeyWord> syntax, VariableType returnType = VariableType.Any)
        {
            if (syntax.Length == 0) { return null; }

            for (int i = 0; i < Syntaxes.Count; i++)
            {
                // Doesn't return correct type
                if (!returnType.Compatable(Syntaxes[i].ReturnType)) { continue; }

                bool could = Syntaxes[i].ValidSyntax(syntax);

                if (could)
                {
                    Executable e = Syntaxes[i].CreateInstance(syntax, returnType);

                    if (e == null) { continue; }

                    if (e.Source != Syntaxes[i])
                    {
                        throw new Exception("Invalid return from syntax");
                    }

                    return e;
                }
            }

            if (syntax[0].Word == "(")
            {
                Executable e = ManageBrackets(syntax, returnType, new KeyWord(), true, out int i);

                if (i != syntax.Length)
                {
                    throw new ConsoleException("Unknown syntax");
                }

                return e;
            }

            throw new ConsoleException("Unknown syntax");
        }

        public static Executable FindCorrectSyntax(ReadOnlySpan<KeyWord> syntax, ISyntax source, VariableType returnType, KeyWord nextWord, bool fill, out int nextIndex)
        {
            nextIndex = 0;

            if (syntax.Length == 0) { return null; }
            // There is a next keyword and the earliest possible syntax doesn't contain it
            if (nextWord.Word != null && !syntax[1..].Contains(nextWord)) { return null; }

            if (syntax[0].Word == "(")
            {
                Executable exe = ManageBrackets(syntax, returnType, nextWord, fill, out nextIndex);
                
                if (exe != null) { return exe; }
            }

            for (int i = 0; i < Syntaxes.Count; i++)
            {
                if (!fill && Syntaxes[i].EqualKeyWords(source)) { continue; }

                // Doesn't return correct type
                if (!returnType.Compatable(Syntaxes[i].ReturnType)) { continue; }

                Executable e = Syntaxes[i].CorrectSyntax(syntax, returnType, out nextIndex, fill);

                // Not correct syntax
                if (e == null) { continue; }

                // Syntax needs to decode all the keywords
                if (fill && nextIndex != syntax.Length) { continue; }

                // There is a next keyword
                if (!fill && nextWord.Word != null &&

                    // Syntax is long enough
                    (syntax.Length > nextIndex &&
                    // Next keyword doesn't match
                    syntax[nextIndex] != nextWord))
                {
                    continue;
                }

                return e;
            }

            return null;
        }
        private static Executable ManageBrackets(ReadOnlySpan<KeyWord> syntax, VariableType returnType, KeyWord nextWord, bool fill, out int nextIndex)
        {
            int end = FindClosingBracket(syntax, 0);

            // Couldn't find closing bracket
            if (end == -1)
            {
                throw new ConsoleException("Invalid syntax - no closing bracket");
            }

            nextIndex = end + 1;

            // Brackets don't fill whole syntax
            if (fill && nextIndex != syntax.Length) { return null; }

            // There is a next keyword
            if (!fill && nextWord.Word != null &&

                // Syntax is long enough
                ((syntax.Length > nextIndex &&
                // Next keyword doesn't match
                syntax[nextIndex] != nextWord) ||

                // Syntax isn't long enough
                syntax.Length < (nextIndex + 1)))
            {
                return null;
            }

            ReadOnlySpan<KeyWord> bracketCode = syntax[1..end];

            Executable e;
            try { e = Decode(bracketCode, returnType); }
            catch (ConsoleException) { return null; }
            return e;
        }
        private static int FindClosingBracket(ReadOnlySpan<KeyWord> syntax, int openBrackIndex)
        {
            int bracketNumber = syntax[openBrackIndex].Info;

            for (int i = openBrackIndex + 1; i < syntax.Length; i++)
            {
                // Found closing breacket
                if (syntax[i].Word == ")" && syntax[i].Info == bracketNumber) { return i; }
            }

            return -1;
        }

        public static List<Variable> Variables { get; } = new List<Variable>();

        static Syntax()
        {
            Syntaxes = new List<ISyntax>()
            {
                new IntegerSyntax(),
                new FloatSyntax(),
                new DoubleSyntax(),
                new StringSyntax(),
                new CharSyntax(),
                new BoolSyntax(),
                new GetVariableSyntax(),
                new SetVariableSyntax(),

                // Vector2
                new Syntax(new KeyWord[]
                {
                    new KeyWord("{", KeyWordType.BracketOpen),
                    new KeyWord(VariableType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VariableType.Double),
                    new KeyWord("}", KeyWordType.BracketClosed)
                }, VariableType.Vector2, (objs) =>
                {
                    return new Vector2((double)objs[0], (double)objs[1]);
                }),
                // Vector3
                new Syntax(new KeyWord[]
                {
                    new KeyWord("{", KeyWordType.BracketOpen),
                    new KeyWord(VariableType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VariableType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VariableType.Double),
                    new KeyWord("}", KeyWordType.BracketClosed)
                }, VariableType.Vector3, (objs) =>
                {
                    return new Vector3((double)objs[0], (double)objs[1], (double)objs[2]);
                }),
                // Vector4
                new Syntax(new KeyWord[]
                {
                    new KeyWord("{", KeyWordType.BracketOpen),
                    new KeyWord(VariableType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VariableType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VariableType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VariableType.Double),
                    new KeyWord("}", KeyWordType.BracketClosed)
                }, VariableType.Vector4, (objs) =>
                {
                    return new Vector4((double)objs[0], (double)objs[1], (double)objs[2], (double)objs[3]);
                }),

                //
                // Interger operators
                //
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("|", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] | (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("&", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] & (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("<<", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] << (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord(">>", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] >> (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("^", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] ^ (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("-", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] - (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("+", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] + (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("*", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] * (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("/", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] / (int)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Int),
                    new KeyWord("%", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return (int)objs[0] % (int)objs[1];
                }),

                new Syntax(new KeyWord[]
                {
                    new KeyWord("-", KeyWordType.Special),
                    new KeyWord(VariableType.Int)
                }, VariableType.Int, (objs) =>
                {
                    return -(int)objs[0];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord("|", KeyWordType.Special),
                    new KeyWord(VariableType.Int),
                    new KeyWord("|", KeyWordType.Special)
                }, VariableType.Int, (objs) =>
                {
                    return Math.Abs((int)objs[0]);
                }),

                //
                // Double operators
                //
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Double),
                    new KeyWord("-", KeyWordType.Special),
                    new KeyWord(VariableType.Double)
                }, VariableType.Double, (objs) =>
                {
                    return (double)objs[0] - (double)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Double),
                    new KeyWord("+", KeyWordType.Special),
                    new KeyWord(VariableType.Double)
                }, VariableType.Double, (objs) =>
                {
                    return (double)objs[0] + (double)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Double),
                    new KeyWord("*", KeyWordType.Special),
                    new KeyWord(VariableType.Double)
                }, VariableType.Double, (objs) =>
                {
                    return (double)objs[0] * (double)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Double),
                    new KeyWord("/", KeyWordType.Special),
                    new KeyWord(VariableType.Double)
                }, VariableType.Double, (objs) =>
                {
                    return (double)objs[0] / (double)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Double),
                    new KeyWord("%", KeyWordType.Special),
                    new KeyWord(VariableType.Double)
                }, VariableType.Double, (objs) =>
                {
                    return (double)objs[0] % (double)objs[1];
                }),

                //
                // Vector2 operators
                //
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Vector2),
                    new KeyWord("-", KeyWordType.Special),
                    new KeyWord(VariableType.Vector2)
                }, VariableType.Vector2, (objs) =>
                {
                    return (Vector2)objs[0] - (Vector2)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Vector2),
                    new KeyWord("+", KeyWordType.Special),
                    new KeyWord(VariableType.Vector2)
                }, VariableType.Vector2, (objs) =>
                {
                    return (Vector2)objs[0] + (Vector2)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Vector2),
                    new KeyWord("*", KeyWordType.Special),
                    new KeyWord(VariableType.Vector2)
                }, VariableType.Vector2, (objs) =>
                {
                    return (Vector2)objs[0] * (Vector2)objs[1];
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord(VariableType.Vector2),
                    new KeyWord("/", KeyWordType.Special),
                    new KeyWord(VariableType.Vector2)
                }, VariableType.Vector2, (objs) =>
                {
                    return (Vector2)objs[0] / (Vector2)objs[1];
                })
            };
        }
    }
}