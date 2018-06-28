using System.Collections;
using System.Collections.Generic;
using Common.OperationCode;
using ExitGames.Client.Photon;
using GameFrame;
using UnityEngine;

public class PhotonManager :Singleton<PhotonManager>,IPhotonPeerListener
{

    private AccountReceiver accountReceiver;

    public AccountReceiver AccountReceiver
    {
        get
        {
            if (accountReceiver == null)
            {
                accountReceiver = new AccountReceiver();
            }
            return accountReceiver;
        }
    }
    private PlayerInfoReceiver playerInfoReceiver;

    public PlayerInfoReceiver PlayerInfoReceiver
    {
        get
        {
            if (playerInfoReceiver == null)
            {
                playerInfoReceiver = new PlayerInfoReceiver();
            }
            return playerInfoReceiver;
        }
    } 

    public void Update () {
	    if (!isConnect)
	    {
	        peer.Connect(serverAddress, applicationName);
	    }
        peer.Service();
	}

    public void OnDestory()
    {
        peer.Disconnect();
    }

    public override void Init()
    {
        base.Init();
        peer = new PhotonPeer(this, protocol);
        peer.Connect(serverAddress, applicationName);

    }

    private PhotonPeer peer;
    /// <summary>
    /// IP地址
    /// </summary>
    [SerializeField]
    private string serverAddress = "127.0.0.1:5055";
    /// <summary>
    /// 名字
    /// </summary>
    private string applicationName = "Server";
    /// <summary>
    /// 协议
    /// </summary>
    private ConnectionProtocol protocol = ConnectionProtocol.Udp;
    /// <summary>
    /// 连接Flag
    /// </summary>
    private bool isConnect = false;

    public string GetServerIP()
    {
        return "127.0.0.1";
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        //Debug.Log("Level： "+level+"    Message： "+message);
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        byte opCode = operationResponse.OperationCode;
        byte subCode = (byte) operationResponse.Parameters[0];
        switch (opCode)
        {
            case (byte)OperationCode.Account:
                AccountReceiver.Receiver(subCode,operationResponse);
                break;
            case (byte)OperationCode.PlayerInfo:
                PlayerInfoReceiver.Receiver(subCode,operationResponse);
                break;
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                isConnect = true;
                Debug.Log("链接服务器成功");
                break;
            case StatusCode.Disconnect:
                //Debug.Log("断开连接");
                break;
            case StatusCode.TimeoutDisconnect:
                //Debug.Log("链接超时");
                break;
            case StatusCode.DisconnectByServer:
                break;
            case StatusCode.ExceptionOnConnect:
                //Debug.Log("链接异常");
                break;
            case StatusCode.ExceptionOnReceive:
                //Debug.Log("接收异常");
                break;
            case StatusCode.Exception:
                //Debug.Log("产生异常");
                break;
        }
    }

    public void OnEvent(EventData eventData)
    {
    }
    /// <summary>
    /// 客户端发向服务器发起起请求
    /// </summary>
    public void SendReguest(byte opCode,byte subCode,params object[] args)
    {
        Dictionary<byte,object> dicSend = new Dictionary<byte, object>();
        dicSend[0] = subCode;
        for (int i = 1; i <= args.Length; i++)
        {
            dicSend[(byte)i] = args[i - 1];
        }
        peer.OpCustom(opCode, dicSend, true);
    }
}
