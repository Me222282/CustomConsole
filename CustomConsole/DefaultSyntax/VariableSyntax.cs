using System;

namespace CustomConsole
{
    public class GetVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Word) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.NonVoid;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            string word = code[0].Word;

            return code.Length == 1 &&
                SyntaxPasser.Variables.Exists(v => v.Name == word);
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => true;

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, out int index, bool fill)
        {
            index = 1;

            if (code.Length == 0) { return null; }

            string word = code[0].Word;
            Variable v = SyntaxPasser.Variables.Find(v => v.Name == word);
            // No variable found
            if (v == null) { return null; }

            // Reference is to the variable
            if (type == VarType.Variable)
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, _ =>
                {
                    return v;
                }, VarType.Variable);
            }

            // Type not supported
            if (!type.Compatible(v.Type)) { return null; }

            return new Executable(this, new KeyWord[] { code[0] }, null, _ =>
            {
                return v.Getter();
            }, v.Type);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            if (code.Length != 1) { return null; }

            return CorrectSyntax(code, type, source, out _, true);
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
        public IVarType ReturnType => VarType.Void;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            // Cannot fit asignment statment
            if (code.Length < 3) { return false; }

            string word = code[0].Word;

            return code[1].Word == "=" &&
                SyntaxPasser.Variables.Exists(v => v.Name == word);
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

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, out int index, bool fill)
        {
            index = -1;

            if (!(code.Length > 2 && code[1].Word == "="))
            {
                return null;
            }

            string word = code[0].Word;
            Variable v = SyntaxPasser.Variables.Find(v => v.Name == word);
            if (v == null) { return null; }

            Executable e = source.FindCorrectSyntax(code[2..], this, v.Type, new KeyWord(), fill, out index);
            index += 2;

            if (e == null) { return null; }

            return new Executable(this, new KeyWord[] { code[0], Keywords[1], new KeyWord(v.Type) }, new Executable[1] { e }, objs =>
            {
                v.Setter(objs[0]);
                return objs[0];
            }, VarType.Void);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            return CorrectSyntax(code, type, source, out _, true);
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
        public IVarType ReturnType => VarType.Void;
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

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, out int index, bool fill)
        {
            index = -1;

            if (code.Length < 4) { return null; }
            if (code[2].Word != "=" || code[1].Type != KeyWordType.Word) { return null; }

            string name = code[1].Word;

            VarType vart;
            try
            {
                vart = GetType(code[0].Word);
            }
            catch (ConsoleException) { return null; }

            // some reasons why a variable cannot be made
            if (vart == null) { return null; }

            Executable e = source.FindCorrectSyntax(code[3..], this, vart, new KeyWord(), fill, out index);
            index += 3;

            // beep
            if (e == null) { return null; }

            return new Executable(this, new KeyWord[] { code[0], code[1], Keywords[2], new KeyWord(vart) }, new Executable[1] { e }, objs =>
            {
                if (SyntaxPasser.Variables.Exists(v => v.Name == name))
                {
                    throw new Exception($"Variable with name {name} already exists");
                }

                SyntaxPasser.Variables.Add(new Variable(name, vart, objs[0]));
                return objs[0];
            }, VarType.Void);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            return CorrectSyntax(code, type, source, out _, true);
        }

        private static VarType GetType(string name)
        {
            return name switch
            {
                "int" => VarType.Int,
                "float" => VarType.Float,
                "double" => VarType.Double,
                "bool" => VarType.Bool,
                "char" => VarType.Char,
                "string" => VarType.String,
                "Vector2" => VarType.Vector2,
                "Vector3" => VarType.Vector3,
                "Vector4" => VarType.Vector4,
                _ => throw new ConsoleException("Unknown type")
            };
        }
    }
}