using System;
using Zene.Windowing;
using Zene.Windowing.Base;
using Zene.Graphics;
using Zene.Structs;
using System.Text;

namespace CustomConsole
{
    class Program : Window
    {
        static void Main()
        {
            Core.Init();
            
            Program window = new Program(800, 500, VirtualConsole.Directory);

            window.Run();
            
            Core.Terminate();
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

            // Opacity
            State.Blending = true;
            Zene.Graphics.Base.GL.BlendFunc(Zene.Graphics.Base.GLEnum.SrcAlpha, Zene.Graphics.Base.GLEnum.OneMinusSrcAlpha);

            OnSizePixelChange(new SizeChangeEventArgs(width, height));
        }

        public override TextureRenderer Framebuffer { get; }
        private readonly TextRenderer _textRender;
        private readonly Font _fontC;

        private readonly StringBuilder _enterText = new StringBuilder(16);

        private double _margin = 5;
        private double _charSize = 15;

        private static char Caret
        {
            get
            {
                char caret = '\x0';

                // Flash caret
                if (((int)Math.Floor(GLFW.GetTime() * 2) % 2) == 0)
                {
                    caret = '_';
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
            VirtualConsole.OnLog += (_, _) => _update = true;

            VirtualConsole.AddFunction("Copy", new VariableType[] { VariableType.String, VariableType.Int }, (objs, info) =>
            {
                bool displayCount = false;

                if (info == "-i")
                {
                    displayCount = true;
                }
                else if (info != null && info.Length != 0)
                {
                    throw new ConsoleException("Invalid extra info");
                }

                string text = (string)objs[0];
                int count = (int)objs[1];

                for (int i = 0; i < count; i++)
                {
                    if (displayCount)
                    {
                        VirtualConsole.Log($"{text} - {i}");
                        continue;
                    }

                    VirtualConsole.Log(text);
                }
            });
            VirtualConsole.AddVariable("margin", VariableType.Double, () =>
            {
                return _margin;
            }, obj =>
            {
                _margin = (double)obj;
            });

            Vector3 v = Vector3.One;
            VirtualConsole.AddVariable("v", VariableType.Vector3, () =>
            {
                return v;
            }, obj =>
            {
                v = (Vector3)obj;
            });

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

                ReadOnlySpan<char> enterText = VirtualConsole.Name + "> " + _enterText.ToString();
                _textRender.DrawLeftBound(enterText, _fontC);

                SetCaretOffset(enterText);

                _textRender.DrawLeftBound($"{Caret}", _fontC);

                if (Title != VirtualConsole.Directory)
                {
                    Title = VirtualConsole.Directory;
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

            for (int i = 0; i < VirtualConsole.Output.Count; i++)
            {
                ReadOnlySpan<char> text = VirtualConsole.Output[i];

                double location = corner.Y + (_viewOffset + lineOffset - _fontC.LineHeight);
                // Above view
                if (location > corner.Y) { goto NextChar; }
                // Below view
                if (location < -corner.Y) { goto NextChar; }

                _textRender.Model = scaleM *
                    Matrix4.CreateTranslation(corner.X, corner.Y + lineOffset, 0d);

                _textRender.DrawLeftBound(text, _fontC);

            NextChar:
                double lineHeight = (_fontC.GetLineHeight(text) + _fontC.LineSpace) * -_charSize;
                lineOffset += lineHeight;
            }

            _textRender.Model = scaleM *
                Matrix4.CreateTranslation(corner.X, corner.Y + lineOffset, 0d);
        }

        private unsafe void SetCaretOffset(ReadOnlySpan<char> enterText)
        {
            ReadOnlySpan<char> offsetCount;

            fixed (char* c = &enterText[0])
            {
                offsetCount = new ReadOnlySpan<char>(c, VirtualConsole.Name.Length + 2 + _textIndex);
            }

            double charSpace = _fontC.GetCharacterData(offsetCount[^1]).Buffer + _fontC.CharSpace;
            double caretOffset = (_fontC.GetLineWidth(offsetCount, _fontC.CharSpace, 4) + charSpace) * _charSize;
            _textRender.Model *= Matrix4.CreateTranslation(caretOffset, 0d, 0d);
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
                _textIndex--;

                if (_textIndex < 0)
                {
                    _textIndex = 0;
                }
            }
            if (e[Keys.Right])
            {
                _textIndex++;

                if (_textIndex > _enterText.Length)
                {
                    _textIndex = _enterText.Length;
                }
            }

            if (e[Keys.BackSpace] && _enterText.Length > 0)
            {
                _textIndex--;
                if (_textIndex < 0)
                {
                    _textIndex = 0;
                }
                // Remove character at text index
                _enterText.Remove(_textIndex, 1);
                return;
            }
            if (e[Keys.Enter] || e[Keys.NumPadEnter])
            {
                string command = _enterText.ToString();

                VirtualConsole.Log(VirtualConsole.Name + "> " + command);
                VirtualConsole.ExecuteCommand(command);

                if (VirtualConsole.Output.Count > 0)
                {
                    VirtualConsole.NewLine();
                }

                _enterText.Clear();
                _textIndex = 0;
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

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            // Invalid character
            if (!e[' '] && !_fontC.GetCharacterData(e.Character).Supported) { return; }

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
                double offset = ((absDelta * _fontC.LineHeight) + (_fontC.LineSpace * absDelta)) * _charSize;

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
    }
}