using System;
using System.Collections.Generic;
using Zene.Structs;

namespace CustomConsole
{
    public class SyntaxPasser
    {
        private readonly List<ISyntax> _possibleSyntaxes = new List<ISyntax>();

        private Executable FindSyntax(ReadOnlySpan<KeyWord> syntax, IVarType returnType)
        {
            if (syntax.Length == 0) { return null; }

            for (int i = 0; i < _possibleSyntaxes.Count; i++)
            {
                // Null is Void
                if (_possibleSyntaxes[i].ReturnType == null &&
                    returnType != null &&
                    returnType != VarType.Any) { continue; }
                // Doesn't return correct type
                if (_possibleSyntaxes[i].ReturnType != null &&
                    !_possibleSyntaxes[i].ReturnType.Compatible(returnType)) { continue; }

                bool could = _possibleSyntaxes[i].ValidSyntax(syntax);

                if (could)
                {
                    Executable e = _possibleSyntaxes[i].CreateInstance(syntax, returnType, this);

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
        public Executable Decode(ReadOnlySpan<KeyWord> syntax, IVarType returnType)
        {
            if (syntax.Length == 0) { return null; }

            _possibleSyntaxes.Clear();

            for (int i = 0; i < Syntaxes.Count; i++)
            {
                bool possible = Syntaxes[i].PossibleSyntax(syntax);

                if (!possible) { continue; }

                _possibleSyntaxes.Add(Syntaxes[i]);
            }

            return FindSyntax(syntax, returnType);
        }
        public Executable Decode(ReadOnlySpan<KeyWord> syntax) => Decode(syntax, VarType.Any);

        public Executable FindCorrectSyntax(ReadOnlySpan<KeyWord> syntax, LastFind lastCall, IVarType returnType, KeyWord nextWord, bool fill, out int nextIndex)
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
                // Removes a possibility of stackoverflow where this function would
                // be called from the same source with the same syntax over and over.
                if (lastCall.LastSyntax == syntax &&
                    lastCall.Source == _possibleSyntaxes[i]) { continue; }

                // Null is Void
                if (_possibleSyntaxes[i].ReturnType == null &&
                    returnType != null &&
                    returnType != VarType.Any) { continue; }
                // Doesn't return correct type
                if (_possibleSyntaxes[i].ReturnType != null &&
                    !_possibleSyntaxes[i].ReturnType.Compatible(returnType)) { continue; }

                Executable e = _possibleSyntaxes[i].CorrectSyntax(syntax, returnType, this, nextWord, out nextIndex, fill);

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
        private Executable ManageBrackets(ReadOnlySpan<KeyWord> syntax, IVarType returnType, KeyWord nextWord, bool fill, out int nextIndex)
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

        public static List<ISyntax> Syntaxes { get; }
        public static List<Variable> Variables { get; } = new List<Variable>();
        public static List<Function> Functions { get; }

        static SyntaxPasser()
        {
            Functions = new List<Function>()
            {
                new Function(new string[] { "memory", "Free" }, new IVarType[] { VarType.Variable }, VarType.Void, objs =>
                {
                    Variable var = (Variable)objs[0];

                    if (!Variables.Exists(v => v.Name == var.Name))
                    {
                        throw new Exception($"No variable with name {var.Name} could be removed");
                    }

                    Variables.Remove(var);
                    return null;
                })
            };

            Syntaxes = new List<ISyntax>()
            {
                new IntegerSyntax(),
                new FloatSyntax(),
                new DoubleSyntax(),
                new StringSyntax(),
                new CharSyntax(),
                new BoolSyntax(),
                new TypeSyntax(),
                new GetVariableSyntax(),
                new FunctionSyntax(),
                new SetVariableSyntax(),
                new CreateVariableSyntax(),

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

                new Syntax(new KeyWord[]
                {
                    new KeyWord("HALT_AND_CATCH_FIRE", KeyWordType.Word)
                }, VarType.Void, _ =>
                {
                    throw new Exception("please STOP what you are DOING and COMBUST!!!");
                }),
                new Syntax(new KeyWord[]
                {
                    new KeyWord("HALT_AND_CATCH_FIRE", KeyWordType.Word),
                    new KeyWord("with", KeyWordType.Word),
                    new KeyWord(VarType.String)
                }, VarType.Void, objs =>
                {
                    throw new Exception((string)objs[0]);
                }),

                //
                // Integer Operators
                //
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                new SyntaxTyped(new KeyWord[]
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
                }, VarType.NonVoid,
                new CodeFormat("-"),
                (objs) =>
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
                new SyntaxTyped(new KeyWord[]
                {
                    new KeyWord("|", KeyWordType.Special),
                    new KeyWord(VarType.NonVoid),
                    new KeyWord("|", KeyWordType.Special)
                }, new IVarType[]
                {
                    VarType.Int,
                    VarType.Double,
                    VarType.Float
                }, VarType.NonVoid,
                new CodeFormat("|"),
                (objs) =>
                {
                    //Console.WriteLine(objs[0].GetType());
                    /*
                    return objs[0] switch
                    {
                        int => Math.Abs((int)objs[0]),
                        double => Math.Abs((double)objs[0]),
                        float => Math.Abs((float)objs[0]),
                        _ => throw new BigException()
                    };*/
                    
                    object o;
                    switch (objs[0])
                    {
                        case int:
                            o = Math.Abs((int)objs[0]);
                            break;

                        case float:
                            o = Math.Abs((float)objs[0]);
                            break;

                        case double:
                            o = Math.Abs((double)objs[0]);
                            break;

                        default:
                            throw new BigException();
                    }

                    //Console.WriteLine(o.GetType());
                    return o;
                }),
            };
        }
    }
}