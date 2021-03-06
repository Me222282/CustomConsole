using System;
using Zene.Windowing;
using Zene.Windowing.Base;
using Zene.Graphics;
using Zene.Structs;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace CustomConsole
{
    class Program : Window
    {
        static void Main()
        {/*
            Core.Init();
            
            Program window = new Program(800, 500, Terminal.Directory);

            window.Run();
            
            Core.Terminate();
            return;*/
            SyntaxPasser.Variables.Add(new Variable("bean", VarType.Int, 5));
            SyntaxPasser.Functions.Add(new Function(new string[] { "Maths", "Round" }, new IVarType[] { VarType.Double }, VarType.Double, objs =>
            {
                return Math.Round((double)objs[0]);
            }));

            SyntaxPasser.Syntaxes.Add(new Syntax(new KeyWord[]
            {
                new KeyWord(VarType.Variable),
                new KeyWord(".", KeyWordType.Special),
                new KeyWord("Type", KeyWordType.Word),
            }, VarType.Type, objs =>
            {
                return ((Variable)objs[0]).Type;
            }));

            SyntaxPasser.Functions.Add(new Function(new string[] { "Console", "WriteLine" }, new IVarType[] { VarType.NonVoid }, VarType.Void, objs =>
            {
                Console.WriteLine(objs[0]);
                return null;
            }));
            SyntaxPasser.Functions.Add(new Function(new string[] { "Console", "ReadLine" }, null, VarType.String, objs =>
            {
                return Console.ReadLine();
            }));

            SyntaxPasser.Variables.Add(new Variable("null", VarType.NonVoid, () => null, _ => throw new Exception("Cannot set null")));

            //SyntaxPasser.Syntaxes.Insert(0, new CommandSyntax());
            CommandSyntax.Commands.Add(new Command("test", new CommandProperty[]
            {
                new CommandProperty("t"),
                new CommandProperty("c"),
                new CommandProperty("str", VarType.String)
            }, objs =>
            {
                Console.WriteLine($"{objs[2]} works with {objs[0]} and {objs[1]}");
            }));

            ExecuteFile("resources/zene.txt");
            Console.ReadLine();
            return;

            KeyWord[] kws = "Maths.Round(5.3f) + 5.2d + bean".FindKeyWords();

            Executable e;
            try
            {
                e = PassLine(kws);
            }
            catch (ConsoleException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return;
            }

            try
            {
                Console.WriteLine(e.Execute());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name} was thrown with message \"{ex.Message}\"");
                Console.ReadLine();
                return;
            }
            Console.WriteLine(e);
            Console.ReadLine();
        }
        
        private static Executable PassLine(ReadOnlySpan<KeyWord> line)
        {
            SyntaxPasser sp = new SyntaxPasser(default);
            return sp.Decode(line);
        }
        private static void ExecuteFile(string path)
        {
            string sourceCode = File.ReadAllText(path);

            ScriptPasser sp = new ScriptPasser();

            sp.Decode(sourceCode);

            sp.Execute();
        }

        public Program(int width, int height, string title)
         : base(width, height, title)
        {
            Framebuffer = new TextureRenderer(width, height);
            Framebuffer.SetColourAttachment(0, TextureFormat.Rgba8);

            _textRender = new TextRenderer(40)
            {
                AutoIncreaseCapacity = true
            };

            _fontC = new FontC();
            _fontC2 = new FontC2();
            _usingFont = _fontC;

            // Opacity
            State.Blending = true;
            Zene.Graphics.Base.GL.BlendFunc(Zene.Graphics.Base.GLEnum.SrcAlpha, Zene.Graphics.Base.GLEnum.OneMinusSrcAlpha);

            OnSizePixelChange(new SizeChangeEventArgs(width, height));
        }

        public override TextureRenderer Framebuffer { get; }
        private readonly TextRenderer _textRender;
        private Font _usingFont;
        private readonly Font _fontC;
        private readonly Font _fontC2;

        private readonly StringBuilder _enterText = new StringBuilder(16);

        private double _margin = 5;
        private double _charSize = 15;

        private static char _caretChar = '|';
        private static char Caret
        {
            get
            {
                char caret = '\x0';

                // Flash caret
                if (((int)Math.Floor(GLFW.GetTime() * 2) % 2) == 0)
                {
                    caret = _caretChar;
                }

                return caret;
            }
        }
        private int _textIndex = 0;

        private double _viewOffset = 0;

        private bool _update = true;

        public void Run()
        {
            // Vsync
            GLFW.SwapInterval(1);

            // Update draw when output to console
            Terminal.OnLog += (_, _) => _update = true;
            // Reset
            Terminal.OnReset += (_, _) => Reset();

            AddVariables();

            Matrix4 originOffset = Matrix4.Identity;

            while (GLFW.WindowShouldClose(Handle) == GLFW.False)
            {
                Framebuffer.Bind();

                if (_update)
                {
                    Draw();
                    // Last set to right position for enter text
                    originOffset = _textRender.Model;
                    _update = false;
                }

                base.Framebuffer.Clear(BufferBit.Colour);
                Framebuffer.CopyFrameBuffer(base.Framebuffer, BufferBit.Colour, TextureSampling.Nearest);

                base.Framebuffer.Bind();

                _textRender.Model = originOffset;

                ReadOnlySpan<char> enterText = Terminal.Name + "> " + _enterText.ToString();
                _textRender.DrawLeftBound(enterText, _usingFont);

                SetCaretOffset(enterText);

                _textRender.DrawLeftBound(Caret.ToString(), _usingFont);

                if (Title != Terminal.Directory)
                {
                    Title = Terminal.Directory;
                }

                GLFW.SwapBuffers(Handle);
                GLFW.PollEvents();
            }
        }
        private void Draw()
        {
            Framebuffer.Clear(BufferBit.Colour);

            _textRender.View = Matrix4.CreateTranslation(0d, _viewOffset, 0d);

            Vector2 corner = ((Width * -0.5) + _margin, (Height * 0.5) - _margin);
            Matrix4 scaleM = Matrix4.CreateScale(_charSize);

            double lineOffset = 0;

            for (int i = 0; i < Terminal.Output.Count; i++)
            {
                ReadOnlySpan<char> text = Terminal.Output[i];

                double location = corner.Y + (_viewOffset + lineOffset - _usingFont.LineHeight);
                // Above view
                if (location > corner.Y) { goto NextChar; }
                // Below view
                if (location < -corner.Y) { goto NextChar; }

                _textRender.Model = scaleM *
                    Matrix4.CreateTranslation(corner.X, corner.Y + lineOffset, 0d);

                _textRender.DrawLeftBound(text, _usingFont);

            NextChar:
                double lineHeight = (_usingFont.GetLineHeight(text) + _usingFont.LineSpace) * -_charSize;
                lineOffset += lineHeight;
            }

            _textRender.Model = scaleM *
                Matrix4.CreateTranslation(corner.X, corner.Y + lineOffset, 0d);
        }

        private unsafe void SetCaretOffset(ReadOnlySpan<char> enterText)
        {
            // A span of all the characters prefixing the caret
            ReadOnlySpan<char> offsetCount;
            fixed (char* c = &enterText[0])
            {
                offsetCount = new ReadOnlySpan<char>(c, Terminal.Name.Length + 2 + _textIndex);
            }

            // The width of the caret
            double caretSize = _usingFont.GetCharacterData(_caretChar).Size.X;

            CharFontData cfd = _usingFont.GetCharacterData(offsetCount[^1]);
            // Gets the space between the edit character and the next character - in character space
            double charSpace = cfd.Buffer + cfd.ExtraOffset.X + _usingFont.CharSpace;

            // Gets the offset for the caret in character space - centers the caret bewteen the two neighbouring characters
            double lineSpace = _usingFont.GetLineWidth(offsetCount, _usingFont.CharSpace, 4) + (charSpace * 0.5) - (caretSize * 0.5);
            // Creates a matrix to offset by "lineSpace" space but converted to pixel space
            _textRender.Model *= Matrix4.CreateTranslation(lineSpace * _charSize, 0d, 0d);
        }

        protected override void OnSizePixelChange(SizeChangeEventArgs e)
        {
            base.OnSizePixelChange(e);

            // Minimised
            if (e.Width < 1 || e.Height < 1) { return; }

            _update = true;

            Framebuffer.ViewSize = e.Size;
            base.Framebuffer.ViewSize = e.Size;
            Framebuffer.Size = e.Size;

            _textRender.Projection = Matrix4.CreateOrthographic(e.Width, e.Height, 0d, 1d);
        }

        private bool _ctrl = false;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e[Keys.LeftControl] || e[Keys.RightControl])
            {
                _ctrl = true;
                return;
            }

            if (e[Keys.Left])
            {
                // Reset caret timer
                GLFW.SetTime(0);

                if (_ctrl)
                {
                    _textIndex = FindNextSigLeft(_textIndex);
                    return;
                }

                _textIndex--;

                if (_textIndex < 0)
                {
                    _textIndex = 0;
                }
            }
            if (e[Keys.Right])
            {
                // Reset caret timer
                GLFW.SetTime(0);

                if (_ctrl)
                {
                    _textIndex = FindNextSigRight(_textIndex);
                    return;
                }

                _textIndex++;

                if (_textIndex > _enterText.Length)
                {
                    _textIndex = _enterText.Length;
                }
            }

            if (e[Keys.BackSpace] && _enterText.Length > 0)
            {
                // Reset caret timer
                GLFW.SetTime(0);

                if (_ctrl)
                {
                    int old = _textIndex;
                    _textIndex = FindNextSigLeft(_textIndex);

                    _enterText.Remove(_textIndex, old - _textIndex);
                    return;
                }

                _textIndex--;

                if (_textIndex < 0)
                {
                    _textIndex = 0;
                    return;
                }
                // Remove character before text index
                _enterText.Remove(_textIndex, 1);
                return;
            }
            if (e[Keys.Delete] && _enterText.Length > _textIndex)
            {
                // Reset caret timer
                GLFW.SetTime(0);

                if (_ctrl)
                {
                    int @new = FindNextSigRight(_textIndex);

                    _enterText.Remove(_textIndex, @new - _textIndex);
                    return;
                }

                // Remove character after text index
                _enterText.Remove(_textIndex, 1);
                return;
            }
            if (e[Keys.Enter] || e[Keys.NumPadEnter])
            {
                string command = _enterText.ToString();

                Terminal.Log(Terminal.Name + "> " + command);
                Terminal.ExecuteCommand(command);

                if (Terminal.Output.Count > 0)
                {
                    Terminal.NewLine();
                }
                
                _enterText.Clear();
                _textIndex = 0;
                return;
            }

            if (e[Keys.V] && _ctrl)
            {
                IntPtr ptr = GLFW.GetClipboardString(Handle);

                string str = Marshal.PtrToStringUTF8(ptr);

                // No string
                if (str.Length == 0) { return; }

                _enterText.Insert(_textIndex, str);
                _textIndex += str.Length;
                return;
            }
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e[Keys.LeftControl] || e[Keys.RightControl])
            {
                _ctrl = false;
                return;
            }
        }

        private int FindNextSigRight(int index)
        {
            // Not long enough
            if (_enterText.Length <= index) { return _enterText.Length; }
            
            char current = _enterText[index];

            bool isNumber = current == '.' || char.IsNumber(current);
            bool isText = current == '_' || char.IsLetter(current);
            
            bool afterWhiteSpace = !isNumber && !isText;

            for (index++; index < _enterText.Length; index++)
            {
                char c = _enterText[index];
                
                if (afterWhiteSpace && !char.IsWhiteSpace(c))
                {
                    return index;
                }

                if (isNumber && c != '_' && c != '.' && !char.IsNumber(c))
                {
                    afterWhiteSpace = true;
                    isNumber = false;
                    index--;
                    continue;
                }

                if (isText && c != '_' && !char.IsNumber(c) && !char.IsLetter(c))
                {
                    afterWhiteSpace = true;
                    isText = false;
                    index--;
                    continue;
                }
            }

            return _enterText.Length;
        }
        private int FindNextSigLeft(int index)
        {
            index--;

            // Not long enough
            if (index <= 0) { return 0; }

            char current;
            if (_enterText.Length <= index)
            {
                current = _enterText[^1];
            }
            else
            {
                current = _enterText[index];
            }

            bool isNumber = current == '.' || char.IsNumber(current);
            bool isText = current == '_' || char.IsLetter(current);

            bool afterWhiteSpace = !isNumber && !isText;

            for (index--; index >= 0; index--)
            {
                char c = _enterText[index];

                if (afterWhiteSpace && !char.IsWhiteSpace(c))
                {
                    return index + 1;
                }

                if (isNumber && c != '_' && c != '.' && !char.IsNumber(c))
                {
                    afterWhiteSpace = true;
                    isNumber = false;
                    index++;
                    continue;
                }

                if (isText && c != '_' && !char.IsNumber(c) && !char.IsLetter(c))
                {
                    afterWhiteSpace = true;
                    isText = false;
                    index++;
                    continue;
                }
            }

            return 0;
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            // Invalid character
            if (!e[' '] && !_usingFont.GetCharacterData(e.Character).Supported) { return; }

            // Reset caret timer
            GLFW.SetTime(0);

            //_enterText.Append(e.Character);
            _enterText.Insert(_textIndex, e.Character);
            _textIndex++;
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);

            _update = true;

            if (!_ctrl)
            {
                double absDelta = Math.Abs(e.DeltaY * 3);
                double offset = ((absDelta * _usingFont.LineHeight) + (_usingFont.LineSpace * absDelta)) * _charSize;

                if (e.DeltaY < 0) { offset = -offset; }
                _viewOffset -= offset;

                if (_viewOffset < 0)
                {
                    _viewOffset = 0;
                }
                return;
            }

            _charSize += e.DeltaY;

            if (_charSize < 2) { _charSize = 2; }
            else if (_charSize > 100) { _charSize = 100; }
        }

        private bool _monoSpace = true;
        private void AddVariables()
        {
            Terminal.AddVariable("margin", VarType.Double, () =>
            {
                return _margin;
            }, obj =>
            {
                _margin = (double)obj;
            });

            Terminal.AddVariable("monospace", VarType.Bool, () =>
            {
                return _monoSpace;
            }, obj =>
            {
                _monoSpace = (bool)obj;

                if (_monoSpace)
                {
                    _usingFont = _fontC;
                    return;
                }

                _usingFont = _fontC2;
            });

            Terminal.AddVariable("charspace", VarType.Double, () =>
            {
                return _usingFont.CharSpace;
            }, obj =>
            {
                _fontC.CharSpace = (double)obj;
                _fontC2.CharSpace = (double)obj;
            });
            Terminal.AddVariable("linespace", VarType.Double, () =>
            {
                return _usingFont.LineSpace;
            }, obj =>
            {
                _fontC.LineSpace = (double)obj;
                _fontC2.LineSpace = (double)obj;
            });

            Terminal.AddVariable("caret", VarType.Char, () =>
            {
                return _caretChar;
            }, obj =>
            {
                _caretChar = (char)obj;
            });
        }

        private void Reset()
        {
            _update = true;
            _viewOffset = 0;
            _margin = 5;
            _charSize = 15;
            _caretChar = '|';
            _textIndex = 0;
            _usingFont = _fontC;

            _fontC.CharSpace = 0.2;
            _fontC2.CharSpace = 0.2;
            _fontC.LineSpace = 0.25;
            _fontC2.LineSpace = 0.25;

            AddVariables();
        }
    }
}