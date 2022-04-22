using System;
using System.Collections.Generic;
using Zene.Structs;

namespace CustomConsole
{
    public sealed class Syntax : ISyntax
    {
        public Syntax(KeyWord[] keywords, IVarType returnType, ExecuteHandle handle)
        {
            Keywords = keywords;
            ReturnType = returnType;
            Handle = handle;

            InputCount = 0;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i] == KeyWord.UnknownInput)
                {
                    InputCount++;
                }
            }
        }

        public KeyWord[] Keywords { get; }
        public ExecuteHandle Handle { get; }
        public IVarType ReturnType { get; }
        public int InputCount { get; }

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
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
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

                    wordIndex++;
                    continue;
                }
            }

            // Reached end of this syntaxes keywords
            return wordIndex == Keywords.Length;
        }
        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, out int index, object param)
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

            return new Executable(this, Keywords, subExes, Handle, ReturnType);
        }

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type)
        {
            Executable e = CorrectSyntax(code, type, out int i, true);

            // Not correct syntax
            if (code.Length != i) { return null; }

            return e;
        }

        private static readonly List<ISyntax> _possibleSyntaxes = new List<ISyntax>();

        public static List<ISyntax> Syntaxes { get; }
        private static Executable FindSyntax(ReadOnlySpan<KeyWord> syntax, IVarType returnType)
        {
            if (syntax.Length == 0) { return null; }

            for (int i = 0; i < _possibleSyntaxes.Count; i++)
            {
                // Null is Void
                if (_possibleSyntaxes[i].ReturnType == null &&
                    (returnType != null || returnType != VarType.Any)) { continue; }
                // Doesn't return correct type
                if (!_possibleSyntaxes[i].ReturnType.Compatible(returnType)) { continue; }

                bool could = _possibleSyntaxes[i].ValidSyntax(syntax);

                if (could)
                {
                    Executable e = _possibleSyntaxes[i].CreateInstance(syntax, returnType);

                    if (e == null) { continue; }

                    if (e.Source != _possibleSyntaxes[i])
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

        public static Executable Decode(ReadOnlySpan<KeyWord> syntax, IVarType returnType)
        {
            _possibleSyntaxes.Clear();

            for (int i = 0; i < Syntaxes.Count; i++)
            {
                bool possible = Syntaxes[i].PossibleSyntax(syntax);

                if (!possible) { continue; }

                _possibleSyntaxes.Add(Syntaxes[i]);
            }

            return FindSyntax(syntax, returnType);
        }

        public static Executable FindCorrectSyntax(ReadOnlySpan<KeyWord> syntax, ISyntax source, IVarType returnType, KeyWord nextWord, bool fill, out int nextIndex)
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

            for (int i = 0; i < _possibleSyntaxes.Count; i++)
            {
                if (!fill && _possibleSyntaxes[i].EqualKeyWords(source)) { continue; }

                // Null is Void
                if (_possibleSyntaxes[i].ReturnType == null &&
                    (returnType != null || returnType != VarType.Any)) { continue; }
                // Doesn't return correct type
                if (!_possibleSyntaxes[i].ReturnType.Compatible(returnType)) { continue; }

                Executable e = _possibleSyntaxes[i].CorrectSyntax(syntax, returnType, out nextIndex, fill);

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
        private static Executable ManageBrackets(ReadOnlySpan<KeyWord> syntax, IVarType returnType, KeyWord nextWord, bool fill, out int nextIndex)
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
            try { e = FindSyntax(bracketCode, returnType); }
            catch (ConsoleException) { return null; }
            return e;
        }
        internal static int FindClosingBracket(ReadOnlySpan<KeyWord> syntax, int openBrackIndex)
        {
            int bracketNumber = syntax[openBrackIndex].Info;

            for (int i = openBrackIndex + 1; i < syntax.Length; i++)
            {
                // Found closing breacket
                if (syntax[i].Word == ")" && syntax[i].Info == bracketNumber) { return i; }
            }

            //return -1;
            return syntax.Length;
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
                new CreateVariableSyntax(),
                new RemoveVariableSyntax(),

                // Vector2
                new Syntax(new KeyWord[]
                {
                    new KeyWord("{", KeyWordType.BracketOpen),
                    new KeyWord(VarType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VarType.Double),
                    new KeyWord("}", KeyWordType.BracketClosed)
                }, VarType.Vector2, (objs) =>
                {
                    return new Vector2((double)objs[0], (double)objs[1]);
                }),
                // Vector3
                new Syntax(new KeyWord[]
                {
                    new KeyWord("{", KeyWordType.BracketOpen),
                    new KeyWord(VarType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VarType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VarType.Double),
                    new KeyWord("}", KeyWordType.BracketClosed)
                }, VarType.Vector3, (objs) =>
                {
                    return new Vector3((double)objs[0], (double)objs[1], (double)objs[2]);
                }),
                // Vector4
                new Syntax(new KeyWord[]
                {
                    new KeyWord("{", KeyWordType.BracketOpen),
                    new KeyWord(VarType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VarType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VarType.Double),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VarType.Double),
                    new KeyWord("}", KeyWordType.BracketClosed)
                }, VarType.Vector4, (objs) =>
                {
                    return new Vector4((double)objs[0], (double)objs[1], (double)objs[2], (double)objs[3]);
                }),

                // String constructor
                new Syntax(new KeyWord[]
                {
                    new KeyWord("new", KeyWordType.BracketOpen),
                    new KeyWord("string", KeyWordType.BracketOpen),
                    new KeyWord("(", KeyWordType.BracketOpen),
                    new KeyWord(VarType.Char),
                    new KeyWord(",", KeyWordType.Special),
                    new KeyWord(VarType.Int),
                    new KeyWord(")", KeyWordType.BracketClosed)
                }, VarType.String, (objs) =>
                {
                    return new string((char)objs[0], (int)objs[1]);
                }),

                //
                // Integer Operators
                //
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("|", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => (int)objs[0] | (int)objs[1],
                        _ => throw new BigException()
                    };
                }),
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("&", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => (int)objs[0] & (int)objs[1],
                        _ => throw new BigException()
                    };
                }),
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord(">", KeyWordType.Special),
                    new KeyWord(">", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => (int)objs[0] >> (int)objs[1],
                        _ => throw new BigException()
                    };
                }),
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("<", KeyWordType.Special),
                    new KeyWord("<", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => (int)objs[0] << (int)objs[1],
                        _ => throw new BigException()
                    };
                }),
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("^", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => (int)objs[0] ^ (int)objs[1],
                        _ => throw new BigException()
                    };
                }),

                //
                // Normal Operators
                //
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("-", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                    VarType.Double,
                    VarType.Float,
                    VarType.Vector2,
                    VarType.Vector3,
                    VarType.Vector4
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => (int)objs[0] - (int)objs[1],
                        double => (double)objs[0] - (double)objs[1],
                        float => (float)objs[0] - (float)objs[1],
                        Vector2 => (Vector2)objs[0] - (Vector2)objs[1],
                        Vector3 => (Vector3)objs[0] - (Vector3)objs[1],
                        Vector4 => (Vector4)objs[0] - (Vector4)objs[1],
                        _ => throw new BigException()
                    };
                }),
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("+", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                    VarType.Double,
                    VarType.Float,
                    VarType.String,
                    VarType.Char,
                    VarType.Vector2,
                    VarType.Vector3,
                    VarType.Vector4
                }, VarType.NonVoid, (objs) =>
                {
                    /*
                    return (VariableType)objs[^1] switch
                    {
                        VariableType.Int => (int)objs[0] + (int)objs[1],
                        VariableType.Double => (double)objs[0] + (double)objs[1],
                        VariableType.Float => (float)objs[0] + (float)objs[1],
                        VariableType.Vector2 => (Vector2)objs[0] + (Vector2)objs[1],
                        VariableType.Vector3 => (Vector3)objs[0] + (Vector3)objs[1],
                        VariableType.Vector4 => (Vector4)objs[0] + (Vector4)objs[1],
                    };*/

                    return objs[0] switch
                    {
                        int => (int)objs[0] + (int)objs[1],
                        double => (double)objs[0] + (double)objs[1],
                        float => (float)objs[0] + (float)objs[1],
                        string => (string)objs[0] + (string)objs[1],
                        char => $"{(char)objs[0]}{(char)objs[1]}",
                        Vector2 => (Vector2)objs[0] + (Vector2)objs[1],
                        Vector3 => (Vector3)objs[0] + (Vector3)objs[1],
                        Vector4 => (Vector4)objs[0] + (Vector4)objs[1],
                        _ => throw new BigException()
                    };
                }),
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("*", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                    VarType.Double,
                    VarType.Float,
                    VarType.Vector2,
                    VarType.Vector3,
                    VarType.Vector4
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => (int)objs[0] * (int)objs[1],
                        double => (double)objs[0] * (double)objs[1],
                        float => (float)objs[0] * (float)objs[1],
                        Vector2 => (Vector2)objs[0] * (Vector2)objs[1],
                        Vector3 => (Vector3)objs[0] * (Vector3)objs[1],
                        Vector4 => (Vector4)objs[0] * (Vector4)objs[1],
                        _ => throw new BigException()
                    };
                }),
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("/", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                    VarType.Double,
                    VarType.Float,
                    VarType.Vector2,
                    VarType.Vector3,
                    VarType.Vector4
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => (int)objs[0] / (int)objs[1],
                        double => (double)objs[0] / (double)objs[1],
                        float => (float)objs[0] / (float)objs[1],
                        Vector2 => (Vector2)objs[0] / (Vector2)objs[1],
                        Vector3 => (Vector3)objs[0] / (Vector3)objs[1],
                        Vector4 => (Vector4)objs[0] / (Vector4)objs[1],
                        _ => throw new BigException()
                    };
                }),

                //
                // Extra Operators
                //
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord("-", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid)
                }, new IVarType[]
                {
                    VarType.Int,
                    VarType.Double,
                    VarType.Float,
                    VarType.Vector2,
                    VarType.Vector3,
                    VarType.Vector4
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => -(int)objs[0],
                        double => -(double)objs[0],
                        float => -(float)objs[0],
                        Vector2 => -(Vector2)objs[0],
                        Vector3 => -(Vector3)objs[0],
                        Vector4 => -(Vector4)objs[0],
                        _ => throw new BigException()
                    };
                }),
                new TypedSyntax(new KeyWord[]
                {
                    new KeyWord("|", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("|", KeyWordType.Special)
                }, new IVarType[]
                {
                    VarType.Int,
                    VarType.Double,
                    VarType.Float
                }, VarType.NonVoid, (objs) =>
                {
                    return objs[0] switch
                    {
                        int => Math.Abs((int)objs[0]),
                        double => Math.Abs((double)objs[0]),
                        float => Math.Abs((float)objs[0]),
                        _ => throw new BigException()
                    };
                }),
            };
        }
    }
}