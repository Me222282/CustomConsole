﻿using System;

namespace CustomConsole
{
    public class FloatSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[1] { new KeyWord(null, KeyWordType.Number) };
        public int InputCount => 0;
        public VariableType ReturnType => VariableType.Float;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 2 &&
                code[0].Type == KeyWordType.Number &&
                code[1].Word == "f";
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Type == KeyWordType.Number &&
                    code.Length > (i + 1) && code[i + 1].Word == "f")
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 1;

            if (code.Length < 2 || code[1].Word != "f") { return null; }

            if (float.TryParse(code[0].Word, out float f))
            {
                return new Executable(this, new KeyWord[] { code[0], code[1] }, null, _ =>
                {
                    return f;
                }, null);
            }

            return null;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            if (code.Length != 2) { return null; }

            return CorrectSyntax(code, type, out _);
        }
    }
}