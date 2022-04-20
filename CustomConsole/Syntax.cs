using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomConsole
{
    public class Syntax : ISyntax
    {
        public Syntax(KeyWord[] keywords, VariableType returnType, ExecuteHandle handle)
        {
            Keywords = keywords;
            ReturnType = returnType;
            Handle = handle;

            // Keywords contain an input of any type
            ContainsInput = keywords.Contains(KeyWord.UnknownInput);
        }

        public KeyWord[] Keywords { get; }
        public ExecuteHandle Handle { get; }
        public VariableType ReturnType { get; }
        public bool ContainsInput { get; }

        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            if (code.Length == 0) { return false; }

            int wordIndex = 0;

            for (int i = 0; i < code.Length; i++)
            {
                // Input text - need to find next keyword
                if (Keywords[wordIndex].Word == "") { wordIndex++; }

                if (code[i].Word == Keywords[wordIndex].Word)
                {
                    // Reached end of this syntaxes keywords but not the end of the code,
                    // the current keyword match is incorrect
                    if (code.Length != (i + 1) &&
                        Keywords.Length == (wordIndex + 1))
                    {
                        continue;
                    }

                    wordIndex++;
                    continue;
                }

                // Empty string means input syntax - it could be any length
                if ((wordIndex - 1 >= 0) && Keywords[wordIndex - 1].Word == "")
                {
                    continue;
                }

                // Reaching this point means that the code doesn't match this syntaxes Keywords
                return false;
            }

            return true;
        }

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code)
        {
            if (!ContainsInput)
            {
                return new Executable(this, Keywords, null, Handle);
            }

            // Find the sections that are other syntax
            // Find the most valid syntax from those keywords
            // Create instance of those sub syntax to use in this instance

            return new Executable(this, Keywords, null, Handle);
        }

        public static List<ISyntax> Syntaxes { get; } = new List<ISyntax>();
        public static ExecultableSyntax FindSyntax(ReadOnlySpan<KeyWord> syntax, VariableType returnType = VariableType.Any)
        {
            for (int i = 0; i < Syntaxes.Count; i++)
            {
                // Doesn't return correct type
                if ((returnType & Syntaxes[i].ReturnType) != Syntaxes[i].ReturnType)
                {
                    continue;
                }

                bool could = Syntaxes[i].ValidSyntax(syntax);

                if (could)
                {
                    Executable e = Syntaxes[i].CreateInstance(syntax);

                    if (e == null) { continue; }

                    return new ExecultableSyntax(e, Syntaxes[i]);
                }
            }

            throw new ConsoleException("Unknown syntax");
        }
    }

    public struct ExecultableSyntax
    {
        public ExecultableSyntax(Executable e, ISyntax s)
        {
            Executable = e;
            Syntax = s;
        }

        public Executable Executable { get; }
        public ISyntax Syntax { get; }
    }
}
