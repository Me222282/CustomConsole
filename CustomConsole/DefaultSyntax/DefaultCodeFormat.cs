namespace CustomConsole
{
    public struct DefaultCodeFormat : ICodeFormat
    {
        private static readonly string[] _preChars = new string[] { ")", "]", "}" };
        private static readonly string[] _postChars = new string[] { "(", "[", "{" };

        public string[] NoPreSpaces => _preChars;
        public string[] NoPostSpaces => _postChars;
    }
}
