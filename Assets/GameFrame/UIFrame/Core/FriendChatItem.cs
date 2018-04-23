using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendChatItem : MonoBehaviour
{
    public bool IsMySelf = false;

    public string ChatString = string.Empty;
    private GameObject RawImage = null;
    private GameObject Balloon = null;
    private Text NameText = null;
    private Text ChatText = null;
    private GameObject LowerPadding = null;
    void Awake()
    {
        RawImage = transform.Find("RawImage").gameObject;
        Balloon = transform.Find("Balloon").gameObject;
        NameText = Balloon.transform.Find("Text").GetComponent<Text>();
        ChatText = Balloon.transform.Find("Lower/Image/Text").GetComponent<Text>();
        LowerPadding = Balloon.transform.Find("Lower/Padding").gameObject;

    }
    public void SetChatItem(bool isMyself,string chatString,string name)
    {
        this.IsMySelf = isMyself;
        this.ChatString = chatString;
        if (isMyself)
        {
            RawImage.transform.SetAsLastSibling();
            NameText.alignment = TextAnchor.UpperRight;
            LowerPadding.transform.SetAsFirstSibling();
            NameText.text = GameData.PlayerInfoModel.RoleName;
        }
        else
        {
            RawImage.transform.SetAsFirstSibling();
            NameText.alignment = TextAnchor.UpperLeft;
            LowerPadding.transform.SetAsLastSibling();
            NameText.text = name;
        }
        //设置聊天文字
        ChatText.text = chatString;
    }
}
