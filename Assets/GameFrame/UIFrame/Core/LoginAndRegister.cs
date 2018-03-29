using GameFrame;
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

    protected override void OnInitantiate()
    {
        base.OnInitantiate();
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
    }

    private void OnLogin()
    {
        
    }

protected override void OnEnter(WindowContext context)
    {
        base.OnEnter(context);
    }
}