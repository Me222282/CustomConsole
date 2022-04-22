namespace CustomConsole
{
    public delegate void CommandHandler(object[] @params);

    public class Command
    {
        public Command(string name, char[] properties, CommandHandler handle)
        {
            Name = name;
            Properties = properties ?? new char[0];
            Handle = handle;
        }

        public string Name { get; }
        public char[] Properties { get; }

        public CommandHandler Handle { get; }
    }
}
