using System;
using UIFrameWork;

public class HintContent:WindowContext
{
    private string hintmsg = String.Empty;

    public HintContent(string msg)
    {
        this.hintmsg = msg;
    }

    public string GetHintMesssge()
    {
        return hintmsg;
    }
}
