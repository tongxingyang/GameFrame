using Common;
using Common.OperationCode;
using ExitGames.Client.Photon;
using GameFrame;
using UIFrameWork;
using UnityEngine;

public class AccountReceiver : ReceiverInterface
{

    public void Receiver(byte subCode, OperationResponse response)
    {
        switch (subCode)
        {
            case (byte)AccountOpCode.Login:
                OnLogin(response);
                break;
            case (byte)AccountOpCode.Register:
                OnRegister(response);
                break;
        }
    }

    private void OnLogin(OperationResponse response)
    {
        WindowBase windowBase = null;
        var retCode = response.ReturnCode;
        var message = response.DebugMessage;
        windowBase = Singleton<WindowManager>.Instance.GetwWindowBase(WindowType.LoginAndRegister);
        if (windowBase != null)
        {
            (windowBase as LoginAndRegister).SetLoginButton(true);
        }
        if (retCode == 0)
        {
            //登陆成功  给本地常量数据更新赋值
            AccountModel account = CommonTool.Deserialize<AccountModel>((byte[])response.Parameters[1]);
            GameData.AccountData.ID = account.ID;
            GameData.AccountData.AccountName = account.AccountName;
            GameData.AccountData.Passworld = account.Passworld;
            GameData.AccountData.ServerID = account.ServerID;
            GameData.AccountData.RegisterTime = account.RegisterTime;
            //用户名验证成功 获取玩家角色信息
            Singleton<PhotonManager>.Instance.SendReguest((byte)OperationCode.PlayerInfo,(byte)PlayerInfoOpCode.GetInfo, GameData.AccountData.ID);

        }
        else if (retCode == 1)
        {
            //用户名不存在
        }else if (retCode == 2)
        {
            //密码错误
        }else if (retCode == 3)
        {
            //玩家在线
        }
        Debug.Log("return code " + retCode + "message  " + message);
    }

    private void OnRegister(OperationResponse response)
    {
        WindowBase windowBase = null;
        var retCode = response.ReturnCode;
        var message = response.DebugMessage;
        windowBase = Singleton<WindowManager>.Instance.GetwWindowBase(WindowType.LoginAndRegister);
        if (windowBase != null)
        { 
            (windowBase as LoginAndRegister).SetRegisterButton(true);
        }
        if (retCode == 0)
        {
            //注册成功
            AccountModel account = CommonTool.Deserialize<AccountModel>((byte[])response.Parameters[1]);
            GameData.AccountData.ID = account.ID;
            GameData.AccountData.AccountName = account.AccountName;
            GameData.AccountData.Passworld = account.Passworld;
            GameData.AccountData.ServerID = account.ServerID;
            GameData.AccountData.RegisterTime = account.RegisterTime;
        }
        Debug.Log("return code " + retCode + "message  " + message);
    }
}
