using System;
using System.Collections.Generic;

namespace CustomConsole
{
    public sealed class ScriptPasser : Executable, ISyntax
    {
        public ScriptPasser()
            : base(CustomConsole.Syntax.Unknown, null, null, _ => null, VarType.Void)
        {
            _log = (str) => Console.WriteLine(str);
        }
        public ScriptPasser(Action<string> onLog)
            : base(CustomConsole.Syntax.Unknown, null, null, _ => null, VarType.Void)
        {
            _log = onLog ?? (_ => { });
        }

        private readonly Action<string> _log;

        public bool Executing { get; private set; } = false;
        public bool Executable { get; private set; } = false;
        public int Index { get; set; } = 0;
        public string LastError { get; private set; }
        public bool LogLineOutput { get; set; } = false;

        public KeyWord[] Keywords { get; private set; }
        public int InputCount => 0;
        public new IVarType ReturnType => VarType.Void;
        public ICodeFormat DisplayFormat { get; } = new DefaultCodeFormat();

        public bool ValidSyntax(ReadOnlySpan<KeyWord> code) => false;
        public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => false;

        public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
        {
            index = code.Length;

            Decode(code, null);
            return this;
        }
        public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
        {
            Decode(code, null);
            return this;
        }

        private Executable[] _executables = null;
        public override Executable[] SubExecutables => _executables;
        public override ISyntax Source => this;

        public void Decode(string sourceCode)
        {
            ReadOnlySpan<KeyWord> keywords = sourceCode.FindKeyWords(out int[] lines);

            Decode(keywords, lines);
        }

        private void Decode(ReadOnlySpan<KeyWord> keywords, int[] lines)
        {
            _executables = null;
            Keywords = null;
            Syntax = null;

            SyntaxPasser sp = new SyntaxPasser(this);

            List<Executable> executables = new List<Executable>();

            int lastLine = 0;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i].Word != ";") { continue; }

                Executable e;
                try
                {
                    e = sp.Decode(keywords[lastLine..i]);
                }
                catch (ConsoleException ce)
                {
                    Executable = false;
                    if (lines == null)
                    {
                        LastError = ce.Message;
                    }
                    else
                    {
                        LastError = $"{ce.Message} on line {lines[lastLine] + 1}";
                    }
                    _log(LastError);
                    return;
                }
                lastLine = i + 1;

                if (e == null) { continue; }

                executables.Add(e);
            }

            if (lastLine != keywords.Length)
            {
                Executable e;
                try
                {
                    e = sp.Decode(keywords[lastLine..keywords.Length]);
                }
                catch (ConsoleException ce)
                {
                    Executable = false;
                    if (lines == null)
                    {
                        LastError = ce.Message;
                    }
                    else
                    {
                        LastError = $"{ce.Message} on line {lines[lastLine] + 1}";
                    }
                    _log(LastError);
                    return;
                }

                if (e != null) { executables.Add(e); }
            }

            LastError = "";

            Keywords = new KeyWord[executables.Count];
            Array.Fill(Keywords, new KeyWord(VarType.Any));
            Syntax = Keywords;
            _executables = executables.ToArray();
            Executable = true;
        }

        public override object Execute()
        {
            if (_executables == null)
            {
                LastError = "Script has no content, nothing has been compiled";
                _log(LastError);
                return LastError;
            }

            Executing = true;
            for (Index = 0; Index < _executables.Length; Index++)
            {
                if (_executables[Index] == null)
                {
                    throw new Exception("Insufficient data, the last compilation had failed.");
                }

                object o;
                try
                {
                    o = _executables[Index].Execute();
                }
                catch (Exception e)
                {
                    LastError = $"{e.GetType().Name} was thrown with message \"{e.Message}\"";
                    _log(LastError);
                    return null;
                }

                if (LogLineOutput && o != null)
                {
                    _log(o.ToString());
                }
            }

            Executing = false;
            return null;
        }

        internal static ScriptPasser Default { get; } = new ScriptPasser();
    }
}
