using System;
using Zene.Graphics;
using Zene.Structs;

namespace CustomConsole
{
    public class FontMeme : Font
    {
        private const double _pixelHeight = 1.0 / 1150;
        private const double _pixelWidth = 1.0 / 1000;

        public FontMeme(string fontPath)
            : base(1.0, (1.0 / 76) * 87)
        {
            // Load font image
            byte[] byteData = Bitmap.ExtractData(fontPath, out int w, out int h);
            // Convert to one channel GLArray
            GLArray<Vector2<byte>> texData = new GLArray<Vector2<byte>>(w, h);
            for (int i = 0; i < texData.Size; i++)
            {
                //texData[i] = new Vector4<byte>(byteData[i * 4], 0, 0, 0);
                texData[i] = new Vector2<byte>(byteData[i * 4], 0);
            }
            // Create and setup texture
            _texture = new Texture2D(TextureFormat.R8, TextureData.Byte)
            {
                WrapStyle = WrapStyle.EdgeClamp,
                MinFilter = TextureSampling.Nearest,
                MagFilter = TextureSampling.Nearest
            };
            // Asign data
            _texture.SetData(w, h, BaseFormat.Rg, texData);

            Vector2 texSize = new Vector2(_pixelWidth * 76, _pixelHeight * 87);
            Vector2 size = new Vector2(1.0, (1.0 / 76) * 87);

            _characterData = new CharFontData[90]
            {
                // !
                new CharFontData(
                    new Vector2(_pixelWidth * 897, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),
                // "
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 46),
                    texSize,
                    size,
                    Vector2.Zero),
                // #
                new CharFontData(
                    new Vector2(_pixelWidth * 677, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),
                // $
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),
                // %
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 46),
                    texSize,
                    size,
                    Vector2.Zero),
                // &
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 46),
                    texSize,
                    size,
                    Vector2.Zero),
                // '
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),
                // (
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 46),
                    texSize,
                    size,
                    Vector2.Zero),
                // )
                new CharFontData(
                    new Vector2(_pixelWidth * 678, _pixelHeight * 46),
                    texSize,
                    size,
                    Vector2.Zero),

                CharFontData.Unsupported,
                CharFontData.Unsupported,

                // ,
                new CharFontData(
                    new Vector2(_pixelWidth * 238, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),

                CharFontData.Unsupported,

                // .
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),
                // /
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 46),
                    texSize,
                    size,
                    Vector2.Zero),
                // 0
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 1
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 2
                new CharFontData(
                    new Vector2(_pixelWidth * 237, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 3
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 4
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 5
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 6
                new CharFontData(
                    new Vector2(_pixelWidth * 677, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 7
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 8
                new CharFontData(
                    new Vector2(_pixelWidth * 897, _pixelHeight * 286),
                    texSize,
                    size,
                    Vector2.Zero),
                // 9
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),
                // :
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),
                // ;
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 166),
                    texSize,
                    size,
                    Vector2.Zero),

                CharFontData.Unsupported,
                CharFontData.Unsupported,
                CharFontData.Unsupported,

                // ?
                new CharFontData(
                    new Vector2(_pixelWidth * 237, _pixelHeight * 46),
                    texSize,
                    size,
                    Vector2.Zero),
                // @
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 46),
                    texSize,
                    size,
                    Vector2.Zero),
                // Alphabet Caps
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 237, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 677, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 897, _pixelHeight * 1054),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 237, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 677, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 897, _pixelHeight * 934),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 814),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 814),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 237, _pixelHeight * 814),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 814),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 814),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 814),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 677, _pixelHeight * 814),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 814),
                    texSize,
                    size,
                    Vector2.Zero),

                CharFontData.Unsupported,
                CharFontData.Unsupported,
                CharFontData.Unsupported,
                CharFontData.Unsupported,
                CharFontData.Unsupported,
                CharFontData.Unsupported,

                // Alphabet lower case
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 237, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 677, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 897, _pixelHeight * 670),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 237, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 677, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 897, _pixelHeight * 550),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 17, _pixelHeight * 430),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 127, _pixelHeight * 430),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 237, _pixelHeight * 430),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 347, _pixelHeight * 430),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 457, _pixelHeight * 430),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 567, _pixelHeight * 430),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 677, _pixelHeight * 430),
                    texSize,
                    size,
                    Vector2.Zero),
                new CharFontData(
                    new Vector2(_pixelWidth * 787, _pixelHeight * 430),
                    texSize,
                    size,
                    Vector2.Zero),
            };

            Name = $"FontMeme [{fontPath}]";
        }

        public override string Name { get; }

        private readonly Texture2D _texture;

        public override void BindTexture(uint slot) => _texture.Bind(slot);

        private readonly CharFontData[] _characterData;

        public override CharFontData GetCharacterData(char character)
        {
            try
            {
                return _characterData[character - 33];
            }
            catch (IndexOutOfRangeException)
            {
                return CharFontData.Unsupported;
            }
        }
    }
}
