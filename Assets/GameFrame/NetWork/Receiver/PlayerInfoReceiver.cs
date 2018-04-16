using Common;
using Common.OperationCode;
using ExitGames.Client.Photon;
using GameFrame;
using UIFrameWork;
using UnityEngine;

public  class PlayerInfoReceiver:ReceiverInterface
{

    public void Receiver(byte subCode, OperationResponse response)
    {
        switch (subCode)
        {
            case (byte)PlayerInfoOpCode.GetInfo:
                GetInfo(response);
                break;
            case (byte)PlayerInfoOpCode.Create:
                Create(response);
                break;
        }
    }

    private void GetInfo(OperationResponse response)
    {
        if (response.ReturnCode == 0)
        {
            //ok
            GameData.PlayerInfoModel = new PlayerInfoModel();
            PlayerInfoModel playerInfo = CommonTool.Deserialize<PlayerInfoModel>((byte[]) response.Parameters[1]);
            GameData.PlayerInfoModel = playerInfo;
            Debug.Log("获取用户信息成功");
        }
        else
        {
            //需要创建
            Debug.Log("需要创建角色信息");
        }
        //打开MianUI逻辑
        Singleton<WindowManager>.Instance.OpenWindow(new WindowInfo(WindowType.MainUI, ShowMode.Main,
            OpenAction.DoNothing, ColliderMode.Node));
        Singleton<WindowManager>.Instance.CloseWindow(new WindowInfo(WindowType.LoginAndRegister, ShowMode.Normal,
            OpenAction.DoNothing, ColliderMode.Node));
    }

    private void Create(OperationResponse response)
    {
        //0 成功 1 角色名重复
        short retCode = response.ReturnCode;
        WindowBase windowBase = null;
        windowBase = Singleton<WindowManager>.Instance.GetwWindowBase(WindowType.MainUI);

        switch (retCode)
        {
            case 0:
                PlayerInfoModel playerInfo = CommonTool.Deserialize<PlayerInfoModel>((byte[]) response.Parameters[1]);
                GameData.PlayerInfoModel = playerInfo;
                if (windowBase != null)
                {
                    (windowBase as MainUI).ShowInfo();
                    (windowBase as MainUI).SetFirstActivite(false);
                }
               break;
            case 1:
               if (windowBase != null)
                {
                    (windowBase as MainUI).SetCreateButton(true);
                }
                Debug.Log(response.DebugMessage);
                break;
        }
    }
}