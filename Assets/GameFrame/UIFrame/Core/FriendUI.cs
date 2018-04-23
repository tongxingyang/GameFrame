using System.Collections.Generic;
using Common;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class FriendUI : WindowBase
{
    private Button CloseButton;
    private FriendsContent friendsContent;
    private GameObject FriendItemObj;
    private GameObject FriendAddItemObj;

    private Transform FriendContent;
    private Transform NewFriendContent;

    private List<FriendItem> FriendItemList = new List<FriendItem>();
    private List<NewFriendItem> AddFriendItemList = new List<NewFriendItem>();

    protected override void OnInitantiate()
    {
        base.OnInitantiate();
        CloseButton = this.CacheTransform.Find("BG/CloseButton").GetComponent<Button>();
        FriendItemObj = this.CacheTransform.Find("BG/FriendList/Viewport/Content/Frienditem").gameObject;
        FriendAddItemObj = this.CacheTransform.Find("BG/NewFriendList/Viewport/Content/NewFrienditem").gameObject;
        FriendContent = this.CacheTransform.Find("BG/FriendList/Viewport/Content");
        NewFriendContent = this.CacheTransform.Find("BG/NewFriendList/Viewport/Content");

        EventTriggerListener.Get(CloseButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
            (a, b, c) =>
            {
                WindowManager.GetInstance().CloseWindow(new WindowInfo(WindowType.FriendUI, ShowMode.Normal,
                    OpenAction.DoNothing, ColliderMode.Node));
            });
    }

    protected override void OnEnter(WindowContext context)
    {
        //感觉content传过来的数据没用 用全局就行 打开之前全局赋值更新过了 // 以后有更改就更新本地数据
        base.OnEnter(context);
        friendsContent = context as FriendsContent;
        //好友
        if (GameData.FriendList != null)
        {
            foreach (FriendModel friendModel in GameData.FriendList)
            {
                GameObject obj = GameObject.Instantiate(FriendItemObj);
                FriendItem friend = obj.GetComponent<FriendItem>();
                friend.Init(friendModel);
                FriendItemList.Add(friend);
                obj.transform.SetParent(FriendContent);
            }
        }
        // todo 好友请求
        if (GameData.AddFriendList != null)
        {
            foreach (FriendModel friendModel in GameData.AddFriendList)
            {
                GameObject obj = GameObject.Instantiate(FriendAddItemObj);
                NewFriendItem friend = obj.GetComponent<NewFriendItem>();
                friend.Init(friendModel);
                AddFriendItemList.Add(friend);
                obj.transform.SetParent(NewFriendContent);
            }
        }
    }
}