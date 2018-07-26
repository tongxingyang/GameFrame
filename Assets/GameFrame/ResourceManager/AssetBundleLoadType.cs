namespace GameFrame
{
    public enum AssetBundleLoadType
    {
        /// <summary>
        /// 利用AssetBundle.LoadFromFile加载
        /// </summary>
        LoadBundleFromFile= 0,
        /// <summary>
        /// 利用AssetBundle.LoadFromFile异步加载
        /// </summary>
        LoadBundleFromFileAsync = 1 << 0,
        /// <summary>
        /// 利用LoadFromMemory加载
        /// </summary>
        LoadFromMemory = 1 << 1,
        /// <summary>
        /// 利用LoadFromMemoryAsync异步加载
        /// </summary>
        LoadFromMemoryAsync = 1 << 2,
        /// <summary>
        /// 利用LoadFromStream加载
        /// </summary>
        LoadFromStream = 1 << 3,
        /// <summary>
        /// 利用LoadFromStreamAsync异步加载
        /// </summary>
        LoadFromStreamAsync = 1 << 4 ,
    }
}