using System;
using UnityEngine;

namespace UIFrameWork.Dialogue
{
    [Serializable]
    public class DialogueNodeData
    {
        public Rect Rect;
        public string Character;
        public string Dir;//0    none  1    left    2    right
        public string Dialogue;
        public DialogueOptionData[] Answers;

        public DialogueNodeData(string node,int dias,int answs,string dir)
        {
            Character = node;
            Dialogue = "dialogue";
            Answers = new DialogueOptionData[answs];
            for (int i = 0; i < Answers.Length; i++)
            {
                Answers[i] = new DialogueOptionData();
            }
            Dir = dir;
        }

        public DialogueNodeData()
        {
            Character = "node";
            Dialogue = "dialogue";
            Answers = new DialogueOptionData[1];
            Answers[0] = new DialogueOptionData();
            Dir = "0";
        }
    }
}