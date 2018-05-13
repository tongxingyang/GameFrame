using System.Collections.Generic;
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
            case (byte)PlayerInfoOpCode.GetFriend:
                GetFriends(response);
                break;
        }
    }

    private void GetFriends(OperationResponse response)
    {
        //error
        if (response.ReturnCode == 0)
        {
            Debug.Log(response.DebugMessage);
        }else if (response.ReturnCode == 1)
        {
            //ok
            //打开面板
            List<FriendModel> list = CommonTool.Deserialize<List<FriendModel>>((byte[])response.Parameters[1]);
            List<FriendModel> addlist = CommonTool.Deserialize<List<FriendModel>>((byte[])response.Parameters[2]);
            GameData.FriendList = list;
            GameData.AddFriendList = addlist;

            WindowManager.GetInstance().OpenWindow("FriendUI",true,true, new FriendsContent(list,addlist));
            //刷新ui显示
            
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
        Singleton<WindowManager>.Instance.OpenWindow("MainUI",true);
        Singleton<WindowManager>.Instance.CloseWindow(false, "LoginAndRegister");
    }

    private void Create(OperationResponse response)
    {
        //0 成功 1 角色名重复
        short retCode = response.ReturnCode;
        WindowBase windowBase = null;
        windowBase = Singleton<WindowManager>.Instance.GetWindow("MainUI");

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