using System;

namespace CustomConsole
{
    public class BigException : Exception
    {
        public BigException()
            : base("Big problem, Huge problem, Massive problem")
        {

        }
    }
}
