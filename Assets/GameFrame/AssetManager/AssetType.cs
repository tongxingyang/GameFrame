namespace GameFrame.AssetManager
{
    public enum AssetBundleExportType
    {
        Asset = 1,
        Root = 1 << 1,
        Standalone = 1 << 2,
    }
    public enum AssetType
    {
        Asset,
        Builtin
    }
}