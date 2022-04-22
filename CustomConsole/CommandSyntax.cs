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

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            if (code.Length == 0) { return false; }

            if (code.Length == 1)
            {
                return code[0].Type == KeyWordType.Word;
            }

            for (int i = 1; i < code.Length; i++)
            {
                if ((i % 2 == 1 && code[i].Word != "-")
                        ||
                    (i % 2 == 0 && code[i].Type != KeyWordType.Word))
                {
                    return false;
                }
            }

            return true;
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => true;

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = 1;

            if (code.Length < 1) { return null; }

            string name = code[0].Word;
            Command c = Commands.Find(c => c.Name == name);
            // No command could be found
            if (c == null) { return null; }

            object[] props = new object[c.Properties.Length];
            // Fills props with false
            for (int i = 0; i < props.Length; i++)
            {
                props[i] = false;
            }

            if (code.Length == 1)
            {
                return new CommandExecutable(this, new KeyWord[] { new KeyWord(name, KeyWordType.Word) }, props, objs =>
                {
                    c.Handle(objs);
                    return null;
                });
            }

            // Find all properties
            for (int i = 1; i < code.Length; i++)
            {
                if (i % 2 == 1 && code[i].Word != "-")
                {
                    break;
                }

                index++;
                if (i % 2 == 1) { continue; }

                if (code[i].Word.Length > 1)
                {
                    if (fill) { return null; }
                    break;
                }

                int proI = Array.IndexOf(c.Properties, code[i].Word[0]);

                if (proI == -1)
                {
                    if (fill) { return null; }
                    break;
                }

                props[proI] = true;
            }

            return new CommandExecutable(this, new KeyWord[] { new KeyWord(name, KeyWordType.Word) }, props, objs =>
            {
                c.Handle(objs);
                return null;
            });
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
        }

        public static List<Command> Commands { get; } = new List<Command>();
    }
}
