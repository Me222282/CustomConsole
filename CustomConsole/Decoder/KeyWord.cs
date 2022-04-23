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
        public KeyWord(string key, KeyWordType type, int info = 0)
        {
            Word = key;
            Type = type;
            Info = info;
            InputType = null;
        }
        public KeyWord(IVarType expectedType)
        {
            Word = "";
            Type = KeyWordType.Input;
            Info = 0;
            InputType = expectedType;
        }

        public string Word { get; }
        public int Info { get; }
        public IVarType InputType { get; }

        public KeyWordType Type { get; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Word, Info, Type);
        }
        public override bool Equals(object obj)
        {
            if (obj is KeyWord k)
            {
                if (k.Word == "" || Word == "")
                {
                    return Type == k.Type;
                }

                return Word == k.Word &&
                    Type == k.Type;
            }

            return false;
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
