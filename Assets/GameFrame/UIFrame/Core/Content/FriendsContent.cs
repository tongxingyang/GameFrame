using System.Collections;
using System.Collections.Generic;
using Common;
using UIFrameWork;
using UnityEngine;

public class FriendsContent : WindowContext
{
    private List<FriendModel> Friendlist = null;
    private List<FriendModel> AddFriendlist = null;
    public List<FriendModel> FriengList
    {
        get { return Friendlist; }
    }
    public List<FriendModel> AddFriendList
    {
        get { return AddFriendlist; }
    }
    public FriendsContent(List<FriendModel> list,List<FriendModel> addlist)
    {
        Friendlist = list;
        AddFriendlist = addlist;
    }
}
