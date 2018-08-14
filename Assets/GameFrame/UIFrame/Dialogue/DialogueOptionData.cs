using System;

namespace UIFrameWork.Dialogue
{
    [Serializable]
    public class DialogueOptionData
    {
        public string Option;
        //目标的节点
        public string Target;
        public DialogueOptionData(string op,string tar)
        {
            Option = op;
            Target = tar;
        }

        public DialogueOptionData()
        {
            Option = "option";
            Target = "end";
        }
    }
}