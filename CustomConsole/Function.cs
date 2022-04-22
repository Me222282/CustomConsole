using System;

namespace CustomConsole
{
    public class Function
    {
        public Function(string name, IVarType[] @params, IVarType returnType, ExecuteHandle handle)
        {
            Path = new string[] { name };
            Parameters = @params ?? Array.Empty<IVarType>();
            ReturnType = returnType;
            Handle = handle;
        }
        public Function(string[] names, IVarType[] @params, IVarType returnType, ExecuteHandle handle)
        {
            Path = names;
            Parameters = @params ?? Array.Empty<IVarType>();
            ReturnType = returnType;
            Handle = handle;
        }

        public string[] Path { get; }
        public IVarType[] Parameters { get; }
        public IVarType ReturnType { get; }

        public ExecuteHandle Handle { get; }
    }
}
