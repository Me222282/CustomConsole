namespace CustomConsole
{
    public delegate void CommandHandler(object[] @params);

    public sealed class Command
    {
        public Command(string name, CommandProperty[] properties, CommandHandler handle)
        {
            Name = name;
            Properties = properties ?? new CommandProperty[0];
            Handle = handle;
        }

        public string Name { get; }
        public CommandProperty[] Properties { get; }

        public CommandHandler Handle { get; }
    }
}
