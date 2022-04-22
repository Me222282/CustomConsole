using System;

namespace CustomConsole
{
    public ref struct LastFind
    {
        public LastFind(ReadOnlySpan<KeyWord> lastSyntax, ISyntax source)
        {
            LastSyntax = lastSyntax;
            Source = source;
        }

        public ReadOnlySpan<KeyWord> LastSyntax { get; }
        public ISyntax Source { get; }
    }
}
