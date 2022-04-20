using System;
using System.Collections.Generic;

namespace CustomConsole
{
    public class Syntax : ISyntax
    {
        public Syntax(KeyWord[] keywords, VariableType returnType, ExecuteHandle handle)
        {
            Keywords = keywords;
            ReturnType = returnType;
            Handle = handle;

            int inputIndex = 0;
            List<VariableType> inputs = new List<VariableType>();

            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i] == KeyWord.UnknownInput)
                {
                    inputs.Add(keywords[i].InputType);
                    inputIndex++;
                }
            }

            InputTypes = inputs.ToArray();
        }

        public KeyWord[] Keywords { get; }
        public VariableType[] InputTypes { get; }
        public ExecuteHandle Handle { get; }
        public VariableType ReturnType { get; }
        public int InputCount => InputTypes.Length;

        public ICodeFormat DisplayFormat { get; } = new DefaultFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            if (code.Length == 0) { return false; }

            int wordIndex = 0;

            for (int i = 0; i < code.Length; i++)
            {
                // Input text - need to find next keyword
                if (Keywords.Length > wordIndex &&
                    Keywords[wordIndex].Word == "") { wordIndex++; }

                if (Keywords.Length > wordIndex &&
                    code[i].Word == Keywords[wordIndex].Word)
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
                    if (code[i].Word == "(")
                    {
                        int end = FindClosingBracket(code, i);

                        if (end == -1)
                        {
                            throw new ConsoleException("Invalid syntax - no closing bracket");
                        }

                        i = end;
                    }

                    continue;
                }

                // Reaching this point means that the code doesn't match this syntaxes Keywords
                return false;
            }

            return wordIndex == Keywords.Length;
        }
        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param)
        {
            bool fill = param is bool b && b;

            index = 0;

            if (code.Length == 0) { return null; }

            int inputIndex = 0;
            Executable[] subExes = new Executable[InputCount];

            for (int i = 0; i < Keywords.Length; i++)
            {
                // Input
                if (Keywords[i].Type == KeyWordType.Input)
                {
                    KeyWord nextKW = new KeyWord();
                    if (Keywords.Length > (i + 1))
                    {
                        nextKW = Keywords[i + 1];
                    }

                    Executable e = FindCorrectSyntax(code[index..], this, Keywords[i].InputType, nextKW, fill && Keywords.Length == (i + 1), out int addIndex);
                    index += addIndex;

                    // No valid syntax to match input could be found
                    if (e == null) { return null; }

                    subExes[inputIndex] = e;
                    inputIndex++;
                    continue;
                }

                // Syntax doesn't match
                if (code[index].Word != Keywords[i].Word)
                {
                    return null;
                }

                index++;
            }

            return new Executable(this, Keywords, subExes, Handle);
        }

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            Executable e = CorrectSyntax(code, type, out int i, true);

            // Not correct syntax
            if (code.Length != i) { return null; }

            return e;
        }

        public static List<ISyntax> Syntaxes { get; }
        public static Executable Decode(ReadOnlySpan<KeyWord> syntax, VariableType returnType = VariableType.Any)
        {
            if (syntax.Length == 0) { return null; }

            for (int i = 0; i < Syntaxes.Count; i++)
            {
                // Doesn't return correct type
                if (!returnType.Compatable(Syntaxes[i].ReturnType)) { continue; }

                bool could = Syntaxes[i].ValidSyntax(syntax);

                if (could)
                {
                    Executable e = Syntaxes[i].CreateInstance(syntax, returnType);

                    if (e == null) { continue; }

                    if (e.Source != Syntaxes[i])
                    {
                        throw new Exception("Invalid return from syntax");
                    }

                    return e;
                }
            }

            if (syntax[0].Word == "(")
            {
                Executable e = ManageBrackets(syntax, returnType, new KeyWord(), true, out int i);

                if (i != syntax.Length)
                {
                    throw new ConsoleException("Unknown syntax");
                }

                return e;
            }

            throw new ConsoleException("Unknown syntax");
        }

        public static Executable FindCorrectSyntax(ReadOnlySpan<KeyWord> syntax, ISyntax source, VariableType returnType, KeyWord nextWord, bool fill, out int nextIndex)
        {
            nextIndex = 0;

            if (syntax.Length == 0) { return null; }
            if (nextWord.Word != null && !syntax.Contains(nextWord)) { return null; }

            if (syntax[0].Word == "(")
            {
                Executable exe = ManageBrackets(syntax, returnType, nextWord, fill, out nextIndex);
                
                if (exe != null) { return exe; }
            }

            for (int i = 0; i < Syntaxes.Count; i++)
            {
                if (!fill && Syntaxes[i].EqualKeyWords(source)) { continue; }

                // Doesn't return correct type
                if (!returnType.Compatable(Syntaxes[i].ReturnType)) { continue; }

                Executable e = Syntaxes[i].CorrectSyntax(syntax, returnType, out nextIndex, fill);

                // Not correct syntax
                if (e == null) { continue; }

                // Syntax needs to decode all the keywords
                if (fill && nextIndex != syntax.Length) { continue; }

                // There is a next keyword
                if (!fill && nextWord.Word != null &&

                    // Syntax is long enough
                    ((syntax.Length > nextIndex &&
                    // Next keyword doesn't match
                    syntax[nextIndex] != nextWord) ||

                    // Syntax isn't long enough
                    syntax.Length < (nextIndex + 1)))
                {
                    continue;
                }

                return e;
            }

            return null;
        }
        private static Executable ManageBrackets(ReadOnlySpan<KeyWord> syntax, VariableType returnType, KeyWord nextWord, bool fill, out int nextIndex)
        {
            int end = FindClosingBracket(syntax, 0);

            // Couldn't find closing bracket
            if (end == -1)
            {
                throw new ConsoleException("Invalid syntax - no closing bracket");
            }

            nextIndex = end + 1;

            // There is a next keyword
            if (!fill && nextWord.Word != null &&

                // Syntax is long enough
                ((syntax.Length > nextIndex &&
                // Next keyword doesn't match
                syntax[nextIndex] != nextWord) ||

                // Syntax isn't long enough
                syntax.Length < (nextIndex + 1)))
            {
                return null;
            }

            ReadOnlySpan<KeyWord> bracketCode = syntax[1..end];

            Executable e;
            try { e = Decode(bracketCode, returnType); }
            catch (ConsoleException) { return null; }
            return e;
        }
        private static int FindClosingBracket(ReadOnlySpan<KeyWord> syntax, int openBrackIndex)
        {
            int bracketNumber = syntax[openBrackIndex].Info;

            for (int i = openBrackIndex + 1; i < syntax.Length; i++)
            {
                // Found closing breacket
                if (syntax[i].Word == ")" && syntax[i].Info == bracketNumber) { return i; }
            }

            return -1;
        }

        public static List<Variable> Variables { get; } = new List<Variable>();

        static Syntax()
        {
            Syntaxes = new List<ISyntax>()
            {
                new IntegerSyntax(),
                new FloatSyntax(),
                new DoubleSyntax(),
                new StringSyntax(),
                new CharSyntax(),
                new BoolSyntax(),
                new GetVariableSyntax(),
                new SetVariableSyntax(),


            };
        }
    }
}