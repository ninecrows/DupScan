namespace C9Native
{
    /// <summary>
    /// Holds the boundaries of a screen rectangle.
    /// </summary>
    public class DisplayRectangle
    {
        /// <summary>
        /// Construct a new immutable rectangle object.
        /// </summary>
        /// <param name="aLeft">Left hand side.</param>
        /// <param name="aTop">Top position.</param>
        /// <param name="aRight">Right hand side.</param>
        /// <param name="aBottom">Bottom position.</param>
        public DisplayRectangle(int aLeft, int aTop, int aRight, int aBottom)
        {
            Left = aLeft;
            Top = aTop;
            Right = aRight;
            Bottom = aBottom;
        }

        /// <summary>
        /// Left hand side of this rectangle.
        /// </summary>
        public int Left { get; }

        /// <summary>
        /// Top of this rectangle.
        /// </summary>
        public int Top { get; }
    
        /// <summary>
        /// Right side of this rectangle.
        /// </summary>
        public int Right { get; }

        /// <summary>
        /// Bottom of this rectangle.
        /// </summary>
        public int Bottom { get; }
   }
}
