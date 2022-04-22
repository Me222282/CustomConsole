using System;
using System.Collections.Generic;

namespace CustomConsole
{
    public class FunctionSyntax : ISyntax
    {
        private static readonly string[] _preChars = new string[] { ")", "(", ".", "," };
        private static readonly string[] _postChars = new string[] { ")", "(", "." };

        public KeyWord[] Keywords { get; } = new KeyWord[]
        {
            new KeyWord(null, KeyWordType.Word),
            new KeyWord("(", KeyWordType.BracketOpen),
            new KeyWord(")", KeyWordType.BracketClosed)
        };
        public int InputCount => -1;
        public IVarType ReturnType => VarType.Any;
        public ICodeFormat DisplayFormat => new CodeFormat(_preChars, _postChars);

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code)
        {
            // No space for smallest function
            if (code.Length < 3) { return false; }

            // Functions must end with closing bracket
            if (code[^1].Word != ")") { return false; }

            int bracketI = 0;

            string[] path = new string[code.Length / 2];
            int pathLength = 0;

            for (int i = 0; i < code.Length; i++)
            {
                // End of function name
                if (code[i].Word == "(")
                {
                    bracketI = i;
                    break;
                }

                // Invalid syntax for function name
                if ((i % 2 == 0 &&
                    code[i].Type != KeyWordType.Word)
                        ||
                    (i % 2 == 1 &&
                    code[i].Word != "."))
                {
                    return false;
                }

                if (code[i].Type != KeyWordType.Word) { continue; }

                path[i / 2] = code[i].Word;
                pathLength++;
            }

            // No space for function name
            if (bracketI == 0) { return false; }

            return FuncExists(path, pathLength);
        }
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => true;

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, out int index, bool fill)
        {
            index = 0;

            if (code.Length < 3) { return null; }

            string[] path = new string[code.Length / 2];
            int pathLength = 0;

            for (int i = 0; i < code.Length; i++)
            {
                // End of function name
                if (code[i].Word == "(")
                {
                    index = i;
                    break;
                }

                // Invalid syntax for function name
                if ((i % 2 == 0 &&
                    code[i].Type != KeyWordType.Word)
                        ||
                    (i % 2 == 1 &&
                    code[i].Word != "."))
                {
                    return null;
                }

                if (code[i].Type != KeyWordType.Word) { continue; }

                path[i / 2] = code[i].Word;
                pathLength++;
            }

            if (index == 0 || pathLength == 0) { return null; }

            index++;

            Function func = FindFunc(path, pathLength, type);
            // No valid function was found
            if (func == null) { return null; }

            Executable[] paramEs = new Executable[func.Parameters.Length];

            for (int i = 0; i < paramEs.Length; i++)
            {
                KeyWord nextK = new KeyWord(",", KeyWordType.Special);
                // Last item
                if (paramEs.Length == (i + 1))
                {
                    nextK = new KeyWord(")", KeyWordType.BracketClosed);
                }

                Executable e = source.FindCorrectSyntax(code[index..], this, func.Parameters[i], nextK, false, out int addIndex);
                index += addIndex + 1;

                // No valid syntax could be found
                if (e == null) { return null; }

                paramEs[i] = e;
            }

            // Include ending bracket
            if (paramEs.Length == 0) { index++; }

            if ((code.Length > (index - 1) &&
                    code[index - 1].Word != ")")
                        ||
                    code.Length < index)
            { return null; }

            List<KeyWord> kws = new List<KeyWord>(index);

            // Add name
            for (int i = 0; i < pathLength; i++)
            {
                kws.Add(new KeyWord(path[i], KeyWordType.Word));

                // Last index
                if (pathLength == (i + 1)) { break; }

                kws.Add(new KeyWord(".", KeyWordType.Special));
            }
            kws.Add(new KeyWord("(", KeyWordType.BracketOpen));

            // Add parameters
            for (int i = 0; i < func.Parameters.Length; i++)
            {
                kws.Add(new KeyWord(func.Parameters[i]));

                // Last index
                if (func.Parameters.Length == (i + 1)) { break; }

                kws.Add(new KeyWord(",", KeyWordType.Special));
            }
            kws.Add(new KeyWord(")", KeyWordType.BracketClosed));

            return new Executable(this, kws.ToArray(), paramEs, func.Handle, func.ReturnType);
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            return CorrectSyntax(code, type, source, out _, true);
        }

        private static bool FuncExists(string[] path, int length)
        {
            if (length == 0) { return false; }

            return SyntaxPasser.Functions.Exists(f =>
            {
                if (f.Path.Length != length) { return false; }

                for (int i = 0; i < length; i++)
                {
                    if (f.Path[i] != path[i])
                    {
                        return false;
                    }
                }

                return true;
            });
        }
        private static Function FindFunc(string[] path, int length, IVarType returnType)
        {
            if (length == 0) { return null; }

            return SyntaxPasser.Functions.Find(f =>
            {
                if (f.Path.Length != length) { return false; }

                // Null is Void
                if (f.ReturnType == null &&
                    returnType != null &&
                    returnType != VarType.Any) { return false; }
                // Return type isn't correct
                if (f.ReturnType != null && !f.ReturnType.Compatible(returnType)) { return false; }

                for (int i = 0; i < length; i++)
                {
                    if (f.Path[i] != path[i]) { return false; }
                }

                return true;
            });
        }
    }
}
