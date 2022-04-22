using System;

namespace CustomConsole
{
    public sealed class TypedSyntax : ISyntax
    {
        public TypedSyntax(KeyWord[] keywords, IVarType[] possibleTypes, IVarType returnType, ExecuteHandle handle)
        {
            Keywords = keywords;
            PossibleTypes = possibleTypes;
            Handle = handle;
            ReturnType = returnType;

            InputCount = 0;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i] == KeyWord.UnknownInput)
                {
                    InputCount++;
                }
            }
        }
        public TypedSyntax(KeyWord[] keywords, IVarType[] possibleTypes, IVarType returnType, ICodeFormat codeFormat, ExecuteHandle handle)
            : this(keywords, possibleTypes, returnType, handle)
        {
            DisplayFormat = codeFormat;
        }

        public KeyWord[] Keywords { get; }
        public ExecuteHandle Handle { get; }
        public IVarType ReturnType { get; }
        public IVarType[] PossibleTypes { get; }
        public int InputCount { get; }

        public ICodeFormat DisplayFormat { get; set; } = new DefaultCodeFormat();

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
                    // Next found keyword left no space for input syntax,
                    // the current keyword match is incorrect
                    if ((i == 0) && (wordIndex > 0))
                    {
                        continue;
                    }

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
                        int end = SyntaxPasser.FindClosingBracket(code, i);

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

            // Reached end of this syntaxes keywords
            return wordIndex == Keywords.Length;
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code)
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
                    // Next found keyword left no space for input syntax,
                    // the current keyword match is incorrect
                    if ((i == 0) && (wordIndex > 0))
                    {
                        continue;
                    }

                    wordIndex++;
                    continue;
                }
            }

            // Reached end of this syntaxes keywords
            return wordIndex == Keywords.Length;
        }
        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, out int index, bool fill)
        {
            index = 0;

            if (code.Length == 0) { return null; }

            int inputIndex = 0;
            Executable[] subExes = new Executable[InputCount];

            IVarType[] inputTypes = new IVarType[InputCount];
            IVarType highestType = type;

            for (int i = 0; i < Keywords.Length; i++)
            {
                // Input
                if (Keywords[i].Type == KeyWordType.Input)
                {
                    ReadOnlySpan<KeyWord> slice = code[index..];

                    KeyWord nextKW = new KeyWord();
                    if (Keywords.Length > (i + 1))
                    {
                        nextKW = Keywords[i + 1];

                        int lastI = slice.FindLastIndex(k => k.Word == nextKW.Word);
                        slice = slice.Slice(0,
                            lastI + 1 > slice.Length ? lastI : (lastI + 1));
                    }

                    Executable e = source.FindCorrectSyntax(slice, this, Keywords[i].InputType, nextKW, fill && Keywords.Length == (i + 1), out int addIndex);
                    index += addIndex;

                    // No valid syntax to match input could be found
                    if (e == null) { return null; }

                    if (Keywords[i].InputType == VarType.NonVoid)
                    {
                        // Type not accepted
                        if (!ValidType(e.ReturnType)) { return null; }

                        IVarType vart;
                        try
                        {
                            vart = highestType.GetHigherType(e.ReturnType);
                        }
                        // Type not accepted
                        catch (ConsoleException) { return null; }

                        highestType = vart;
                    }

                    inputTypes[inputIndex] = Keywords[i].InputType;

                    subExes[inputIndex] = e;
                    inputIndex++;
                    continue;
                }

                // Syntax doesn't match
                if (code[index].Word != Keywords[i].Word) { return null; }

                index++;
            }

            KeyWord[] kws = new KeyWord[Keywords.Length];

            // Replace typed input types with highest type
            for (int i = 0; i < kws.Length; i++)
            {
                if (Keywords[i].Type == KeyWordType.Input &&
                    Keywords[i].InputType == VarType.NonVoid)
                {
                    kws[i] = new KeyWord(highestType);
                    continue;
                }

                kws[i] = Keywords[i];
            }

            return new Executable(this, kws, subExes, Handle,
                ReturnType == VarType.NonVoid ? highestType : ReturnType);
        }

        private bool ValidType(IVarType type)
        {
            for (int i = 0; i < PossibleTypes.Length; i++)
            {
                if (type.Compatible(PossibleTypes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            Executable e = CorrectSyntax(code, type, source, out int i, true);

            // Not correct syntax
            if (code.Length != i) { return null; }

            return e;
        }
    }
}