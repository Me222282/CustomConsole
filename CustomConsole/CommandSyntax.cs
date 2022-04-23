using System;
using System.Collections.Generic;

namespace CustomConsole
{
    public class CommandSyntax : ISyntax
    {
        private static readonly string[] _postChars = new string[] { "-" };

        public KeyWord[] Keywords { get; } = new KeyWord[] { new KeyWord(null, KeyWordType.Word) };
        public int InputCount => 0;
        public IVarType ReturnType => VarType.Void;
        public ICodeFormat DisplayFormat { get; } = new CodeFormat(null, _postChars);

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code) => code[0].Type == KeyWordType.Word;
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => code[0].Type == KeyWordType.Word;

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = code.Length;

            if (code.Length < 1 || !fill) { return null; }

            string name = code[0].Word;
            Command c = Commands.Find(c => c.Name == name);
            // No command could be found
            if (c == null) { return null; }

            Executable[] props = FindProperties(code[1..], new LastFind(code, this), source, c.Properties);

            // Invalid properties
            if (props == null) { return null; }

            KeyWord[] syntax = new KeyWord[1 + c.Properties.Length];
            syntax[0] = new KeyWord(name, KeyWordType.Word);

            for (int i = 1; i < syntax.Length; i++)
            {
                syntax[i] = new KeyWord(c.Properties[i - 1].DataType);
            }

            return new CommandExecutable(this, syntax, props, c.Handle);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
        }

        public static List<Command> Commands { get; } = new List<Command>();

        private Executable[] FindProperties(ReadOnlySpan<KeyWord> syntax, LastFind lastcall, SyntaxPasser source, CommandProperty[] props)
        {
            if (props.Length == 0) { return new Executable[0]; }

            List<(CommandProperty, int)> nonPrefix = new List<(CommandProperty, int)>();
            List<(CommandProperty, int)> prefix = new List<(CommandProperty, int)>();

            // Sort properties by prefix
            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].DataType == VarType.Bool)
                {
                    prefix.Add((props[i], i));
                    continue;
                }

                nonPrefix.Add((props[i], i));
            }

            Executable[] exes = new Executable[props.Length];

            int nonPreCount = 0;

            for (int i = 0; i < syntax.Length; i++)
            {
                // Prefixed property
                if (syntax[i].Word == "-")
                {
                    if (syntax.Length <= (i + 1)) { return null; }

                    string name = syntax[i + 1].Word;
                    i += 1;

                    (CommandProperty, int) property = prefix.Find(cp => cp.Item1.Name == name);

                    // No command property found
                    if (property.Item1.Name == null) { return null; }

                    exes[property.Item2] = new BooleanExecutable(this, new KeyWord[]
                    {
                        new KeyWord("-", KeyWordType.Special),
                        new KeyWord(name, KeyWordType.Word)
                    }, true);
                    continue;
                }

                Executable e = source.FindCorrectSyntax(syntax[i..], lastcall, nonPrefix[nonPreCount].Item1.DataType, new KeyWord(), false, out int addI);

                // No valid syntax
                if (e == null) { return null; }

                i += addI - 1;

                exes[nonPrefix[nonPreCount].Item2] = e;

                nonPreCount++;
            }

            // Replace all boolean nulls with false
            for (int i = 0; i < exes.Length; i++)
            {
                if (exes[i] == null && props[i].DataType == VarType.Bool)
                {
                    exes[i] = new BooleanExecutable(this, null, false);
                }
            }

            return exes;
        }
    }
}
