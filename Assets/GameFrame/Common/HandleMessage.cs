namespace GameFrame
{
    public interface HandleMessage
    {
        void HandleMessage(string msg, object[] args);
        object HandleMessageRetValue(string msg, object[] args);
    }
}