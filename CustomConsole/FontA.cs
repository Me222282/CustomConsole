using Zene.Graphics;
using Zene.Structs;

namespace CustomConsole
{
    public class FontA : Font
    {
        private const double _widthToHeight = (1.0 / 7) * 8;

        public FontA()
            : base(1.0, _widthToHeight)
        {
            // Load font image
            byte[] byteData = Bitmap.ExtractData("resources/fontA.png", out int w, out int h);
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
        }

        //public override string Name => "FontA";

        private readonly Texture2D _texture;

        public override void BindTexture(uint slot) => _texture.Bind(slot);

        public override CharFontData GetCharacterData(char character)
        {
            character = char.ToUpper(character);

            Vector2 texSize = new Vector2(0.1, 0.25);
            Vector2 size = new Vector2(1, _widthToHeight);

            return character switch
            {
                'A' => new CharFontData(
                    new Vector2(0, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'B' => new CharFontData(
                    new Vector2(texSize.X, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'C' => new CharFontData(
                    new Vector2(texSize.X * 2, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'D' => new CharFontData(
                    new Vector2(texSize.X * 3, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'E' => new CharFontData(
                    new Vector2(texSize.X * 4, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'F' => new CharFontData(
                    new Vector2(texSize.X * 5, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'G' => new CharFontData(
                    new Vector2(texSize.X * 6, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'H' => new CharFontData(
                    new Vector2(texSize.X * 7, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'I' => new CharFontData(
                    new Vector2(texSize.X * 8, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'J' => new CharFontData(
                    new Vector2(texSize.X * 9, texSize.Y * 3),
                    texSize,
                    size,
                    Vector2.Zero),
                'K' => new CharFontData(
                    new Vector2(0, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'L' => new CharFontData(
                    new Vector2(texSize.X, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'M' => new CharFontData(
                    new Vector2(texSize.X * 2, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'N' => new CharFontData(
                    new Vector2(texSize.X * 3, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'O' => new CharFontData(
                    new Vector2(texSize.X * 4, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'P' => new CharFontData(
                    new Vector2(texSize.X * 5, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'Q' => new CharFontData(
                    new Vector2(texSize.X * 6, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'R' => new CharFontData(
                    new Vector2(texSize.X * 7, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'S' => new CharFontData(
                    new Vector2(texSize.X * 8, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'T' => new CharFontData(
                    new Vector2(texSize.X * 9, texSize.Y * 2),
                    texSize,
                    size,
                    Vector2.Zero),
                'U' => new CharFontData(
                    new Vector2(0, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                'V' => new CharFontData(
                    new Vector2(texSize.X, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                'W' => new CharFontData(
                    new Vector2(texSize.X * 2, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                'X' => new CharFontData(
                    new Vector2(texSize.X * 3, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                'Y' => new CharFontData(
                    new Vector2(texSize.X * 4, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                'Z' => new CharFontData(
                    new Vector2(texSize.X * 5, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                '0' => new CharFontData(
                    new Vector2(texSize.X * 6, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                '1' => new CharFontData(
                    new Vector2(texSize.X * 7, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                '2' => new CharFontData(
                    new Vector2(texSize.X * 8, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                '3' => new CharFontData(
                    new Vector2(texSize.X * 9, texSize.Y),
                    texSize,
                    size,
                    Vector2.Zero),
                '4' => new CharFontData(
                    new Vector2(0, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                '5' => new CharFontData(
                    new Vector2(texSize.X, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                '6' => new CharFontData(
                    new Vector2(texSize.X * 2, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                '7' => new CharFontData(
                    new Vector2(texSize.X * 3, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                '8' => new CharFontData(
                    new Vector2(texSize.X * 4, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                '9' => new CharFontData(
                    new Vector2(texSize.X * 5, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                '.' => new CharFontData(
                    new Vector2(texSize.X * 6, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                ':' => new CharFontData(
                    new Vector2(texSize.X * 7, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                '-' => new CharFontData(
                    new Vector2(texSize.X * 8, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                ',' => new CharFontData(
                    new Vector2(texSize.X * 9, 0),
                    texSize,
                    size,
                    Vector2.Zero),
                _ => CharFontData.Unsupported
            };
        }
    }
}
