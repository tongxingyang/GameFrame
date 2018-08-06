namespace GameFrame
{
    public interface HandleMessage
    {
        void HandleMessage(string msg, object[] args);
        object HandleMessageValue(string msg, object[] args);
    }
}