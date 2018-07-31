using System.Collections;
using Common.Protocol;
using GameFrame.NetWork.Handles;
using ServerFrame.Encode;

namespace GameFrame.NetWork
{
    public class HandleCenter
    {
        public AccountHandle AccountHandle = new AccountHandle();
        public PlayerInfoHandle PlayerInfoHandle = new PlayerInfoHandle();
        public void MessageReceive(SocketModel model)
        {
            switch (model.area)
            {
                case Protocol.Account:
                    AccountHandle.MessageReceive(model);
                    break;
                case Protocol.PlayerInfo:
                    PlayerInfoHandle.MessageReceive(model);
                    break;
            }
        }
    }
}