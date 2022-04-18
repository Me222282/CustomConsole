using System;

namespace CustomConsole
{
    public class ConsoleException : Exception
    {
        public ConsoleException(string message)
            : base(message)
        {

        }
    }
}
