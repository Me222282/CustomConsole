﻿using System;

namespace CustomConsole
{
    public class StringSyntax : ISyntax
    {
        public KeyWord[] Keywords { get; } = new KeyWord[3]
        {
            new KeyWord("\"", KeyWordType.String),
            new KeyWord(null, KeyWordType.String),
            new KeyWord("\"", KeyWordType.String)
        };
        public VariableType ReturnType => VariableType.String;
        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            return code.Length == 3 &&
                code[0].Word == "\"" &&
                code[2].Word == "\"";
        }

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param = null)
        {
            index = 3;

            if (code.Length == 0) { return null; }

            // Not valid string
            if (code[0].Word != "\"" || code[2].Word != "\"")
            {
                return null;
            }

            string text = code[1].Word;

            return new Executable(this, new KeyWord[]
                {
                    new KeyWord("\"", KeyWordType.String),
                    new KeyWord(text, KeyWordType.String),
                    new KeyWord("\"", KeyWordType.String)
                }, null, objs => text);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            if (code.Length != 3) { return null; }

            return CorrectSyntax(code, type, out _);
        }
    }
}
