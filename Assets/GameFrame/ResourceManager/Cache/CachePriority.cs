namespace GameFrame
{
    public enum CachePriority
    {
        /// <summary>
        /// 不缓存
        /// </summary>
        NoCache = 0,
        /// <summary>
        /// Short
        /// </summary>
        ShortTime = 1 << 0,
        /// <summary>
        /// Middle
        /// </summary>
        MiddleTime = 1 << 1,
        /// <summary>
        /// Long
        /// </summary>
        LongTime = 1 << 2,
        /// <summary>
        /// Persistent
        /// </summary>
        Persistent = 1 << 3,
    }
}