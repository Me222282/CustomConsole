using System;

namespace CustomConsole
{
    public sealed class SyntaxTyped : ISyntax
    {
        public SyntaxTyped(KeyWord[] keywords, VarTypeGroup[] typeTs, IVarType returnType, ExecuteHandle handle)
        {
            Keywords = keywords;
            UnknownTypes = typeTs ?? new VarTypeGroup[0];
            Handle = handle;

            if (returnType is TypedVar tv)
            {
                if (UnknownTypes.Length < tv.Id + 1)
                {
                    throw new Exception($"Unsupport {tv.Name}");
                }

                ReturnType = UnknownTypes[tv.Id];
            }
            else
            {
                ReturnType = returnType;
            }

            InputCount = 0;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i] == KeyWord.UnknownInput)
                {
                    InputCount++;
                }
            }
        }
        public SyntaxTyped(KeyWord[] keywords, VarTypeGroup[] typeTs, IVarType returnType, ICodeFormat codeFormat, ExecuteHandle handle)
            : this(keywords, typeTs, returnType, handle)
        {
            DisplayFormat = codeFormat;
        }

        public KeyWord[] Keywords { get; }
        public ExecuteHandle Handle { get; }
        public IVarType ReturnType { get; }
        public VarTypeGroup[] UnknownTypes { get; }
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

                if (wordIndex == Keywords.Length) { break; }
            }

            // Reached end of this syntaxes keywords
            return wordIndex == Keywords.Length;
        }
        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = 0;

            if (code.Length == 0) { return null; }

            int inputIndex = 0;
            Executable[] subExes = new Executable[InputCount];

            IVarType[] inputTypes = new IVarType[InputCount];
            IVarType[] highestType = new IVarType[UnknownTypes.Length];
            Array.Fill(highestType, VarType.Any);

            for (int i = 0; i < Keywords.Length; i++)
            {
                // Input
                if (Keywords[i].Type == KeyWordType.Input)
                {
                    ReadOnlySpan<KeyWord> slice = code[index..];

                    KeyWord nextKW = nextKeyword;
                    if (Keywords.Length > (i + 1))
                    {
                        nextKW = Keywords[i + 1];

                        int lastI = slice.FindLastIndex(k => k.Word == nextKW.Word);
                        slice = slice.Slice(0,
                            lastI + 1 > slice.Length ? lastI : (lastI + 1));
                    }

                    IVarType expectedType = Keywords[i].InputType;
                    if (Keywords[i].InputType is TypedVar tv1)
                    {
                        if (highestType.Length < tv1.Id + 1)
                        {
                            throw new Exception($"Unsupport {tv1.Name}");
                        }

                        expectedType = UnknownTypes[tv1.Id];
                    }

                    Executable e = source.FindCorrectSyntax(slice, new LastFind(code, this), expectedType, nextKW, fill && Keywords.Length == (i + 1), out int addIndex);
                    index += addIndex;

                    // No valid syntax to match input could be found
                    if (e == null) { return null; }

                    if (Keywords[i].InputType is TypedVar tv2)
                    {
                        // Type not accepted
                        if (!ValidType(e.ReturnType, tv2.Id)) { return null; }

                        IVarType vart;
                        try
                        {
                            vart = highestType[tv2.Id].GetHigherType(e.ReturnType);
                        }
                        // Type not accepted
                        catch (ConsoleException) { return null; }

                        highestType[tv2.Id] = vart;
                    }

                    inputTypes[inputIndex] = Keywords[i].InputType;

                    subExes[inputIndex] = e;
                    inputIndex++;
                    continue;
                }

                // Syntax doesn't match
                if (!(code.Length > index && code[index].Word == Keywords[i].Word)) { return null; }

                index++;
            }

            KeyWord[] kws = new KeyWord[Keywords.Length];

            // Replace typed input types with highest type
            for (int i = 0; i < kws.Length; i++)
            {
                if (Keywords[i].Type == KeyWordType.Input &&
                    Keywords[i].InputType is TypedVar tv3)
                {
                    if (highestType.Length < tv3.Id + 1)
                    {
                        throw new Exception($"Unsupport {tv3.Name}");
                    }

                    kws[i] = new KeyWord(highestType[tv3.Id]);
                    continue;
                }

                kws[i] = Keywords[i];
            }

            IVarType returnType = ReturnType;
            if (returnType is TypedVar tv4)
            {
                if (highestType.Length < tv4.Id + 1)
                {
                    throw new Exception($"Unsupport {tv4.Name}");
                }

                returnType = highestType[tv4.Id];
            }
            return new Executable(this, kws, subExes, Handle, returnType);
        }

        private bool ValidType(IVarType type, uint idT) => type.Compatible(UnknownTypes[idT]);

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            Executable e = CorrectSyntax(code, type, source, new KeyWord(), out int i, true);

            // Not correct syntax
            if (code.Length != i) { return null; }

            return e;
        }
    }
}