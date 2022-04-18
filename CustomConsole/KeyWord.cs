namespace CustomConsole
{
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

        public KeyWord(string key, int info = 0)
        {
            Word = key;
            Info = info;
        }
        public KeyWord(VariableType expectedType)
        {
            Word = "";
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
    }
}
