namespace FractalDataWorks.Entities
{
    /// <summary>
    /// Unit type representing void/no return value in Result patterns.
    /// Used for operations that don't return a meaningful value but can succeed or fail.
    /// </summary>
    public readonly record struct Unit
    {
        /// <summary>
        /// The singleton Unit value
        /// </summary>
        public static readonly Unit Value = new();
        
        /// <summary>
        /// String representation
        /// </summary>
        /// <returns>Empty string</returns>
        public override string ToString() => string.Empty;
    }
}