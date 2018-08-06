using System;
using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : WindowBase
{
    private Action<GameObject, object, object[]> twoButtonAction = null;
    private MessageBoxContent currentGameInfoContent;//用于界面resume的数据
    private Text showInfoText = null;
    private Text titleText = null;
    private Button oneButton = null;
    private Button twoButton = null;
    protected override void OnInit(Camera UICamera)
    {
        var content = this.CacheTransform.Find("Content").transform;
        showInfoText = content.Find("Message").GetComponent<Text>();
        titleText = content.Find("Title").GetComponent<Text>();
        oneButton = content.Find("Button1").GetComponent<Button>();
        twoButton = content.Find("Button2").GetComponent<Button>();
        EventTriggerListener.Get(oneButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
            (a, b, c) =>
            {
                ShowNextMessage();
            });

    }

    protected void RegisterCallBack()
    {
        twoButtonAction = currentGameInfoContent.GetTwoAction();
        EventTriggerListener.Get(twoButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
            (a, b, c) =>
            {
                if (twoButtonAction != null)
                {
                    twoButtonAction(a, b, c);
                    ShowNextMessage();
                }
            });
    }

    private void ShowNextMessage()
    {
        var message = Singleton<WindowManager>.GetInstance().GetMessageBoxContext();
        if (message != null)
        {
            InitShow(message);
        }else
        {
            Singleton<WindowManager>.GetInstance().CloseWindow(false,"MessageBox");
        }
    }
    
    protected override void OnAppear(int sequence, int openOrder, WindowContext context)
    {
        base.OnAppear(sequence, openOrder, context);
        MessageBoxContent gameInfoContent = context as MessageBoxContent;
        currentGameInfoContent = gameInfoContent;
        InitShow(currentGameInfoContent);
    }

    private void InitShow(MessageBoxContent gameInfoContent)
    {
        if (gameInfoContent.GetShowType() == 2)
        {
            //显示两个按钮 设置连个按钮的回掉
            twoButton.gameObject.SetActive(true);
        }
        else
        {
            twoButton.gameObject.SetActive(false);
        }
        twoButtonAction = null;
        titleText.text = gameInfoContent.GetTitle();
        showInfoText.text = gameInfoContent.GetContent();
        RegisterCallBack();
    }
}
