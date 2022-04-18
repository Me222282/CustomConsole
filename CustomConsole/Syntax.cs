namespace CustomConsole
{
    public abstract class Syntax
    {
        public Syntax(VariableType returnType, ExecuteHandle handle)
        {
            ReturnType = returnType;
            Handle = handle;
        }

        public virtual KeyWord[] Keywords { get; }
        public virtual ExecuteHandle Handle { get; }
        public VariableType ReturnType { get; }

        public bool PotentialMatch(KeyWord[] code)
        {
            int wordIndex = 0;

            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].Word == Keywords[wordIndex].Word)
                {
                    // Reached end of this syntax keywords but not the end of the code,
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
                if (Keywords[wordIndex].Word == "")
                {
                    continue;
                }

                // Reaching this point means that the code doesn't match this syntax Keywords
                return false;
            }

            return true;
        }

        public Executable CreateInstance(KeyWord[] code)
        {
            // Find the sections that are other syntax
            // Find the most valid syntax from those keywords
            // Create instance of those sub syntax to use in this instance

            return new Executable(Keywords, null, Handle);
        }
    }
}
