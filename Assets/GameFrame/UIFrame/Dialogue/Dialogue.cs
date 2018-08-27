using System.Collections;
using System.IO;
using GameFrame;
using GameFrameDebuger;
using UIFrameWork;
using UIFrameWork.Dialogue;
using UnityEngine;
using UnityEngine.UI;

public class DialogueContent : WindowContext
{
    public string FileName ;
    public string StartNode;
    public Sprite LeftIcon;
    public GameObject LeftModel;
    public Sprite RightIcon;
    public GameObject RightModel;
    public int ShowType = 0;// 不显示 1 显示ICON 2 显示 MODEL
    public DialogueContent(string file,string node,Sprite lefticon = null ,Sprite righticon = null,GameObject leftmodle = null,GameObject rightmodel = null,int show = 0)
    {
        FileName = file;
        StartNode = node;
        LeftIcon = lefticon;
        RightIcon = righticon;
        LeftModel = leftmodle;
        RightModel = rightmodel;
        ShowType = show;
    }
}
/// <summary>
/// 对话
/// </summary>
public class Dialogue : WindowBase
{

    private Image LeftIcon = null;
    private Image RightIcon = null;
    private Transform LeftModel = null;
    private Transform RightModel = null;

    private Text ShowText = null;
    private Text TitleText = null;
    private GameObject[] Answers = new GameObject[4];
    private DialogueContent Content;
    private DialogueData Data;
    private DialogueNodeData NodeData;
    private Coroutine Coroutine;
    protected override void OnInit(Camera UICamera)
    {
        base.OnInit(UICamera);
        LeftIcon = CacheTransform.Find("Content/LeftImage").GetComponent<Image>();
        RightIcon = CacheTransform.Find("Content/RightImage").GetComponent<Image>();
        LeftModel = CacheTransform.Find("Content/LeftModel");
        RightModel = CacheTransform.Find("Content/RightModel");
        ShowText = CacheTransform.Find("Content/ShowText").GetComponent<Text>();
        TitleText = CacheTransform.Find("Content/TitleText").GetComponent<Text>();
        for (int i = 0; i < 4; i++)
        {
            Answers[i] = CacheTransform.Find("Content/AnswerBtn" + (i + 1)).gameObject;
            EventTriggerListener.Get(Answers[i].gameObject).SetEventHandle(EnumTouchEventType.OnClick, (a, b, c) =>
            {
                StopContentText();
                string nextNode = NodeData.Answers[(int) (c[0])].Target;
                if (nextNode == "end")
                {
                    //关闭界面
                    Singleton<WindowManager>.GetInstance().CloseWindow(false,"Dialogue");
                    return;
                }
                NodeData = GetDialogue(nextNode);
                Coroutine = SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(ShowContentText());
            },i);
        }
    }
    
    private void LoadDialogueData(string filename)
    {
        string filePath = Platform.Path + filename;
        if (File.Exists(filePath))
        {
            Data = JsonUtility.FromJson<DialogueData>(File.ReadAllText(filePath));
        }
        else
        {
            Debuger.LogError("加载Dialogue文件出错");
        }
    }

    private DialogueNodeData GetDialogue(string nodename)
    {
        for (int i = 0; i < Data.Nodes.Length; i++)
        {
            if (Data.Nodes[i].Character == nodename)
            {
                return Data.Nodes[i];
            }
        }
        return null;
    }
    
    protected override void OnAppear(int sequence, int openOrder, WindowContext context)
    {
        base.OnAppear(sequence, openOrder, context);
        Content = context as DialogueContent;
//        LeftIcon.sprite = Content.LeftIcon;
//        RightIcon.sprite = Content.RightIcon;
//        Content.LeftModel.transform.SetParent(LeftModel);
//        Content.RightModel.transform.SetParent(RightModel);
        LoadDialogueData(Content.FileName);
        this.NodeData = GetDialogue(Content.StartNode);
        Coroutine = SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(ShowContentText());
    }

    private void StopContentText()
    {
        SingletonMono<GameFrameWork>.GetInstance().StopCoroutine(Coroutine);
//        TitleText.text = "";
//        ShowText.text = "";
    }
    
    private IEnumerator ShowContentText()
    {
        TitleText.text = NodeData.Character;
        //显示按钮
        for (int i = 0; i < 4; i++)
        {
            if (NodeData.Answers.Length > i)
            {
                Answers[i].SetActive(true);
                Answers[i].transform.Find("Text").gameObject.GetComponent<Text>().text = NodeData.Answers[i].Option;
            }
            else
            {
                Answers[i].SetActive(false);
            }
        }
        ShowText.text = "";
//        Debuger.LogError("hhhhhhhh       "+NodeData.Dialogue);
        char[] letters = NodeData.Dialogue.ToCharArray();
        for (int i = 0; i < letters.Length; i++)
        {
            ShowText.text += letters[i];
            yield return new WaitForEndOfFrame();
        }
    }
    
    
    
    protected override void OnHide(WindowContext context)
    {
        base.OnHide(context);
        StopContentText();
    }
    
    protected override void OnClose(WindowContext context)
    {
        base.OnClose(context);
        this.NodeData = null;
        StopContentText();
    }
}
