namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Provides enumerated values for the kinds of operations that are to be allowed.
    /// </summary>
    public enum EffectAllowed
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Copy.
        /// </summary>
        Copy = 1,

        /// <summary>
        /// CopyLink.
        /// </summary>
        CopyLink = 2,

        /// <summary>
        /// CopyMove.
        /// </summary>
        CopyMove = 3,

        /// <summary>
        /// Link.
        /// </summary>
        Link = 4,

        /// <summary>
        /// LinkMove.
        /// </summary>
        LinkMove = 5,

        /// <summary>
        /// Move.
        /// </summary>
        Move = 6,

        /// <summary>
        /// All.
        /// </summary>
        All = 7,

        /// <summary>
        /// Uninitialized.
        /// </summary>
        Uninitialized = 8,
    }
}
