using System;

namespace CustomConsole
{
    public class GetVariableSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Word) };
        public VariableType[] InputTypes => null;
        public VariableType ReturnType => VariableType.NonVoid;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            string word = code[0].Word;

            return code.Length == 1 &&
                Syntax.Variables.Exists(v => v.Name == word);
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 1;

            if (code.Length == 0) { return null; }

            string word = code[0].Word;
            Variable v = Syntax.Variables.Find(v => v.Name == word && v.Type.Compatable(type));
            if (v != null)
            {
                return new Executable(this, new KeyWord[] { code[0] }, null, objs =>
                {
                    return v.Getter();
                });
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
        public VariableType[] InputTypes { get; } = new VariableType[1] { VariableType.NonVoid };
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

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = -1;

            if (code.Length == 0) { return null; }

            string word = code[0].Word;
            Variable v = Syntax.Variables.Find(v => v.Name == word && v.Type.Compatable(type));
            if (v == null) { return null; }

            if (code.Length < 3 || code[1].Word != "=")
            {
                return null;
            }

            Executable e = Syntax.FindCorrectSyntax(code[2..], this, v.Type, new KeyWord(), true, out index);

            if (e == null || code.Length != (index + 2)) { return null; }

            return new Executable(this, new KeyWord[] { code[0] }, new Executable[1] { e }, objs =>
            {
                v.Setter(objs[0]);
                return objs[0];
            });
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            return CorrectSyntax(code, type, out _);
        }
    }
}
