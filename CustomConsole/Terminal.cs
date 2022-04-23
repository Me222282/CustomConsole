using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CustomConsole
{
    public static class Terminal
    {
        private class CDSyntax : ISyntax
        {
            public KeyWord[] Keywords { get; } = new KeyWord[] { new KeyWord("cd", KeyWordType.Word) };
            public int InputCount => 0;
            public IVarType ReturnType => VarType.Void;
            public ICodeFormat DisplayFormat => new DefaultCodeFormat();

            public bool ValidSyntax(ReadOnlySpan<KeyWord> code) => code[0].Word == "cd";
            public bool PossibleSyntax(ReadOnlySpan<KeyWord> code) => code[0].Word == "cd";

            public Executable CorrectSyntax(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source, KeyWord nextKeyword, out int index, bool fill)
            {
                index = code.Length;

                if (!fill || code[0].Word != "cd") { return null; }

                StringBuilder text = new StringBuilder();

                for (int i = 1; i < code.Length; i++)
                {
                    if (code[i].Word == "\"") { continue; }

                    text.Append(code[i].Word);
                }

                string path = text.ToString();
                return new Executable(this, code.ToArray(), null, _ =>
                {
                    //string pathFull = Path.Combine(Directory, path);
                    string pathFull = Path.GetFullPath(path, Directory);

                    if (System.IO.Directory.Exists(pathFull))
                    {
                        Directory = pathFull;
                        return null;
                    }

                    if (System.IO.Directory.Exists(path))
                    {
                        Directory = path;
                        return null;
                    }

                    Log("Path could not be found");

                    return null;
                }, VarType.Void);
            }

            public Executable CreateInstance(ReadOnlySpan<KeyWord> code, IVarType type, SyntaxPasser source)
            {
                return CorrectSyntax(code, type, source, new KeyWord(), out _, true);
            }
        }

        static Terminal()
        {
            SyntaxPasser.Syntaxes.Insert(0, new CommandSyntax());
            SyntaxPasser.Syntaxes.Add(new CDSyntax());

            SyntaxPasser.Variables.AddRange(new Variable[]
            {
                new Variable("name", VarType.String, () =>
                {
                    return Name;
                }, obj =>
                {
                    Name = (string)obj;
                }),

                new Variable("dir", VarType.String, () =>
                {
                    return Directory;
                }, obj =>
                {
                    string path = (string)obj;

                    if (!System.IO.Directory.Exists(path))
                    {
                        throw new ConsoleException("Path could not be found");
                    }

                    Environment.CurrentDirectory = path;
                }),

                new Variable("user", VarType.String, () =>
                {
                    return Environment.UserName;
                }, obj =>
                {
                    throw new ConsoleException("Variable cannot be set");
                }),

                new Variable("PATH", VarType.String, () =>
                {
                    return Environment.GetEnvironmentVariable("PATH");
                }, obj =>
                {
                    Environment.SetEnvironmentVariable("PATH", (string)obj);
                })
            });

            CommandSyntax.Commands.AddRange(new Command[]
            {
                new Command("clear", new CommandProperty[]
                {
                    new CommandProperty("c"),
                    new CommandProperty("h")
                }, objs =>
                {
                    bool c = (bool)objs[0];
                    bool h = (bool)objs[1];

                    if ((c && h)
                        ||
                        (!c && !h))
                    {
                        _lines.Clear();
                        _history.Clear();
                        return;
                    }

                    if (c)
                    {
                        _lines.Clear();
                        return;
                    }
                    if (h)
                    {
                        _history.Clear();
                        return;
                    }
                }),
                new Command("close", null, objs =>
                {
                    Environment.Exit(0);
                }),
                new Command("hx", null, objs =>
                {
                    if (_history.Count == 0)
                    {
                        Log("No command history");
                        return;
                    }

                    for (int i = 0; i < _history.Count; i++)
                    {
                        Log($"{i + 1}: {_history[i]}");
                    }
                }),
                new Command("dir", null, objs =>
                {
                    Log(Directory);
                })/*,
                new Command("cd", new CommandProperty[] { new CommandProperty("directory", VarType.String) }, objs =>
                {
                    string path = (string)objs[0];

                    //string pathFull = Path.Combine(Directory, path);
                    string pathFull = Path.GetFullPath(path, Directory);

                    if (System.IO.Directory.Exists(pathFull))
                    {
                        Directory = pathFull;
                        return;
                    }

                    if (System.IO.Directory.Exists(path))
                    {
                        Directory = path;
                        return;
                    }

                    Log("Path could not be found");
                })*/
            });
        }

        private static readonly List<string> _history = new List<string>(256);
        private static readonly List<string> _lines = new List<string>(256);

        public static string Directory
        {
            get => Environment.CurrentDirectory;
            set => Environment.CurrentDirectory = value;
        }

        private static string _name = Environment.UserName;
        public static string Name
        {
            get => _name;
            set
            {
                if (value.Contains('\n') || value.Contains('\r'))
                {
                    throw new ConsoleException("Console Name cannot contain new lines characters");
                }

                _name = value;
            }
        }

        public static void ExecuteCommand(string text)
        {
            text = text.Trim();

            // No value
            if (text.Length < 1) { return; }

            SyntaxPasser sp = new SyntaxPasser();

            Executable e;
            try
            {
                e = sp.Decode(text.FindKeyWords());
            }
            catch (ConsoleException ex)
            {
                Log(ex.Message);
                return;
            }

            if (e == null) { return; }

            string sc = e.SourceCode;
            if (sc != "hx")
            {
                _history.Add(sc);
            }

            try
            {
                object o = e.Execute();

                if (o != null)
                {
                    Log(o.ToString());
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return;
            }
        }

        public static event EventHandler<string> OnLog;

        public static void AddFunction(string name, IVarType[] paramTypes, IVarType returnType, ExecuteHandle callcack)
        {
            SyntaxPasser.Functions.Add(new Function(name, paramTypes, returnType, callcack));
        }
        public static void AddVariable(string name, IVarType type, VariableGetter get, VariableSetter set)
        {
            SyntaxPasser.Variables.Add(new Variable(name, type, get, set));
        }
        public static void Log(string value)
        {
            _lines.Add(value);

            OnLog?.Invoke(null, value);
        }
        public static void NewLine()
        {
            _lines.Add("");
        }

        public static List<string> Output => _lines;
    }
}