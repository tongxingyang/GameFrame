using System;
using UnityEngine;

namespace UIFrameWork.Dialogue
{
    [Serializable]
    public class DialogueData
    {
        public Rect Rect;
        public DialogueNodeData[] Nodes;

        public DialogueData()
        {
            Nodes = new DialogueNodeData[1];
            Nodes[0] = new DialogueNodeData();
        }
    }
}