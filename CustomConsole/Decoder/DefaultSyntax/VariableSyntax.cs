using System;

namespace CustomConsole
{
    public sealed class GetVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Word) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.NonVoid;
        public ICodeFormat DisplayFormat { get; } = new DefaultCodeFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
            => code.Length == 1 && code[0].Type == KeyWordType.Word;
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => true;

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = 1;
            /*
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
            }, v.Type);*/

            if (!(code.Length > 0 && code[0].Type == KeyWordType.Word)) { return null; }

            string word = code[0].Word;

            return new Executable(this, new KeyWord[] { code[0] }, null, _ =>
            {
                Variable v = SyntaxPasser.Variables.Find(v => v.Name == word);
                // No variable found
                if (v == null) { throw new Exception($"Coulnd't find variable with name {word}"); }

                // Reference is to the variable
                if (type == VarType.Variable) { return v; }

                return v.Getter();
            }, VarType.NonVoid);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            if (code.Length != 1) { return null; }

            return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
        }
    }

    public sealed class SetVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[3]
        {
            new KeyWord(null, KeyWordType.Word),
            new KeyWord("=", KeyWordType.Special),
            new KeyWord(VarType.NonVoid)
        };
        public int InputCount => 1;
        public IVarType ReturnType => VarType.Void;
        public ICodeFormat DisplayFormat { get; } = new DefaultCodeFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            // Cannot fit asignment statment
            if (code.Length < 3) { return false; }

            return code[1].Word == "=" &&
                code[0].Type == KeyWordType.Word;
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

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = 0;
            /*
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
            }, VarType.Void);*/

            if (code.Length == 0 || code[0].Type != KeyWordType.Word) { return null; }

            string word = code[0].Word;

            Executable e = source.FindCorrectSyntax(code[2..], new LastFind(code, this), VarType.NonVoid, nextKeyword, fill, out index);
            index += 2;
            // No valid syntax could be found
            if (e == null) { return null; }

            return new Executable(this, new KeyWord[] { code[0], Keywords[1], new KeyWord(e.ReturnType) }, new Executable[1] { e }, objs =>
            {
                Variable v = SyntaxPasser.Variables.Find(v => v.Name == word);
                // No variable found
                if (v == null) { throw new Exception($"Coulnd't find variable with name {word}"); }

                if (!objs[0].GetVarType().Compatible(v.Type))
                {
                    throw new Exception($"Variable {v.Name}'s type is not compatible with assignment type");
                }

                Extensions.PassToType(ref objs[0], v.Type);

                v.Setter(objs[0]);
                return objs[0];
            }, VarType.Void);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
        }
    }

    public sealed class CreateVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[4]
        {
            new KeyWord(VarType.Type),
            new KeyWord(null, KeyWordType.Word),
            new KeyWord("=", KeyWordType.Special),
            new KeyWord(VarType.NonVoid)
        };
        public int InputCount => 1;
        public IVarType ReturnType => VarType.Void;
        public ICodeFormat DisplayFormat { get; } = new DefaultCodeFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            if (code.Length < 4) { return false; }

            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Word == "=" &&
                    (i - 1) > 0 &&
                    code[i - 1].Type == KeyWordType.Word)
                {
                    return true;
                }
            }

            return false;
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => ValidSyntax(code);

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = 0;

            if (code.Length < 4) { return null; }

            Executable t = source.FindCorrectSyntax(code, new LastFind(code, this), VarType.Type, new KeyWord("", KeyWordType.Word), false, out index);
            if (t == null) { return null; }

            if (!(code.Length > (index + 2) &&
                code[index].Type == KeyWordType.Word &&
                code[index + 1].Word == "="))
            { return null; }

            string name = code[index].Word;

            Executable e = source.FindCorrectSyntax(code[(index + 2)..], new LastFind(code, this), VarType.NonVoid, nextKeyword, fill, out int addIndex);
            index += addIndex;
            // beep
            if (e == null) { return null; }

            return new Executable(this, new KeyWord[] { new KeyWord(VarType.Type), code[1], Keywords[2], new KeyWord(e.ReturnType) }, new Executable[2] { t, e }, objs =>
            {
                if (SyntaxPasser.Variables.Exists(v => v.Name == name))
                {
                    throw new Exception($"Variable with name {name} already exists");
                }

                IVarType type = (IVarType)objs[0];
                Extensions.PassToType(ref objs[1], type);

                SyntaxPasser.Variables.Add(new Variable(name, type, objs[1]));
                return objs[1];
            }, VarType.Void);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
        }
    }
}