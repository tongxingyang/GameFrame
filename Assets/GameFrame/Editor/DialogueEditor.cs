using System.Collections.Generic;
using System.IO;
using UIFrameWork.Dialogue;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class DialogueEditor : EditorWindow
    {
      	public DialogueData dialogueData;
		public Vector2 scrollPos = Vector2.zero;
	
		private int maxHorizontal = 1000;
		private int maxVertical = 1000;
		private int connectFrom = -1;
		
		Dictionary<string, int> nodeIDs = new Dictionary<string, int>();
		List<KeyValuePair<int, int>> attachedWindows = new List<KeyValuePair<int, int>>();
	
		private string dialogueDataProjectFilePath = "/StreamingAssets/mac/";
		private string dialogueDataProjectFileName = "dialoguedata.json";
	
		[MenuItem ("Window/对话编辑器")]
		static void Init()
		{
			EditorWindow.GetWindow (typeof(DialogueEditor)).Show ();
		}
	
		void OnGUI()
		{
			GUILayout.BeginHorizontal("box");
			dialogueDataProjectFileName = GUILayout.TextField(dialogueDataProjectFileName);
			if (GUILayout.Button ("加载数据"))
			{
				LoadGameData();
			}
			
			if (GUILayout.Button ("新建节点"))
			{
				if (dialogueData == null) dialogueData = new DialogueData();
				CreateNode();
			}
	
			if (dialogueData != null)
			{
				if (GUILayout.Button ("保存数据"))
				{
					SaveGameData();
				}
				
				if (GUILayout.Button ("重新生成"))
				{
					updateDataStructures();
				}
				
				scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos,
					new Rect(0, 0, maxHorizontal, maxVertical));
	
				for (int i = 0; i < attachedWindows.Count; i++)
				{
					Rect start = dialogueData.Nodes[attachedWindows[i].Key].Rect;
					Rect end = dialogueData.Rect;
					if(attachedWindows[i].Value != dialogueData.Nodes.Length) end = dialogueData.Nodes[attachedWindows[i].Value].Rect;
					ConnectNodesWithCurve(start, end);
				}
	
				BeginWindows();
				for (int i = 0; i < dialogueData.Nodes.Length; i++)
				{
					dialogueData.Nodes[i].Rect = GUILayout.Window(i, dialogueData.Nodes[i].Rect, DrawNodeWindow, dialogueData.Nodes[i].Character);
				}
	
				dialogueData.Rect = GUILayout.Window(dialogueData.Nodes.Length, dialogueData.Rect,DrawEndWindow, "end");
				EndWindows();
				GUI.EndScrollView();
			}
			GUILayout.EndHorizontal();
		}
		
		void DrawNodeWindow(int id)
		{
			GUILayout.BeginHorizontal("box");
				if (GUILayout.Button("宽度+"))
				{
					dialogueData.Nodes[id].Rect.width -= 10f;
				}
				if (GUILayout.Button("宽度-"))
				{
					dialogueData.Nodes[id].Rect.width += 10f;
				}
				if (GUILayout.Button("高度-"))
				{
					dialogueData.Nodes[id].Rect.height -= 10f;
				}
				if (GUILayout.Button("高度+"))
				{
					dialogueData.Nodes[id].Rect.height += 10f;
				}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal("box");
				GUILayout.Label("名字");
		
				string oldName = dialogueData.Nodes[id].Character;
				dialogueData.Nodes[id].Character = GUILayout.TextField(dialogueData.Nodes[id].Character);
				dialogueData.Nodes[id].Dir = GUILayout.TextField(dialogueData.Nodes[id].Dir);
				if (oldName != dialogueData.Nodes[id].Character)
				{
				}
			GUILayout.EndHorizontal();
			
				dialogueData.Nodes[id].Dialogue = GUILayout.TextArea(dialogueData.Nodes[id].Dialogue);
			for (int i = 0; i < dialogueData.Nodes[id].Answers.Length; i++) {
				GUILayout.BeginHorizontal("box");
				dialogueData.Nodes[id].Answers[i].Option = GUILayout.TextField(dialogueData.Nodes[id].Answers[i].Option);
				dialogueData.Nodes[id].Answers[i].Target = GUILayout.TextField(dialogueData.Nodes[id].Answers[i].Target);
				if (GUILayout.Button("删除"))
				{
					ArrayUtility.RemoveAt(ref dialogueData.Nodes[id].Answers, i);
					updateDataStructures();
				}
				GUILayout.EndHorizontal();
			}
			
			if (GUILayout.Button("增加"))
			{
				ArrayUtility.Add(ref dialogueData.Nodes[id].Answers, new DialogueOptionData());
				updateDataStructures();
			}
			
			GUILayout.BeginHorizontal("box");
			{
				GUIStyle selectedStyle = new GUIStyle(GUI.skin.button);
				if (connectFrom == id)
				{
					selectedStyle.normal.textColor = Color.red;
				}
	
				if (GUILayout.Button("From", selectedStyle))
				{
					connectFrom = id;
				}
	
				if (GUILayout.Button("To"))
				{
					if (connectFrom != -1)
					{
						DialogueOptionData newAnswer = new DialogueOptionData();
						newAnswer.Target = dialogueData.Nodes[id].Character;
						ArrayUtility.Add(ref dialogueData.Nodes[connectFrom].Answers, newAnswer);
					}
	
					updateDataStructures();
				}
			}
			GUILayout.EndHorizontal();
			
			
			if (dialogueData.Nodes[id].Rect.x + dialogueData.Nodes[id].Rect.width > maxHorizontal)
			{
				maxHorizontal = (int)(dialogueData.Nodes[id].Rect.x + dialogueData.Nodes[id].Rect.width);
			}
			if (dialogueData.Nodes[id].Rect.y + dialogueData.Nodes[id].Rect.height > maxVertical)
			{
				maxVertical = (int)(dialogueData.Nodes[id].Rect.y + dialogueData.Nodes[id].Rect.height);
			}
			GUI.DragWindow();
			
		}
		void DrawEndWindow(int id)
		{
			GUILayout.TextField("结束");
			if (GUILayout.Button("To"))
			{
				if (connectFrom != -1)
				{
					DialogueOptionData newAnswer = new DialogueOptionData();
					newAnswer.Target = "end";
					ArrayUtility.Add(ref dialogueData.Nodes[connectFrom].Answers, newAnswer);
				}
				
				updateDataStructures();
			}
			GUI.DragWindow();
		}
		
		void ConnectNodesWithCurve(Rect start, Rect end) {
			Vector3 startPosition = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
			Vector3 endPosition = new Vector3(end.x, end.y + end.height / 2, 0);
			Vector3 startTangent = startPosition + Vector3.right * 20;
			Vector3 endTangent = endPosition + Vector3.left * 20;
	 
			Handles.DrawBezier(startPosition, endPosition, startTangent, endTangent, Color.black, null, 5);
		}
	
		private void CreateNode()
		{
			ArrayUtility.Add(ref dialogueData.Nodes, new DialogueNodeData());
			updateDataStructures();
		}
	
		private void LoadGameData()
		{
			string filePath = Application.dataPath + dialogueDataProjectFilePath+dialogueDataProjectFileName;
	
			if (File.Exists (filePath)) {
				string dataAsJson = File.ReadAllText (filePath);
				dialogueData = JsonUtility.FromJson<DialogueData> (dataAsJson);
				updateDataStructures();
			} else 
			{
				dialogueData = new DialogueData();
			}
		}
	
		private void SaveGameData()
		{
	
			string dataAsJson = JsonUtility.ToJson (dialogueData, true);
	
			string filePath = Application.dataPath + dialogueDataProjectFilePath+dialogueDataProjectFileName;
			File.WriteAllText (filePath, dataAsJson);
	
		}
	
		private void updateDataStructures()
		{
			nodeIDs.Clear();
			attachedWindows.Clear();
			for (int i = 0; i < dialogueData.Nodes.Length; i++)
			{
				nodeIDs[dialogueData.Nodes[i].Character] = i;
				if (dialogueData.Nodes[i].Rect.x + dialogueData.Nodes[i].Rect.width > maxHorizontal)
				{
					maxHorizontal = (int)(dialogueData.Nodes[i].Rect.x + dialogueData.Nodes[i].Rect.width);
				}
				if (dialogueData.Nodes[i].Rect.y + dialogueData.Nodes[i].Rect.height > maxVertical)
				{
					maxVertical = (int)(dialogueData.Nodes[i].Rect.y + dialogueData.Nodes[i].Rect.height);
				}
			}
			nodeIDs["end"] = dialogueData.Nodes.Length;
			for (int i = 0; i < dialogueData.Nodes.Length; i++)
			{
				for (int ii = 0; ii < dialogueData.Nodes[i].Answers.Length; ii++)
				{
					if(nodeIDs.ContainsKey(dialogueData.Nodes[i].Answers[ii].Target))
						attachedWindows.Add(new KeyValuePair<int, int>(i, nodeIDs[dialogueData.Nodes[i].Answers[ii].Target]));
				}
			}
		}
    }
}