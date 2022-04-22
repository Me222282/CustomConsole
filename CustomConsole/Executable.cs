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

            _inputTypes = GetInputTypes(syntax);
            ReturnType = returnType;
        }

        public ISyntax Source { get; }
        public KeyWord[] Syntax { get; }
        public Executable[] SubExecutables { get; }
        
        public IVarType ReturnType { get; }
        private readonly IVarType[] _inputTypes;

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
                ICodeFormat format = Source.DisplayFormat ?? new DefaultFormat();

                StringBuilder str = new StringBuilder();

                int inputCount = 0;

                for (int i = 0; i < Syntax.Length; i++)
                {
                    string word = Syntax[i].Word;

                    if (word == "")
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

                Console.WriteLine($"{this}, param {i} is type {@params[i].GetType()}");

                if (@params[i] is int @int)
                {
                    if (_inputTypes[i] == VarType.Float)
                    {
                        @params[i] = (float)@int;
                    }
                    else if (_inputTypes[i] == VarType.Double)
                    {
                        @params[i] = (double)@int;
                    }
                }
                if (@params[i] is float @float)
                {
                    if (_inputTypes[i] == VarType.Double)
                    {
                        @params[i] = (double)@float;
                    }
                }
                else if (@params[i] is Vector3 vector3)
                {
                    if (_inputTypes[i] == VarType.Vector2)
                    {
                        @params[i] = (Vector2)vector3;
                    }
                }
                else if (@params[i] is Vector4 vector4)
                {
                    if (_inputTypes[i] == VarType.Vector3)
                    {
                        @params[i] = (Vector3)vector4;
                    }
                    else if (_inputTypes[i] == VarType.Vector2)
                    {
                        @params[i] = (Vector2)vector4;
                    }
                }
            }

            return Function(@params);
        }

        public override string ToString()
        {
            return $"Executable: \"{SourceCode}\"";
        }

        private static IVarType[] GetInputTypes(KeyWord[] syntax)
        {
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