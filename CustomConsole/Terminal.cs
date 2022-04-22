using System;
using System.Collections.Generic;

namespace CustomConsole
{
    public static class Terminal
    {
        static Terminal()
        {
            SyntaxPasser.Syntaxes.Insert(0, new CommandSyntax());

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
                new Command("clear", new char[] { 'c', 'h' }, objs =>
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
                }),
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