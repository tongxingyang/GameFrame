namespace GameFrame
{
    public enum CachePriority
    {
        None,
        /// <summary>
        /// 短时缓存
        /// </summary>
        ShortTime,
        /// <summary>
        /// 一般
        /// </summary>
        MiddleTime,
        /// <summary>
        /// 长时缓存
        /// </summary>
        LongTime,
        /// <summary>
        /// 常驻内存
        /// </summary>
        Persistent,
    }
}