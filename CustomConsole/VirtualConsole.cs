using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Zene.Structs;

namespace CustomConsole
{
    public delegate object StringConverter(string str);

    public enum VariableType
    {
        Int,
        Float,
        Double,
        String,
        Char,
        Bool,

        Vector2,
        Vector3
    }

    public static class VirtualConsole
    {
        static VirtualConsole()
        {
            _variables = new List<Variable>(256)
            {
                new Variable("name", StringParam, () =>
                {
                    return Name;
                }, obj =>
                {
                    Name = (string)obj;
                }),

                new Variable("dir", StringParam, () =>
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

                new Variable("user", StringParam, () =>
                {
                    return Environment.UserName;
                }, obj =>
                {
                    throw new ConsoleException("Variable cannot be set");
                }),

                new Variable("PATH", StringParam, () =>
                {
                    return Environment.GetEnvironmentVariable("PATH");
                }, obj =>
                {
                    Environment.SetEnvironmentVariable("PATH", (string)obj);
                })
            };
        }

        private static readonly List<string> _history = new List<string>(256);
        private static readonly List<string> _lines = new List<string>(256);
        private static readonly List<Function> _functions = new List<Function>(256);
        private static readonly List<Variable> _variables;

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

            // Clear command
            if (text.Length >= 5 && text[0..5] == "clear")
            {
                string info = text[5..].Trim();

                if (info.Length == 0)
                {
                    _history.Clear();
                    _lines.Clear();
                    return;
                }

                if (info == "-c")
                {
                    _lines.Clear();

                    _history.Add(text);
                    return;
                }

                if (info == "-h")
                {
                    _history.Clear();
                    return;
                }

                if (info.Length < 4)
                {
                    Log("Invalid command parameters");
                    return;
                }

                if ((info[0] == '-' &&
                    info[^2] == '-') &&

                    (info[1] == 'c' &&
                    info[^1] == 'h') ||

                    (info[1] == 'h' &&
                    info[^1] == 'c'))
                {
                    if ((info.Length > 4) && (info[2..^3].Trim() != ""))
                    {
                        Log("Invalid command parameters");
                        return;
                    }

                    _history.Clear();
                    _lines.Clear();
                    return;
                }

                Log("Invalid command parameters");
                return;
            }
            // Close command
            if (text == "close")
            {
                Environment.Exit(0);
                return;
            }
            // History command
            if (text == "hx")
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

                return;
            }
            // Directory command
            if (text == "dir")
            {
                Log(Directory);

                _history.Add(text);
                return;
            }
            // Log command
            if (text.Length >= 3 && text[0..3] == "log")
            {
                string info = text[3..].Trim();

                if (info.Length == 0)
                {
                    Log("Invalid parameter");
                    return;
                }

                try
                {
                    Log((string)StringParam(info));
                }
                catch (ConsoleException e)
                {
                    Log(e.Message);
                }

                _history.Add(text);
                return;
            }
            // cd command
            if (text.Length >= 2 && text[0] == 'c' && text[1] == 'd')
            {
                string path = text[2..].Trim();

                if (path.Length == 0)
                {
                    Log("No path was provided");
                    return;
                }

                if ((path.Count(c => c == '\"') % 2) != 0)
                {
                    Log("Quotation marks weren't closed");
                    return;
                }

                path = path.Replace("\"", "");

                //string pathFull = Path.Combine(Directory, path);
                string pathFull = Path.GetFullPath(path, Directory);

                if (System.IO.Directory.Exists(pathFull))
                {
                    Directory = pathFull;

                    _history.Add(text);
                    return;
                }

                if (System.IO.Directory.Exists(path))
                {
                    Directory = path;

                    _history.Add(text);
                    return;
                }

                Log("Path could not be found");
                return;
            }

            // Variable
            if (text[0] == '$')
            {
                Variable var;
                string setting;

                try
                {
                    var = FindVariable(text, out setting);
                }
                catch (ConsoleException e)
                {
                    Log(e.Message);
                    return;
                }

                // Error was thrown
                if (var == null) { return; }

                if (setting == null)
                {
                    Log(var.Getter().ToString());
                    _history.Add(text);
                    return;
                }

                try
                {
                    var.Setter(var.ParamConverter(setting));
                }
                catch (ConsoleException e)
                {
                    Log(e.Message);
                    return;
                }
                _history.Add(text);
                return;
            }

            Function f;
            int paramIndex;

            try
            {
                f = FindFunction(text, out paramIndex);
            }
            catch (ConsoleException e)
            {
                Log(e.Message);
                return;
            }

            object[] paramData;
            int endIndex;

            try
            {
                paramData = f.GetParams(text[paramIndex..], out endIndex);
            }
            catch (ConsoleException e)
            {
                Log(e.Message);
                return;
            }

            try
            {
                f.Callback(paramData, text[(endIndex + paramIndex)..].Trim());
            }
            catch (ConsoleException e)
            {
                Log(e.Message);
                return;
            }

            _history.Add(text);
        }

        public static event EventHandler<string> OnLog;

        public static void AddFunction(string name, VariableType[] paramTypes, FunctionPasser callcack)
        {
            StringConverter[] paramConverters = new StringConverter[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                paramConverters[i] = paramTypes[i] switch
                {
                    VariableType.Int => IntParam,
                    VariableType.Float => FloatParam,
                    VariableType.Double => DoubleParam,
                    VariableType.String => StringParam,
                    VariableType.Char => CharParam,
                    VariableType.Bool => BoolParam,
                    VariableType.Vector2 => Vector2Param,
                    _ => throw new Exception("Invalid type"),
                };
            }

            _functions.Add(new Function(name, paramConverters, callcack));
        }
        public static void AddVariable(string name, VariableType type, VariableGetter get, VariableSetter set)
        {
            StringConverter sc = type switch
            {
                VariableType.Int => IntParam,
                VariableType.Float => FloatParam,
                VariableType.Double => DoubleParam,
                VariableType.String => StringParam,
                VariableType.Char => CharParam,
                VariableType.Bool => BoolParam,
                VariableType.Vector2 => Vector2Param,
                VariableType.Vector3 => Vector3Param,
                _ => throw new Exception("Invalid type"),
            };

            _variables.Add(new Variable(name, sc, get, set));

            if (type == VariableType.Vector2)
            {
                // X sub value
                _variables.Add(new Variable(name + ".x", DoubleParam, () =>
                {
                    return ((Vector2)get()).X;
                }, v =>
                {
                    set(new Vector2((double)v, ((Vector2)get()).Y));
                }));
                // Y sub value
                _variables.Add(new Variable(name + ".y", DoubleParam, () =>
                {
                    return ((Vector2)get()).Y;
                }, v =>
                {
                    set(new Vector2(((Vector2)get()).X, (double)v));
                }));

                return;
            }

            if (type == VariableType.Vector3)
            {
                // X sub value
                _variables.Add(new Variable(name + ".x", DoubleParam, () =>
                {
                    return ((Vector3)get()).X;
                }, v =>
                {
                    Vector3 old = (Vector3)get();

                    set(new Vector3((double)v, old.Y, old.Z));
                }));
                // Y sub value
                _variables.Add(new Variable(name + ".y", DoubleParam, () =>
                {
                    return ((Vector3)get()).Y;
                }, v =>
                {
                    Vector3 old = (Vector3)get();

                    set(new Vector3(old.X, (double)v, old.Z));
                }));
                // Z sub value
                _variables.Add(new Variable(name + ".z", DoubleParam, () =>
                {
                    return ((Vector3)get()).Z;
                }, v =>
                {
                    Vector3 old = (Vector3)get();

                    set(new Vector3(old.X, old.Y, (double)v));
                }));

                return;
            }
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

        private static Function FindFunction(string text, out int paramIndex)
        {
            StringBuilder name = new StringBuilder(32);

            paramIndex = text.Length - 1;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\n') { throw new ConsoleException("Invalid syntax"); }

                if (c == ' ' || c == '(')
                {
                    paramIndex = i;
                    break;
                }

                name.Append(c);
            }

            string nameStr = name.ToString();

            Function f = _functions.Find(f => f.Name == nameStr);

            if (f == null)
            {
                throw new ConsoleException("Unknown function");
            }

            return f;
        }
        private static Variable FindVariable(string text, out string setter)
        {
            StringBuilder name = new StringBuilder(32);

            bool foundName = false;

            setter = null;

            for (int i = 1; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\n')
                {
                    throw new ConsoleException("Invalid syntax");
                }

                // End of name
                if (c == ' ')
                {
                    foundName = true;
                    continue;
                }

                if (c == '=')
                {
                    if (text.Length < (i + 2))
                    {
                        throw new ConsoleException("Invalid syntax");
                    }

                    setter = text[(i + 1)..].Trim();
                    break;
                }

                if (foundName)
                {
                    throw new ConsoleException("Invalid syntax");
                }

                name.Append(c);
            }

            string nameStr = name.ToString();
            Variable v = _variables.Find(v => v.Name == nameStr);

            if (v == null)
            {
                throw new ConsoleException("Unknown variable");
            }

            return v;
        }

        private enum Operation
        {
            None = 0,
            Add,
            Subtract,
            Mutliply,
            Divide,
            Modulo,

            LeftShift,
            RightShift,
            Up,
            And,
            Or
        }

        public static object IntParam(string value)
        {
            bool number = false;
            bool endNumber = false;
            bool refName = false;
            bool negate = false;
            bool inBrackets = false;
            int bracketCount = 0;

            Operation oper = Operation.Add;
            StringBuilder input = new StringBuilder(32);

            int result = 0;

            void FromVar()
            {
                Variable var = FindVariable('$' + input.ToString(), out _);
                object obj = var.Getter();

                int val;

                try
                {
                    val = (int)obj;
                }
                catch (Exception)
                {
                    throw new ConsoleException("Variable not convertable to integer type");
                }

                SetValue(val, oper, negate, ref result);
                refName = false;
                input.Clear();
            }
            void ChangeNorm()
            {
                if (!endNumber)
                {
                    if (refName)
                    {
                        FromVar();
                    }
                    else
                    {
                        SetValue(input.ToString(), oper, negate, ref result);
                        input.Clear();
                    }

                    negate = false;
                }

                endNumber = false;
                number = false;
            }
            void ChangeDiff()
            {
                if (number)
                {
                    SetValue(input.ToString(), oper, negate, ref result);
                    input.Clear();
                    negate = false;
                }
                else if (refName)
                {
                    FromVar();
                }

                endNumber = false;
                number = false;
            }

            bool ls = false;
            bool rs = false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (inBrackets)
                {
                    if (c == '(')
                    {
                        bracketCount++;
                    }
                    if (c == ')')
                    {
                        bracketCount--;

                        if (bracketCount < 0)
                        {
                            inBrackets = false;
                            endNumber = true;

                            SetValue((int)IntParam(input.ToString()), oper, negate, ref result);
                            input.Clear();
                            oper = Operation.None;
                            negate = false;
                            continue;
                        }
                    }

                    input.Append(c);
                    continue;
                }

                switch (c)
                {
                    case '+':
                        ChangeDiff();

                        if (oper == Operation.None)
                        {
                            oper = Operation.Add;
                        }
                        continue;

                    case '-':
                        ChangeDiff();

                        if (oper == Operation.None)
                        {
                            oper = Operation.Subtract;
                            continue;
                        }
                        negate = !negate;
                        continue;

                    case '*':
                        ChangeNorm();
                        oper = Operation.Mutliply;
                        continue;

                    case '/':
                        ChangeNorm();
                        oper = Operation.Divide;
                        continue;

                    case '%':
                        ChangeNorm();
                        oper = Operation.Modulo;
                        continue;

                    case '>':
                        if (!ls && value.Length > (i + 1) && value[i + 1] != '>')
                        {
                            throw new ConsoleException("Invalid integer syntax");
                        }
                        else if (!ls)
                        {
                            ls = true;
                        }
                        // ls is true
                        else
                        {
                            ls = false;
                            continue;
                        }

                        ChangeNorm();
                        oper = Operation.LeftShift;
                        continue;

                    case '<':
                        if (!rs && value.Length > (i + 1) && value[i + 1] != '<')
                        {
                            throw new ConsoleException("Invalid integer syntax");
                        }
                        else if (!rs)
                        {
                            rs = true;
                        }
                        // rs is true
                        else
                        {
                            rs = false;
                            continue;
                        }

                        ChangeNorm();
                        oper = Operation.RightShift;
                        continue;

                    case '^':
                        ChangeNorm();
                        oper = Operation.Up;
                        continue;

                    case '&':
                        ChangeNorm();
                        oper = Operation.And;
                        continue;

                    case '|':
                        ChangeNorm();
                        oper = Operation.Or;
                        continue;

                    case ' ':
                        if (number)
                        {
                            endNumber = true;
                            number = false;
                            SetValue(input.ToString(), oper, negate, ref result);
                            oper = Operation.None;
                            negate = false;
                            input.Clear();
                            continue;
                        }
                        if (refName)
                        {
                            endNumber = true;

                            FromVar();

                            oper = Operation.None;
                            negate = false;
                            continue;
                        }

                        continue;

                    default:
                        if (endNumber)
                        {
                            throw new ConsoleException("Invalid integer syntax");
                        }
                        break;
                }

                if (refName)
                {
                    input.Append(c);
                    continue;
                }

                if (!number)
                {
                    if (char.IsNumber(c))
                    {
                        number = true;
                        input.Append(c);
                        continue;
                    }
                    if (c == '$')
                    {
                        refName = true;
                        number = false;
                        continue;
                    }
                    if (c == '(')
                    {
                        inBrackets = true;
                        continue;
                    }
                }
                else
                {
                    if (!char.IsNumber(c) && c != '.' && c != '(')
                    {
                        throw new ConsoleException("Invalid integer paramter");
                    }
                }

                input.Append(c);
                continue;
            }

            if ((!endNumber && !number && !refName) || inBrackets)
            {
                throw new ConsoleException("Invalid integer syntax");
            }

            if (number)
            {
                SetValue(input.ToString(), oper, negate, ref result);
            }
            if (refName)
            {
                FromVar();
            }

            return result;
        }

        private static void SetValue(string str, Operation o, bool neg, ref int i)
        {
            if (str.Length == 0)
            {
                throw new ConsoleException("Invalid integer parameter");
            }

            if (!int.TryParse(str, out int value))
            {
                throw new ConsoleException("Invalid integer parameter");
            }

            SetValue(value, o, neg, ref i);
        }
        private static void SetValue(int value, Operation o, bool neg, ref int i)
        {
            if (neg) { value = -value; }

            switch (o)
            {
                case Operation.Add:
                    i += value;
                    return;

                case Operation.Subtract:
                    i -= value;
                    return;

                case Operation.Mutliply:
                    i *= value;
                    return;

                case Operation.Divide:
                    i /= value;
                    return;

                case Operation.Modulo:
                    i %= value;
                    return;

                case Operation.LeftShift:
                    i >>= value;
                    return;

                case Operation.RightShift:
                    i <<= value;
                    return;

                case Operation.Up:
                    i ^= value;
                    return;

                case Operation.And:
                    i &= value;
                    return;

                case Operation.Or:
                    i |= value;
                    return;
            }
        }

        public static object FloatParam(string value)
        {
            float result;

            try
            {
                result = (float)(double)DoubleParam(value);
            }
            catch (ConsoleException e)
            {
                throw new ConsoleException(e.Message.Replace("double", "float"));
            }

            return result;
        }
        public static object DoubleParam(string value)
        {
            bool number = false;
            bool endNumber = false;
            bool refName = false;
            bool negate = false;
            bool inBrackets = false;
            int bracketCount = 0;

            Operation oper = Operation.Add;
            StringBuilder input = new StringBuilder(32);

            double result = 0d;

            void FromVar()
            {
                Variable var = FindVariable('$' + input.ToString(), out _);
                object obj = var.Getter();

                double val;

                try
                {
                    val = (double)obj;
                }
                catch (Exception)
                {
                    throw new ConsoleException("Variable not convertable to double type");
                }

                SetValue(val, oper, negate, ref result);
                refName = false;
                input.Clear();
            }
            void ChangeNorm()
            {
                if (!endNumber)
                {
                    if (refName)
                    {
                        FromVar();
                    }
                    else
                    {
                        SetValue(input.ToString(), oper, negate, ref result);
                        input.Clear();
                    }

                    negate = false;
                }

                endNumber = false;
                number = false;
            }
            void ChangeDiff()
            {
                if (number)
                {
                    SetValue(input.ToString(), oper, negate, ref result);
                    input.Clear();
                    negate = false;
                }
                else if (refName)
                {
                    FromVar();
                }

                endNumber = false;
                number = false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (inBrackets)
                {
                    if (c == '(')
                    {
                        bracketCount++;
                    }
                    if (c == ')')
                    {
                        bracketCount--;

                        if (bracketCount < 0)
                        {
                            inBrackets = false;
                            endNumber = true;

                            SetValue((double)DoubleParam(input.ToString()), oper, negate, ref result);
                            input.Clear();
                            oper = Operation.None;
                            negate = false;
                            continue;
                        }
                    }

                    input.Append(c);
                    continue;
                }

                switch (c)
                {
                    case '+':
                        ChangeDiff();

                        if (oper == Operation.None)
                        {
                            oper = Operation.Add;
                        }
                        continue;

                    case '-':
                        ChangeDiff();

                        if (oper == Operation.None)
                        {
                            oper = Operation.Subtract;
                            continue;
                        }
                        negate = !negate;
                        continue;

                    case '*':
                        ChangeNorm();
                        oper = Operation.Mutliply;
                        continue;

                    case '/':
                        ChangeNorm();
                        oper = Operation.Divide;
                        continue;

                    case '%':
                        ChangeNorm();
                        oper = Operation.Modulo;
                        continue;

                    case ' ':
                        if (number)
                        {
                            endNumber = true;
                            number = false;
                            SetValue(input.ToString(), oper, negate, ref result);
                            oper = Operation.None;
                            negate = false;
                            input.Clear();
                            continue;
                        }
                        if (refName)
                        {
                            endNumber = true;

                            FromVar();

                            oper = Operation.None;
                            negate = false;
                            continue;
                        }

                        continue;

                    default:
                        if (endNumber)
                        {
                            throw new ConsoleException("Invalid double syntax");
                        }
                        break;
                }

                if (refName)
                {
                    input.Append(c);
                    continue;
                }

                if (!number)
                {
                    if (char.IsNumber(c))
                    {
                        number = true;
                        input.Append(c);
                        continue;
                    }
                    if (c == '$')
                    {
                        refName = true;
                        number = false;
                        continue;
                    }
                    if (c == '(')
                    {
                        inBrackets = true;
                        continue;
                    }
                }
                else
                {
                    if (!char.IsNumber(c) && c != '.' && c != '(')
                    {
                        throw new ConsoleException("Invalid double paramter");
                    }
                }

                input.Append(c);
                continue;
            }

            if ((!endNumber && !number && !refName) || inBrackets)
            {
                throw new ConsoleException("Invalid double syntax");
            }

            if (number)
            {
                SetValue(input.ToString(), oper, negate, ref result);
            }
            if (refName)
            {
                FromVar();
            }

            return result;
        }

        private static void SetValue(string str, Operation o, bool neg, ref double d)
        {
            if (str.Length == 0)
            {
                throw new ConsoleException("Invalid double parameter");
            }

            if (!double.TryParse(str, out double value))
            {
                throw new ConsoleException("Invalid double parameter");
            }

            SetValue(value, o, neg, ref d);
        }
        private static void SetValue(double value, Operation o, bool neg, ref double d)
        {
            if (neg) { value = -value; }

            switch (o)
            {
                case Operation.Add:
                    d += value;
                    return;

                case Operation.Subtract:
                    d -= value;
                    return;

                case Operation.Mutliply:
                    d *= value;
                    return;

                case Operation.Divide:
                    d /= value;
                    return;

                case Operation.Modulo:
                    d %= value;
                    return;
            }
        }

        public static object StringParam(string value)
        {
            bool inQuotes = false;
            bool specialChar = false;
            bool endQuotes = false;
            bool refName = false;

            StringBuilder result = new StringBuilder(32);
            StringBuilder varName = new StringBuilder(32);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (refName)
                {
                    if (c == ' ')
                    {
                        refName = false;
                        Variable var = FindVariable('$' + varName.ToString(), out _);

                        result.Append(var.Getter().ToString());
                        varName.Clear();
                        continue;
                    }
                    if (c == '+')
                    {
                        refName = false;
                        endQuotes = false;
                        Variable var = FindVariable('$' + varName.ToString(), out _);

                        result.Append(var.Getter().ToString());
                        varName.Clear();
                        continue;
                    }

                    varName.Append(c);
                    continue;
                }

                if (!inQuotes)
                {
                    if (c == ' ') { continue; }

                    if (!endQuotes)
                    {
                        if (c == '\"')
                        {
                            inQuotes = true;
                            continue;
                        }

                        if (c == '$')
                        {
                            refName = true;
                            endQuotes = true;
                            continue;
                        }
                    }
                    else if (c == '\"'|| c == '$')
                    {
                        throw new ConsoleException("Invalid string syntax");
                    }

                    if (c == '+')
                    {
                        if (!endQuotes)
                        {
                            throw new ConsoleException("Invalid string syntax");
                        }

                        endQuotes = false;
                        continue;
                    }

                    throw new ConsoleException("Invalid string syntax");
                }
                if (!specialChar)
                {
                    if (c == '\\')
                    {
                        specialChar = true;
                        continue;
                    }
                    if (c == '\"')
                    {
                        inQuotes = false;
                        endQuotes = true;
                        continue;
                    }
                }
                else
                {
                    if (c == 'n')
                    {
                        result.Append('\n');
                        specialChar = false;
                        continue;
                    }
                    if (c == 'r')
                    {
                        result.Append('\r');
                        specialChar = false;
                        continue;
                    }

                    specialChar = false;
                }

                if (endQuotes)
                {
                    throw new ConsoleException("Invalid string syntax");
                }

                result.Append(c);
            }

            if (inQuotes || !endQuotes)
            {
                throw new ConsoleException("Invalid string syntax");
            }

            if (refName)
            {
                Variable var = FindVariable('$' + varName.ToString(), out _);

                result.Append(var.Getter().ToString());
            }

            return result.ToString();
        }

        public static object CharParam(string value)
        {
            bool inQuotes = false;
            bool specialChar = false;
            bool refName = false;
            bool index = false;

            char result = '\0';
            StringBuilder name = new StringBuilder(32);
            StringBuilder number = new StringBuilder(4);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (refName)
                {
                    if (c == ' ')
                    {
                        throw new ConsoleException("Invalid syntax");
                    }
                    if (c == '[')
                    {
                        index = true;
                        continue;
                    }

                    if (index)
                    {
                        if (c == ']')
                        {
                            index = false;
                            if (value.Length > i + 1)
                            {
                                throw new ConsoleException("Invalid character syntax");
                            }

                            break;
                        }

                        number.Append(c);
                        continue;
                    }

                    name.Append(c);
                    continue;
                }

                if (c == '$' && !inQuotes)
                {
                    refName = true;
                    continue;
                }

                if (c == '\\' && !specialChar)
                {
                    specialChar = true;
                    continue;
                }

                if (c == '\'' && !inQuotes)
                {
                    inQuotes = true;
                    continue;
                }
                else if (c == '\'' && !specialChar)
                {
                    if (result == '\0')
                    {
                        throw new ConsoleException("Invalid character syntax");
                    }

                    inQuotes = false;
                    continue;
                }

                if (specialChar)
                {
                    if (c == 'n')
                    {
                        result = '\n';
                        specialChar = false;
                        continue;
                    }
                    if (c == 'r')
                    {
                        result = '\r';
                        specialChar = false;
                        continue;
                    }

                    specialChar = false;
                }

                if (result != '\0')
                {
                    throw new ConsoleException("Invalid character syntax");
                }

                result = c;
            }

            if (index)
            {
                throw new ConsoleException("Invalid character syntax");
            }

            if (refName)
            {
                Variable var = FindVariable('$' + name.ToString(), out _);

                object get = var.Getter();

                if (get is char ch) { return ch; }
                else if (get is string str)
                {
                    int i = (int)IntParam(number.ToString());

                    if (i < 0 || i > (str.Length - 1))
                    {
                        throw new ConsoleException("Index out of range");
                    }

                    return str[i];
                }
            }

            return result;
        }
        public static object BoolParam(string value)
        {
            bool result = true;
            bool fromVar = false;

            int i = 0;

            if (value[i] == '!')
            {
                result = false;
                i++;
            }
            if (value[i] == '$')
            {
                fromVar = true;
                i++;
            }

            string info = value[i..].Trim();

            if (fromVar)
            {
                Variable var = FindVariable('$' + info, out _);
                object obj = var.Getter();

                if (obj is bool b)
                {
                    return result ? b : !b;
                }
                if (obj is int @int)
                {
                    return result ? @int > 0 : @int == 0;
                }

                throw new ConsoleException("Variable not convertable to boolean type");
            }
            else
            {
                info = info.ToLower();

                if (info == "true")
                {
                    return result;
                }
                if (info == "false")
                {
                    return !result;
                }

                throw new ConsoleException("Invalid boolean parameter");
            }
        }

        public static object Vector2Param(string value)
        {
            bool construct = false;
            bool part2 = false;
            bool endNumber = false;
            bool refName = false;
            bool negate = false;
            bool inBrackets = false;
            int bracketCount = 0;

            Operation oper = Operation.Add;
            StringBuilder input = new StringBuilder(32);

            Vector2 result = Vector2.Zero;

            void FromVar()
            {
                Variable var = FindVariable('$' + input.ToString(), out _);
                object obj = var.Getter();

                Vector2 val;

                try
                {
                    val = (Vector2)obj;
                }
                catch (Exception)
                {
                    throw new ConsoleException("Variable not convertable to Vector2 type");
                }

                SetValue(val, oper, negate, ref result);
                refName = false;
                input.Clear();
            }
            void ChangeNorm()
            {
                if (!endNumber)
                {
                    if (refName) { FromVar(); }

                    negate = false;
                }

                endNumber = false;
                construct = false;
            }
            void ChangeDiff()
            {
                if (refName) { FromVar(); }
                endNumber = false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (construct)
                {
                    if (c == ',' && !part2)
                    {
                        SetValue((double)DoubleParam(input.ToString()), true, oper, negate, ref result);
                        input.Clear();
                        part2 = true;
                        continue;
                    }
                    else if (c == ',')
                    {
                        throw new ConsoleException("Invalid Vector2 paramter");
                    }

                    if (c == ']')
                    {
                        if (!part2)
                        {
                            throw new ConsoleException("Invalid Vector2 paramter");
                        }

                        construct = false;
                        endNumber = true;

                        SetValue((double)DoubleParam(input.ToString()), false, oper, negate, ref result);
                        input.Clear();
                        oper = Operation.None;
                        negate = false;
                        continue;
                    }

                    if (c != ' ')
                    {
                        input.Append(c);
                        continue;
                    }

                    continue;
                }

                if (inBrackets)
                {
                    if (c == '(')
                    {
                        bracketCount++;
                    }
                    if (c == ')')
                    {
                        bracketCount--;

                        if (bracketCount < 0)
                        {
                            inBrackets = false;
                            endNumber = true;

                            SetValue((Vector2)Vector2Param(input.ToString()), oper, negate, ref result);
                            input.Clear();
                            oper = Operation.None;
                            negate = false;
                            continue;
                        }
                    }

                    input.Append(c);
                    continue;
                }

                switch (c)
                {
                    case '+':
                        ChangeDiff();

                        if (oper == Operation.None)
                        {
                            oper = Operation.Add;
                        }
                        continue;

                    case '-':
                        ChangeDiff();

                        if (oper == Operation.None)
                        {
                            oper = Operation.Subtract;
                            continue;
                        }
                        negate = !negate;
                        continue;

                    case '*':
                        ChangeNorm();
                        oper = Operation.Mutliply;
                        continue;

                    case '/':
                        ChangeNorm();
                        oper = Operation.Divide;
                        continue;

                    case ' ':
                        if (refName)
                        {
                            endNumber = true;

                            FromVar();

                            oper = Operation.None;
                            negate = false;
                            continue;
                        }

                        continue;

                    default:
                        if (endNumber)
                        {
                            throw new ConsoleException("Invalid Vector2 syntax");
                        }
                        break;
                }

                if (refName)
                {
                    input.Append(c);
                    continue;
                }

                if (!construct)
                {
                    if (c == '[')
                    {
                        construct = true;
                        part2 = false;
                        continue;
                    }
                    if (c == '$')
                    {
                        refName = true;
                        construct = false;
                        continue;
                    }
                    if (c == '(')
                    {
                        inBrackets = true;
                        continue;
                    }
                }

                input.Append(c);
                continue;
            }

            if ((!endNumber && !refName) || inBrackets || construct)
            {
                throw new ConsoleException("Invalid Vector2 syntax");
            }

            if (refName)
            {
                FromVar();
            }

            return result;
        }

        private static void SetValue(Vector2 value, Operation o, bool neg, ref Vector2 v)
        {
            if (neg) { value = -value; }

            switch (o)
            {
                case Operation.Add:
                    v += value;
                    return;

                case Operation.Subtract:
                    v -= value;
                    return;

                case Operation.Mutliply:
                    v *= value;
                    return;

                case Operation.Divide:
                    v /= value;
                    return;
            }
        }
        private static void SetValue(double value, bool side, Operation o, bool neg, ref Vector2 v)
        {
            if (neg) { value = -value; }

            if (side)
            {
                switch (o)
                {
                    case Operation.Add:
                        v.X += value;
                        return;

                    case Operation.Subtract:
                        v.X -= value;
                        return;

                    case Operation.Mutliply:
                        v.X *= value;
                        return;

                    case Operation.Divide:
                        v.X /= value;
                        return;
                }

                return;
            }

            switch (o)
            {
                case Operation.Add:
                    v.Y += value;
                    return;

                case Operation.Subtract:
                    v.Y -= value;
                    return;

                case Operation.Mutliply:
                    v.Y *= value;
                    return;

                case Operation.Divide:
                    v.Y /= value;
                    return;
            }
        }

        public static object Vector3Param(string value)
        {
            bool construct = false;
            int part2 = 0;
            bool endNumber = false;
            bool refName = false;
            bool negate = false;
            bool inBrackets = false;
            int bracketCount = 0;

            Operation oper = Operation.Add;
            StringBuilder input = new StringBuilder(32);

            Vector3 result = Vector3.Zero;

            void FromVar()
            {
                Variable var = FindVariable('$' + input.ToString(), out _);
                object obj = var.Getter();

                Vector3 val;

                try
                {
                    val = (Vector3)obj;
                }
                catch (Exception)
                {
                    throw new ConsoleException("Variable not convertable to Vector3 type");
                }

                SetValue(val, oper, negate, ref result);
                refName = false;
                input.Clear();
            }
            void ChangeNorm()
            {
                if (!endNumber)
                {
                    if (refName) { FromVar(); }

                    negate = false;
                }

                endNumber = false;
                construct = false;
            }
            void ChangeDiff()
            {
                if (refName) { FromVar(); }
                endNumber = false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (construct)
                {
                    if (c == ',' && part2 != 2)
                    {
                        SetValue((double)DoubleParam(input.ToString()), part2, oper, negate, ref result);
                        input.Clear();
                        part2++;
                        continue;
                    }
                    else if (c == ',')
                    {
                        throw new ConsoleException("Invalid Vector3 paramter");
                    }

                    if (c == ']')
                    {
                        if (part2 != 2)
                        {
                            throw new ConsoleException("Invalid Vector3 paramter");
                        }

                        construct = false;
                        endNumber = true;

                        SetValue((double)DoubleParam(input.ToString()), part2, oper, negate, ref result);
                        input.Clear();
                        oper = Operation.None;
                        negate = false;
                        continue;
                    }

                    if (c != ' ')
                    {
                        input.Append(c);
                        continue;
                    }

                    continue;
                }

                if (inBrackets)
                {
                    if (c == '(')
                    {
                        bracketCount++;
                    }
                    if (c == ')')
                    {
                        bracketCount--;

                        if (bracketCount < 0)
                        {
                            inBrackets = false;
                            endNumber = true;

                            SetValue((Vector3)Vector2Param(input.ToString()), oper, negate, ref result);
                            input.Clear();
                            oper = Operation.None;
                            negate = false;
                            continue;
                        }
                    }

                    input.Append(c);
                    continue;
                }

                switch (c)
                {
                    case '+':
                        ChangeDiff();

                        if (oper == Operation.None)
                        {
                            oper = Operation.Add;
                        }
                        continue;

                    case '-':
                        ChangeDiff();

                        if (oper == Operation.None)
                        {
                            oper = Operation.Subtract;
                            continue;
                        }
                        negate = !negate;
                        continue;

                    case '*':
                        ChangeNorm();
                        oper = Operation.Mutliply;
                        continue;

                    case '/':
                        ChangeNorm();
                        oper = Operation.Divide;
                        continue;

                    case ' ':
                        if (refName)
                        {
                            endNumber = true;

                            FromVar();

                            oper = Operation.None;
                            negate = false;
                            continue;
                        }

                        continue;

                    default:
                        if (endNumber)
                        {
                            throw new ConsoleException("Invalid Vector3 syntax");
                        }
                        break;
                }

                if (refName)
                {
                    input.Append(c);
                    continue;
                }

                if (!construct)
                {
                    if (c == '[')
                    {
                        construct = true;
                        part2 = 0;
                        continue;
                    }
                    if (c == '$')
                    {
                        refName = true;
                        construct = false;
                        continue;
                    }
                    if (c == '(')
                    {
                        inBrackets = true;
                        continue;
                    }
                }

                input.Append(c);
                continue;
            }

            if ((!endNumber && !refName) || inBrackets || construct)
            {
                throw new ConsoleException("Invalid Vector3 syntax");
            }

            if (refName)
            {
                FromVar();
            }

            return result;
        }

        private static void SetValue(Vector3 value, Operation o, bool neg, ref Vector3 v)
        {
            if (neg) { value = -value; }

            switch (o)
            {
                case Operation.Add:
                    v += value;
                    return;

                case Operation.Subtract:
                    v -= value;
                    return;

                case Operation.Mutliply:
                    v *= value;
                    return;

                case Operation.Divide:
                    v /= value;
                    return;
            }
        }
        private static void SetValue(double value, int side, Operation o, bool neg, ref Vector3 v)
        {
            if (neg) { value = -value; }

            if (side == 0)
            {
                switch (o)
                {
                    case Operation.Add:
                        v.X += value;
                        return;

                    case Operation.Subtract:
                        v.X -= value;
                        return;

                    case Operation.Mutliply:
                        v.X *= value;
                        return;

                    case Operation.Divide:
                        v.X /= value;
                        return;
                }

                return;
            }
            if (side == 1)
            {
                switch (o)
                {
                    case Operation.Add:
                        v.Y += value;
                        return;

                    case Operation.Subtract:
                        v.Y -= value;
                        return;

                    case Operation.Mutliply:
                        v.Y *= value;
                        return;

                    case Operation.Divide:
                        v.Y /= value;
                        return;
                }

                return;
            }

            switch (o)
            {
                case Operation.Add:
                    v.Z += value;
                    return;

                case Operation.Subtract:
                    v.Z -= value;
                    return;

                case Operation.Mutliply:
                    v.Z *= value;
                    return;

                case Operation.Divide:
                    v.Z /= value;
                    return;
            }
        }
    }
}
