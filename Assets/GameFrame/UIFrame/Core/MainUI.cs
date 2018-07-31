using System.Collections;
using System.Collections.Generic;
using Common;
using Common.Model;
using GameFrame;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : WindowBase
{
    private GameObject FirstGameObject;
    private Transform HeadTransform;
    private InputField CreatePlayerName;
    private Button CreatePlayerInfoButton;



    // ping 
    private Ping ping;

    private string ip;

    bool isNetWorkLose = false;
    protected override void OnWindowCustomUpdate()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            isNetWorkLose = true;
            PingText.text = "460";
            SetColor(460);
        }
        else 
        {
            isNetWorkLose = false;
        }
        if (!isNetWorkLose)
        {
            Invoke("SendPing", 1);
            if (ping != null && ping.isDone)
            {
                PingText.text = ping.time.ToString();
                SetColor(ping.time);
                ping = null;
            }
        }
    }

    #region Head

    private Image HeadImage;
    private Text RoleName;
    private Text RoleLV;
    private Slider EXPSlider;
    private Text EXPText;
    private Text CoinNumText;
    private Button CoinBuyButton;
    private Button MialButton;
    private Text PingText;
    private Button FriendButton;

    #endregion

    private bool isFirst = true;
    protected override void OnInit(Camera UICamera)
    {
        FirstGameObject = CacheTransform.Find("BG/First").gameObject;
        CreatePlayerName = CacheTransform.Find("BG/First/Board/PlayerName").GetComponent<InputField>();
        CreatePlayerInfoButton = CacheTransform.Find("BG/First/Board/Button").GetComponent<Button>();

        #region Head

        HeadTransform = CacheTransform.Find("BG/Head").transform;
        HeadImage = HeadTransform.Find("HeadImage").GetComponent<Image>();
        RoleName = HeadTransform.Find("RoleName").GetComponent<Text>();
        RoleLV = HeadTransform.Find("RoleLV").GetComponent<Text>();
        EXPSlider = HeadTransform.Find("RoleEXP").GetComponent<Slider>();
        EXPText = HeadTransform.Find("RoleEXP/Text").GetComponent<Text>();
        CoinNumText = HeadTransform.Find("Coinicon/Text").GetComponent<Text>();
        CoinBuyButton = HeadTransform.Find("Coinicon/AddButton").GetComponent<Button>();
        MialButton = HeadTransform.Find("MailButton").GetComponent<Button>();
        PingText = HeadTransform.Find("Ping/Text").GetComponent<Text>();
        FriendButton = HeadTransform.Find("Friend").GetComponent<Button>();
        
        #endregion
        //ping 
//        ip = Singleton<PhotonManager>.Instance.GetServerIP();
        //ip = "115.239.211.112";
    }

    void SendPing()
    {
        ping = new Ping(ip);
    }
    void SetColor(int pingValue)
    {
        if (pingValue < 100)
        {
            PingText.color = new Color(0, 1, 0);
        }
        else if (pingValue < 200)
        {
            PingText.color = new Color(1, 1, 0);
        }
        else
        {
            PingText.color = new Color(1, 0, 0);
        }
    }
    protected override void OnAppear(int sequence, int openOrder, WindowContext context)
    {
        if (isFirst)
        {
            isFirst = false;
            if (GameData.PlayerInfoModel == null)
            {
                FirstGameObject.SetActive(true);
                EventTriggerListener.Get(CreatePlayerInfoButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                    (a, b, c) =>
                    {
                        //点击之后发送注册角色消息
                        PlayerInfoModel playerInfo = new PlayerInfoModel();
                        playerInfo.ID = 0;
                        playerInfo.HeadImg = "1.png";
                        playerInfo.RoleName = CreatePlayerName.text;
                        playerInfo.CoinNum = 200;
                        playerInfo.EXP = 1;
                        playerInfo.LV = 1;
                        playerInfo.AccountID = GameData.AccountData.ID;
                        playerInfo.FriendID = "";
                        playerInfo.HeroID = "";
                        playerInfo.Power = 0;
                        playerInfo.WinNum = 0;
                        playerInfo.LostNum = 0;
                        playerInfo.RunNum = 0;

                        //发送数据模型
//                        Singleton<PhotonManager>.Instance.SendReguest((byte)OperationCode.PlayerInfo, (byte)PlayerInfoOpCode.Create, CommonTool.Serialize<PlayerInfoModel>(playerInfo));
                        SetCreateButton(false);
                    });
            }
            else
            {
                FirstGameObject.SetActive(false);
                ShowInfo();
            }
        }
        else
        {
            ShowInfo();
        }

    }
    public void SetCreateButton(bool iscanuse)
    {
        CreatePlayerInfoButton.interactable = iscanuse;
    }
    public void SetFirstActivite(bool iscanuse)
    {
       FirstGameObject.SetActive(iscanuse);
    }

    public void ShowInfo()
    {
        // todo 头像
        RoleName.text = GameData.PlayerInfoModel.RoleName;
        RoleLV.text = GameData.PlayerInfoModel.LV.ToString();
        EXPSlider.value = GameData.PlayerInfoModel.EXP / (float)100;
        EXPText.text = GameData.PlayerInfoModel.EXP.ToString();
        CoinNumText.text = GameData.PlayerInfoModel.CoinNum.ToString();
        //绑定点击事件
        EventTriggerListener.Get(FriendButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
            (a, b, c) =>
            {
                //发送获取好友列表的请求 在返回之后在打开UI
//                Singleton<PhotonManager>.GetInstance().SendReguest((byte)OperationCode.PlayerInfo,(byte)PlayerInfoOpCode.GetFriend,GameData.PlayerInfoModel.ID);
            });
    }
}
