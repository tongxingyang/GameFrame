namespace GameFrame.Update
{
    public enum enClientState
    {
       
        State_Init,//初始化
        State_InitSDK,//初始化sdk
        State_UnZipData,//解压资源
        State_UpdateApp,//更新应用
        State_UpdateResource,//更新资源
        State_Start,//更新完成
        
    }
}