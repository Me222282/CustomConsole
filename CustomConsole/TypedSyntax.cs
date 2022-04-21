using System;

namespace CustomConsole
{
    public sealed class TypedSyntax : ISyntax
    {
        public TypedSyntax(KeyWord[] keywords, VariableType[] possibleTypes, ExecuteHandle handle)
        {
            Keywords = keywords;
            PossibleTypes = possibleTypes;
            Handle = handle;

            InputCount = 0;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i] == KeyWord.UnknownInput)
                {
                    InputCount++;
                }
            }
        }

        public KeyWord[] Keywords { get; }
        public ExecuteHandle Handle { get; }
        public VariableType ReturnType => VariableType.NonVoid;
        public VariableType[] PossibleTypes { get; }
        public int InputCount { get; }

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
                        int end = Syntax.FindClosingBracket(code, i);

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
        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, VariableType type, out int index, object param)
        {
            bool fill = param is bool b && b;

            index = 0;

            // Type not accepted
            if (!ValidType(type)) { return null; }

            if (code.Length == 0) { return null; }

            int inputIndex = 0;
            Executable[] subExes = new Executable[InputCount + 1];

            VariableType expectedType = type;

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

                    Executable e = Syntax.FindCorrectSyntax(slice, this, VariableType.NonVoid, nextKW, fill && Keywords.Length == (i + 1), out int addIndex);
                    index += addIndex;

                    // No valid syntax to match input could be found
                    if (e == null) { return null; }

                    VariableType vart = GetHigherType(expectedType, GetReturnType(e));
                    if (vart < 0) { return null; }
                    expectedType = vart;

                    // First input
                    if (inputIndex == 0 &&
                        e.Source.ReturnType != VariableType.NonVoid &&
                        e.Source.ReturnType != VariableType.Any)
                    {
                        expectedType = e.Source.ReturnType;
                    }

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

            VariableType[] vartary = GetArray(InputCount, expectedType);

            // The passes type that this instance contains
            subExes[^1] = new Executable(this, null, null, _ => expectedType, vartary);

            return new Executable(this, Keywords, subExes, Handle, vartary);
        }

        private static VariableType GetReturnType(Executable e)
        {
            if (e.Source is TypedSyntax)
            {
                return (VariableType)e.SubExecutables[^1].Execute();
            }

            return e.Source.ReturnType;
        }
        private static VariableType GetHigherType(VariableType old, VariableType @new)
        {
            if (old == VariableType.Any || old == VariableType.NonVoid)
            {
                return @new;
            }

            if (old.Compatable(@new)) { return old; }

            switch (old)
            {
                case VariableType.Int:
                    if (@new == VariableType.Float)
                    {
                        return VariableType.Float;
                    }
                    if (@new == VariableType.Double)
                    {
                        return VariableType.Double;
                    }
                    break;

                case VariableType.Float:
                    if (@new == VariableType.Double)
                    {
                        return VariableType.Double;
                    }
                    break;

                case VariableType.Vector3:
                    if (@new == VariableType.Vector2)
                    {
                        return VariableType.Vector2;
                    }
                    break;

                case VariableType.Vector4:
                    if (@new == VariableType.Vector3)
                    {
                        return VariableType.Vector3;
                    }
                    if (@new == VariableType.Vector2)
                    {
                        return VariableType.Vector2;
                    }
                    break;
            }

            return (VariableType)(-1);
        }

        private static VariableType[] GetArray(int count, VariableType value)
        {
            VariableType[] array = new VariableType[count];

            Array.Fill(array, value);

            return array;
        }

        private bool ValidType(VariableType type)
        {
            for (int i = 0; i < PossibleTypes.Length; i++)
            {
                if (type.Compatable(PossibleTypes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, VariableType type)
        {
            Executable e = CorrectSyntax(code, type, out int i, true);

            // Not correct syntax
            if (code.Length != i) { return null; }

            return e;
        }
    }
}