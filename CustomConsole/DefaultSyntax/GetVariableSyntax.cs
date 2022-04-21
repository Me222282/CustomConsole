using System;

namespace CustomConsole
{
    public class GetVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Word) };
        public int InputCount => 0;
        public VariableType ReturnType => VariableType.NonVoid;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            string word = code[0].Word;

            return code.Length == 1 &&
                Syntax.Variables.Exists(v => v.Name == word);
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => true;

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 1;

            if (code.Length == 0) { return null; }

            string word = code[0].Word;
            Variable v = Syntax.Variables.Find(v => v.Name == word && v.Type.Compatable(type));
            if (v != null)
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, _ =>
                {
                    return v.Getter();
                }, null);
            }

            return null;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            if (code.Length != 1) { return null; }

            return CorrectSyntax(code, type, out _);
        }
    }

    public class SetVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[3]
        {
            new KeyWord(null, KeyWordType.Word),
            new KeyWord("=", KeyWordType.Special),
            new KeyWord("", KeyWordType.Input, (int)VariableType.NonVoid)
        };
        public int InputCount => 1;
        public VariableType ReturnType => VariableType.Void;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            // Cannot fit asignment statment
            if (code.Length < 3) { return false; }

            string word = code[0].Word;

            return code[1].Word == "=" &&
                Syntax.Variables.Exists(v => v.Name == word);
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Type == KeyWordType.Word &&
                    code.Length > (i + 1) && code[i + 1].Word == "=")
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = -1;

            if (!(code.Length > 3 && code[1].Word == "="))
            {
                return null;
            }

            string word = code[0].Word;
            Variable v = Syntax.Variables.Find(v => v.Name == word && v.Type.Compatable(type));
            if (v == null) { return null; }

            Executable e = Syntax.FindCorrectSyntax(code[2..], this, v.Type, new KeyWord(), true, out index);
            index += 2;

            if (e == null) { return null; }

            return new Executable(this, new KeyWord[] { code[0], Keywords[1], new KeyWord(v.Type) }, new Executable[1] { e }, objs =>
            {
                v.Setter(objs[0]);
                return objs[0];
            }, new VariableType[] { v.Type });
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            return CorrectSyntax(code, type, out _);
        }
    }

    public class CreateVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[4]
        {
            new KeyWord(null, KeyWordType.Word),
            new KeyWord(null, KeyWordType.Word),
            new KeyWord("=", KeyWordType.Special),
            new KeyWord("", KeyWordType.Input, (int)VariableType.NonVoid)
        };
        public int InputCount => 1;
        public VariableType ReturnType => VariableType.Void;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            // Cannot fit asignment statment
            if (code.Length < 4) { return false; }

            return code[2].Word == "=" &&
                code[0].Type == KeyWordType.Word &&
                code[1].Type == KeyWordType.Word;
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Type == KeyWordType.Word &&
                    code.Length > (i + 2) &&
                    code[i + 1].Type == KeyWordType.Word &&
                    code[i + 2].Word == "=")
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = -1;

            if (code.Length < 4) { return null; }
            if (code[2].Word != "=" || code[1].Type != KeyWordType.Word) { return null; }

            string name = code[1].Word;

            VariableType? vart = GetType(code[0].Word);

            // some reasons why a variable cannot be made
            if (vart == null) { return null; }

            Executable e = Syntax.FindCorrectSyntax(code[3..], this, (VariableType)vart, new KeyWord(), true, out index);
            index += 3;

            // beep
            if (e == null) { return null; }

            return new Executable(this, new KeyWord[] { code[0], code[1], Keywords[2], new KeyWord((VariableType)vart) }, new Executable[1] { e }, objs =>
            {
                if (Syntax.Variables.Exists(v => v.Name == name))
                {
                    throw new Exception($"Variable with name {name} already exists");
                }

                Syntax.Variables.Add(new Variable(name, (VariableType)vart, objs[0]));
                return objs[0];
            }, new VariableType[] { (VariableType)vart });
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            return CorrectSyntax(code, type, out _);
        }

        private static VariableType? GetType(string name)
        {
            return name switch
            {
                "int" => VariableType.Int,
                "float" => VariableType.Float,
                "double" => VariableType.Double,
                "bool" => VariableType.Bool,
                "char" => VariableType.Char,
                "string" => VariableType.String,
                "Vector2" => VariableType.Vector2,
                "Vector3" => VariableType.Vector3,
                "Vector4" => VariableType.Vector4,
                _ => null
            };
        }
    }

    public class RemoveVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[6]
        {
            new KeyWord("memory", KeyWordType.Word),
            new KeyWord(".", KeyWordType.Special),
            new KeyWord("Collect", KeyWordType.Word),
            new KeyWord("(", KeyWordType.BracketOpen),
            new KeyWord(null, KeyWordType.Word),
            new KeyWord(")", KeyWordType.BracketClosed),
        };
        public int InputCount => 0;
        public VariableType ReturnType => VariableType.Void;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            // Cannot fit assignment statment
            if (code.Length != 6) { return false; }

            string var = code[4].Word;

            return code[0].Word == Keywords[0].Word &&
                code[1].Word == Keywords[1].Word &&
                code[2].Word == Keywords[2].Word &&
                code[3].Word == Keywords[3].Word &&
                code[5].Word == Keywords[5].Word &&
                Syntax.Variables.Exists(v => v.Name == var);
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (code.Length > (i + 5) &&
                    code[i].Word == Keywords[0].Word &&
                    code[i + 1].Word == Keywords[1].Word &&
                    code[i + 2].Word == Keywords[2].Word &&
                    code[i + 3].Word == Keywords[3].Word &&
                    code[i + 4].Type == KeyWordType.Word &&
                    code[i + 5].Word == Keywords[5].Word)
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 6;

            if (code.Length < 6) { return null; }

            // Not valid syntax
            if (code[0].Word != Keywords[0].Word ||
                code[1].Word != Keywords[1].Word ||
                code[2].Word != Keywords[2].Word ||
                code[3].Word != Keywords[3].Word ||
                code[5].Word != Keywords[5].Word)
            { return null; }

            string var = code[4].Word;
            Variable v = Syntax.Variables.Find(v => v.Name == var);
            if (v == null) { return null; }

            return new Executable(this, new KeyWord[] { Keywords[0], Keywords[1], Keywords[2], code[3], new KeyWord(var, KeyWordType.Word), code[5] }, null, objs =>
            {
                if (!Syntax.Variables.Exists(v => v.Name == var))
                {
                    throw new Exception($"No variable with name {var} could be removed");
                }

                Syntax.Variables.Remove(v);
                return null;
            }, null);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            return CorrectSyntax(code, type, out _);
        }
    }
}