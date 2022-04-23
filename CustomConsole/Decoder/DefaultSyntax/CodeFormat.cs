namespace CustomConsole
{
    public struct CodeFormat : ICodeFormat
    {
        public CodeFormat(string[] pres, string[] posts)
        {
            NoPreSpaces = pres ?? new string[0];
            NoPostSpaces = posts ?? new string[0];
        }
        public CodeFormat(params string[] chars)
        {
            NoPreSpaces = chars ?? new string[0];
            NoPostSpaces = chars ?? new string[0];
        }

        public string[] NoPreSpaces { get; }
        public string[] NoPostSpaces { get; }
    }
}
