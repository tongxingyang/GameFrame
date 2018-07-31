using System;
using Common;
using Common.Model;
using Common.Protocol;
using GameFrame;
using GameFrame.NetWork;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

public class LoginAndRegister : WindowBase
{
    private GameObject loginGameObject;
    private GameObject registerGameObject;

    #region Login

    private InputField usernameField;
    private InputField passworldField;
    private Button loginButton;
    private Button regButton;

    #endregion

    #region Register

    private InputField registerusernameField;
    private InputField registerpassworld1Field;
    private InputField registerpassworld2Field;
    private Button registerButton;
    private Button cancleButton;

    #endregion

    protected override void OnInit(Camera UICamera)
    {
        //初始化
        loginGameObject = this.CacheTransform.Find("LoginBG").gameObject;
        registerGameObject = this.CacheTransform.Find("RegisterBG").gameObject;
        usernameField = loginGameObject.transform.Find("InputUsername").GetComponent<InputField>();
        passworldField = loginGameObject.transform.Find("InputPassword").GetComponent<InputField>();
        loginButton = loginGameObject.transform.Find("ButtonLogin").GetComponent<Button>();
        regButton = loginGameObject.transform.Find("ButtonRegister").GetComponent<Button>();

        registerusernameField = registerGameObject.transform.Find("InputUsername").GetComponent<InputField>();
        registerpassworld1Field = registerGameObject.transform.Find("InputPassword").GetComponent<InputField>();
        registerpassworld2Field = registerGameObject.transform.Find("InputPasswordAgain").GetComponent<InputField>();
        registerButton = registerGameObject.transform.Find("ButtonRegister").GetComponent<Button>();
        cancleButton = registerGameObject.transform.Find("ButtonCancel").GetComponent<Button>();

        loginGameObject.SetActive(true); registerGameObject.SetActive(false);
        EventTriggerListener.Get(loginButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick, (a, b, c) =>
        {
            OnLogin();
        });
        EventTriggerListener.Get(registerButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick, (a, b, c) =>
        {
            OnLoginRegister();
        });
        EventTriggerListener.Get(regButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
            (a, b, c) =>
            {
                OnRegister();

            });
        EventTriggerListener.Get(cancleButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
            (a, b, c) =>
            {
                OnCancle();
            });
    }

    public void SetLoginButton(bool iscanuse)
    {
        loginButton.interactable = iscanuse;
    }
    public void SetRegisterButton(bool iscanuse)
    {
        registerButton.interactable = iscanuse;
    }
    private void OnLogin()
    {
        if (usernameField.text == string.Empty)
        {
            Debug.Log("请输入用户名");return;
        }
        if (passworldField.text == string.Empty)
        {
            Debug.Log("请输入密码");return;
        }
       
        GameData.AccountData.AccountName = usernameField.text;
        GameData.AccountData.Passworld = passworldField.text;
        GameData.AccountData.RegisterTime = DateTime.Today;
        GameData.AccountData.ServerID = 0;
        //禁用掉按钮的点击事件
        SetLoginButton(false);
        //向服务器发送数据模型验证账户是否存在
        SingletonMono<ServerManager>.Instance.Send(Protocol.Account, AccoutProtocal.LOGIN, 0 ,GameData.AccountData);
    }

    private void OnLoginRegister()
    {
        if (registerusernameField.text == string.Empty)
        {
            Debug.Log("用户名不能为空"); return;
        }
        if (registerpassworld1Field.text == string.Empty)
        {
            Debug.Log("密码不弄为空"); return;
        }
        if (registerpassworld2Field.text == string.Empty)
        {
            Debug.Log("请再次输入密码"); return;
        }
        if (registerpassworld1Field.text != registerpassworld2Field.text)
        {
            Debug.Log("两次输入的密码不相同"); return;
        }
        SetRegisterButton(false);
        GameData.AccountData.AccountName = registerusernameField.text;
        GameData.AccountData.Passworld = registerpassworld1Field.text;
        GameData.AccountData.RegisterTime = DateTime.Today;
        GameData.AccountData.ServerID = 0;
        //禁用掉按钮的点击事件
        SetLoginButton(false);
        //向服务器发送数据模型验证账户是否存在
        SingletonMono<ServerManager>.Instance.Send<AccountModel>(Protocol.Account, AccoutProtocal.REGISTER, 0 ,GameData.AccountData);
    }

    private void OnRegister()
    {
        registerGameObject.gameObject.SetActive(true);
        registerusernameField.text = String.Empty;
        registerpassworld1Field.text = string.Empty;
        registerpassworld2Field.text = string.Empty;
        loginGameObject.SetActive(false);
        usernameField.text = string.Empty;
        passworldField.text = string.Empty;
    }

    private void OnCancle()
    {
        registerGameObject.gameObject.SetActive(false);
        registerusernameField.text = String.Empty;
        registerpassworld1Field.text = string.Empty;
        registerpassworld2Field.text = string.Empty;
        loginGameObject.SetActive(true);
        usernameField.text = string.Empty;
        passworldField.text = string.Empty;
    }
}