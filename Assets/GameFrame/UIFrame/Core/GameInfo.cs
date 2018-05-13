using System;
using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class GameInfo : WindowBase
{
    private Action<GameObject, object, object[]> oneButtonAction = null;
    private Action<GameObject, object, object[]> twoButtonAction = null;
    private GameInfoContent currentGameInfoContent;//用于界面resume的数据
    private Text showInfoText = null;
    private Text titleText = null;
    private Button oneButton = null;
    private Button twoButton = null;
    private RectTransform oneRectTransform;
    protected override void OnInit(Camera UICamera)
    {
        //加载组建
        showInfoText = this.CacheTransform.Find("BG/TextContent").GetComponent<Text>();
        titleText = this.CacheTransform.Find("BG/TextTitle").GetComponent<Text>();
        oneButton = this.CacheTransform.Find("BG/Button1").GetComponent<Button>();
        twoButton = this.CacheTransform.Find("BG/Button2").GetComponent<Button>();
        EventTriggerListener.Get(oneButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
            (a, b, c) =>
            {
                if (oneButtonAction != null)
                {
                    oneButtonAction(a, b, c);
                }
            });
        EventTriggerListener.Get(twoButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
            (a, b, c) =>
            {
                if (twoButtonAction != null)
                {
                    twoButtonAction(a, b, c);
                }
            });
        oneRectTransform = oneButton.GetComponent<RectTransform>();
    }

    protected override void OnAppear(int sequence, int openOrder, WindowContext context)
    {
        GameInfoContent gameInfoContent = context as GameInfoContent;
        currentGameInfoContent = gameInfoContent;
        // InitShow(gameInfoContent);
    }


    protected override void OnHide(WindowContext context)
    {
        //InitShow(currentGameInfoContent);
    }

    private void InitShow(GameInfoContent gameInfoContent)
    {
        if (gameInfoContent.GetShowType() == 2)
        {
            //显示两个按钮 设置连个按钮的回掉
            oneButton.gameObject.SetActive(true);
            twoButton.gameObject.SetActive(true);
            oneButtonAction = null;
            twoButtonAction = null;
            oneButtonAction += gameInfoContent.GetOneAction();
            twoButtonAction += gameInfoContent.GetTwoAction();
        }
        else
        {
            oneButton.gameObject.SetActive(true);
            oneRectTransform.localPosition = new Vector3(0,oneRectTransform.localPosition.y, oneRectTransform.localPosition.z);
            oneRectTransform.gameObject.SetActive(false);
            oneButtonAction = null;
            twoButtonAction = null;
            oneButtonAction += gameInfoContent.GetOneAction();
        }
        titleText.text = gameInfoContent.GetTitle();
        showInfoText.text = gameInfoContent.GetContent();
    }
}
