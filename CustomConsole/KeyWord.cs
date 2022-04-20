using System;

namespace CustomConsole
{
    public enum KeyWordType
    {
        BracketOpen = 0b_01,
        BracketClosed = 0b_10,
        Bracket = BracketOpen | BracketClosed,
        Word,
        Number,
        String,
        Char,
        Special,
        Input
    }

    public struct KeyWord
    {
        public const int Double = 1;
        public const int Int = 2;
        public const int Float = 3;
        public const int String = 4;
        public const int Char = 5;
        public const int Bool = 6;
        public const int Vector2 = 7;
        public const int Vecotr3 = 8;

        public KeyWord(string key, KeyWordType type, int info = 0)
        {
            Word = key;
            Type = type;
            Info = info;
        }
        public KeyWord(VariableType expectedType)
        {
            Word = "";
            Type = KeyWordType.Input;

            Info = expectedType switch
            {
                VariableType.Double => Double,
                VariableType.Int => Int,
                VariableType.Float => Float,
                VariableType.String => String,
                VariableType.Char => Char,
                VariableType.Bool => Bool,
                VariableType.Vector2 => Vector2,
                VariableType.Vector3 => Vecotr3,
                _ => (int)expectedType
            };
        }

        public string Word { get; }
        public int Info { get; }

        public KeyWordType Type { get; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Word, Info, Type);
        }
        public override bool Equals(object obj)
        {
            return obj is KeyWord k &&
                Word == k.Word &&
                Type == k.Type;
        }

        public static bool operator ==(KeyWord l, KeyWord r)
        {
            return l.Equals(r);
        }
        public static bool operator !=(KeyWord l, KeyWord r)
        {
            return !l.Equals(r);
        }

        public static KeyWord UnknownInput { get; } = new KeyWord("", KeyWordType.Input);
    }
}
