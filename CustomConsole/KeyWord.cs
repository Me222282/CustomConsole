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
        public const int Double = (int)VariableType.Double;
        public const int Int = (int)VariableType.Int;
        public const int Float = (int)VariableType.Float;
        public const int String = (int)VariableType.String;
        public const int Char = (int)VariableType.Char;
        public const int Bool = (int)VariableType.Bool;
        public const int Vector2 = (int)VariableType.Vector2;
        public const int Vecotr3 = (int)VariableType.Vector3;

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
            Info = (int)expectedType;
        }

        public string Word { get; }
        public int Info { get; }
        public VariableType InputType => (VariableType)Info;

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
