namespace CustomConsole
{
    public delegate object ExecuteHandle(object[] @params);

    public class Executable
    {
        public Executable(KeyWord[] syntax, Executable[] subExecutables, ExecuteHandle handle)
        {
            Syntax = syntax;
            SubExecutables = subExecutables;
            Function = handle;
        }

        public KeyWord[] Syntax { get; }
        public Executable[] SubExecutables { get; }

        public ExecuteHandle Function { get; }

        public object Execute()
        {
            object[] @params = new object[SubExecutables.Length];

            for (int i = 0; i < @params.Length; i++)
            {
                @params[i] = SubExecutables[i].Execute();
            }

            return Function(@params);
        }
    }
}
