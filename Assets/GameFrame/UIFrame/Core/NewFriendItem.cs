using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.UI;

public class NewFriendItem : MonoBehaviour {
    private FriendModel friendModel;
    public int PlayerID;
    public int AccountID;
    public FriendModel FriendModel
    {
        get { return friendModel; }
    }

    private Image Icon;
    private Text Name;
    private Text Lv;
    private Button Tongyi;
    private Button Jujue;
    void Awake()
    {
        Icon = transform.Find("Icon").GetComponent<Image>();
        Name = transform.Find("Name").GetComponent<Text>();
        Lv = transform.Find("Lv").GetComponent<Text>();
        Tongyi = transform.Find("Tongyi").GetComponent<Button>();
        Tongyi = transform.Find("Jujue").GetComponent<Button>();
    }
    public void Init(FriendModel Model)
    {
        friendModel = Model;
        // icon todo 
        Name.text = FriendModel.PlayerInfo.RoleName;
        Lv.text = FriendModel.PlayerInfo.LV.ToString();
        PlayerID = FriendModel.PlayerInfo.ID;
        AccountID = FriendModel.PlayerInfo.AccountID;
    }
}
