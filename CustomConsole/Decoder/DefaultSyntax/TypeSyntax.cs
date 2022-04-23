using System;

namespace CustomConsole
{
    public class TypeSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Word) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.Type;
        public ICodeFormat DisplayFormat { get; } = new DefaultCodeFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
            => code.Length == 1 && code[0].Type == KeyWordType.Word;
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => true;

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = 1;

            if (!(code.Length > 0 && code[0].Type == KeyWordType.Word)) { return null; }

            VarType value = code[0].Word switch
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
                "Type" => VarType.Type,
                _ => null
            };

            if (value == null) { return null; } 

            return new Executable(this, new KeyWord[] { code[0] }, null, _ =>
            {
                return value;
            }, VarType.Type);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            if (code.Length != 1) { return null; }

            return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
        }
    }
}
