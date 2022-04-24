using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zene.Structs;

namespace CustomConsole
{
    public delegate object ExecuteHandle(object[] @params);

    public class Executable
    {
        public Executable(ISyntax src, KeyWord[] syntax, Executable[] subExecutables, ExecuteHandle handle, IVarType returnType)
        {
            Source = src ?? throw new Exception($"{nameof(src)} cannot be null.");

            Syntax = syntax ?? new KeyWord[0];

            SubExecutables = subExecutables ?? Array.Empty<Executable>();

            Function = handle ?? throw new Exception($"{nameof(handle)} cannot be null.");

            ReturnType = returnType;
        }

        public virtual ISyntax Source { get; }
        private KeyWord[] _syntax;
        public KeyWord[] Syntax
        {
            get => _syntax;
            protected set
            {
                _syntax = value ?? new KeyWord[0];

                _inputTypes = GetInputTypes(_syntax);
            }
        }
        public virtual Executable[] SubExecutables { get; }
        
        public IVarType ReturnType { get; }
        protected IVarType[] _inputTypes;

        public KeyWord[] CompleteSyntax
        {
            get
            {
                List<KeyWord> combinedWords = new List<KeyWord>();

                int subSyntaxCount = 0;
                for (int i = 0; i < Syntax.Length; i++)
                {
                    if (Syntax[i].Type == KeyWordType.Input)
                    {
                        if (SubExecutables.Length < subSyntaxCount + 1)
                        {
                            throw new Exception($"Insufficient {nameof(Executable)} data");
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
        public string SourceCode
        {
            get
            {
                if (Source == null
                        ||
                    (SubExecutables == null &&
                    _inputTypes.Length > 0))
                {
                    throw new Exception($"Insufficient {nameof(Executable)} data");
                }

                ICodeFormat format = Source.DisplayFormat ?? new DefaultCodeFormat();

                StringBuilder str = new StringBuilder();

                int inputCount = 0;

                for (int i = 0; i < Syntax.Length; i++)
                {
                    string word = Syntax[i].Word;

                    if (Syntax[i].Type == KeyWordType.Input)
                    {
                        if (SubExecutables.Length < inputCount + 1)
                        {
                            throw new Exception($"Insufficient {nameof(Executable)} data");
                        }

                        str.Append(SubExecutables[inputCount].SourceCode);
                        inputCount++;
                    }
                    else
                    {
                        str.Append(word);
                    }

                    // End of loop - break before adding final space
                    if (Syntax.Length == (i + 1)) { break; }

                    // No spaces after specified format keywords
                    if (format.NoPostSpaces.Contains(word)) { continue; }

                    // No spaces before specified format keywords
                    string nextWord = Syntax[i + 1].Word;
                    if (format.NoPreSpaces.Contains(nextWord)) { continue; }

                    // End of string is already a space
                    if (str[^1] == ' ') { continue; }

                    str.Append(' ');
                }

                return str.ToString();
            }
        }

        public ExecuteHandle Function { get; }

        public virtual object Execute()
        {
            object[] @params = new object[SubExecutables.Length];

            for (int i = 0; i < @params.Length; i++)
            {
                @params[i] = SubExecutables[i].Execute();

                if (_inputTypes.Length < (i + 1)) { continue; }

                Extensions.PassToType(ref @params[i], _inputTypes[i]);
            }

            return Function(@params);
        }

        public override string ToString()
        {
            return $"Executable: \"{SourceCode}\"";
        }

        private static IVarType[] GetInputTypes(KeyWord[] syntax)
        {
            if (syntax == null) { return new IVarType[0]; }

            List<IVarType> types = new List<IVarType>();
            for (int i = 0; i < syntax.Length; i++)
            {
                if (syntax[i].Type == KeyWordType.Input)
                {
                    types.Add(syntax[i].InputType);
                }
            }

            return types.ToArray();
        }
    }
}