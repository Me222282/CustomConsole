namespace CustomConsole
{
    public delegate void CommandHandler(object[] @params);

    public class Command
    {
        public Command(string name, char[] properties, CommandHandler handle)
        {
            Name = name;
            Properties = properties;
            Handle = handle;
        }

        public string Name { get; }
        public char[] Properties { get; }

        public CommandHandler Handle { get; }
    }
}
