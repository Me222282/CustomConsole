namespace CustomConsole
{
    public sealed class CommandExecutable : Executable
    {
        public CommandExecutable(ISyntax src, KeyWord[] syntax, Executable[] properties, CommandHandler handle)
            : base(src, syntax, properties, objs => { handle(objs); return null; }, VarType.Void)
        {
            
        }
    }

    public sealed class BooleanExecutable : Executable
    {
        public BooleanExecutable(ISyntax src, KeyWord[] syntax, bool value)
            : base(src, syntax, null, _ => value, VarType.Bool)
        {

        }
    }
}
