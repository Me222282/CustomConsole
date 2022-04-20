using System;
using System.Collections.Generic;

namespace CustomConsole
{
    public delegate object ExecuteHandle(object[] @params);

    public class Executable
    {
        public Executable(ISyntax src, KeyWord[] syntax, Executable[] subExecutables, ExecuteHandle handle)
        {
            Source = src ?? throw new Exception($"{nameof(src)} cannot be null.");

            if (syntax == null)
            {
                Syntax = new KeyWord[1] { new KeyWord("", 0) };
            }
            else
            {
                Syntax = syntax;
            }

            if (subExecutables == null)
            {
                SubExecutables = Array.Empty<Executable>();
            }
            else
            {
                SubExecutables = subExecutables;
            }

            Function = handle ?? throw new Exception($"{nameof(handle)} cannot be null.");
        }

        public ISyntax Source { get; }
        public KeyWord[] Syntax { get; }
        public Executable[] SubExecutables { get; }

        public KeyWord[] CompleteSyntax
        {
            get
            {
                List<KeyWord> combinedWords = new List<KeyWord>();

                int subSyntaxCount = 0;
                for (int i = 0; i < Syntax.Length; i++)
                {
                    if (Syntax[i].Word == "")
                    {
                        if (SubExecutables.Length < subSyntaxCount + 1)
                        {
                            throw new Exception($"Incompatible {nameof(Executable)} data");
                        }

                        combinedWords.AddRange(SubExecutables[subSyntaxCount].CompleteSyntax);
                        subSyntaxCount++;
                        continue;
                    }

                    combinedWords.Add(Syntax[i]);
                }

                return combinedWords.ToArray();
            }
        }

        public ExecuteHandle Function { get; }

        public object Execute()
        {
            object[] @params = new object[SubExecutables.Length];

            for (int i = 0; i < @params.Length; i++)
            {
                @params[i] = SubExecutables[i].Execute();
            }

            return Function(@params);
        }
    }
}
