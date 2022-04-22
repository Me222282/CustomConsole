namespace CustomConsole
{
    public interface IVarType
    {
        public IVarType[] ImplicitTo { get; }

        public bool Equals(IVarType type);
        /// <summary>
        /// This <see cref="IVarType"/> can be implicitly casted to <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Compatible(IVarType type);
    }
}
