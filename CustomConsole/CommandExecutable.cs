namespace CustomConsole
{
    public class CommandExecutable : Executable
    {
        public CommandExecutable(ISyntax src, KeyWord[] syntax, object[] properties, ExecuteHandle handle)
            : base(src, syntax, null, handle, VarType.Void)
        {
            Paramerters = properties ?? new object[0];
        }

        public object[] Paramerters { get; }

        public override object Execute()
        {
            return Function(Paramerters);
        }
    }
}
