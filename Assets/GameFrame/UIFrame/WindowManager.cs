using System.Collections.Generic;
using GameFrame;
using UIFrameWork;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManager:Singleton<WindowManager>
{
    private Dictionary<WindowInfo, WindowBase> showWindowDic = new Dictionary<WindowInfo, WindowBase>();
    private Dictionary<WindowInfo, WindowBase> cacheWindowDic = new Dictionary<WindowInfo, WindowBase>();
    private Stack<WindowStackData> windowStackDatas = new Stack<WindowStackData>();
    private GameObject mUICamera;
    private WindowInfo _mainWindowInfo = null;
    private GameObject normalRoot = null;
    private GameObject mainRoot = null;
    private GameObject fixedRoot = null;
    private GameObject popupRoot = null;
    private GameObject cacheRoot = null;
    public WindowInfo MainWindowInfo
    {
        get { return _mainWindowInfo; }
    }

    public void Clearup()
    {
        foreach (var wnd in showWindowDic)
        {
            if (wnd.Value != null)
            {
                GameObject.Destroy(wnd.Value.gameObject);
            }
        }
        foreach (var wnd in cacheWindowDic)
        {
            if (wnd.Value != null)
            {
                GameObject.Destroy(wnd.Value.gameObject);
            }
        }
        showWindowDic.Clear();
        cacheWindowDic.Clear();
        windowStackDatas.Clear();
        Resources.UnloadUnusedAssets();
    }
    public void InitWindowManager()
    {
        SetupCanvas();
    }

    public void SetupCanvas()
    {

        GameObject uiRoot = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        uiRoot.transform.position = Vector3.zero;
        uiRoot.transform.localScale = Vector3.one;

        var canvas = uiRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = uiRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0;

        var raycaster = uiRoot.GetComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

        GameObject evebtRoot = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        evebtRoot.transform.localPosition = Vector3.zero;
        evebtRoot.transform.localScale = Vector3.one;

        mUICamera = new GameObject("Camera", typeof(Camera));
        mUICamera.transform.localPosition = Vector3.zero;
        mUICamera.transform.localScale = Vector3.one;

        mainRoot = new GameObject("main");
        mainRoot.transform.parent = uiRoot.transform;
        mainRoot.transform.localPosition = Vector3.zero;
        mainRoot.transform.localScale = Vector3.one;

        fixedRoot = new GameObject("fixed");
        fixedRoot.transform.parent = uiRoot.transform;
        fixedRoot.transform.localPosition = Vector3.zero;
        fixedRoot.transform.localScale = Vector3.one;

        normalRoot = new GameObject("normal");
        normalRoot.transform.parent = uiRoot.transform;
        normalRoot.transform.localPosition = Vector3.zero;
        normalRoot.transform.localScale = Vector3.one;

        popupRoot = new GameObject("popup");
        popupRoot.transform.parent = uiRoot.transform;
        popupRoot.transform.localPosition = Vector3.zero;
        popupRoot.transform.localScale = Vector3.one;

        cacheRoot = new GameObject("cache");
        cacheRoot.transform.parent = uiRoot.transform;
        cacheRoot.transform.localPosition = Vector3.zero;
        cacheRoot.transform.localScale = Vector3.one;
    }

    public void RemoveCamera()
    {
        GameObject.Destroy(mUICamera);
        mUICamera = null;
    }

    public void AddCamera()
    {
        if (mUICamera == null)
        {
            mUICamera = new GameObject("Camera", typeof(Camera));
            mUICamera.transform.localPosition = Vector3.zero;
            mUICamera.transform.localScale = Vector3.one;
        }
    }

    public GameObject GetModeRoot(ShowMode mode)
    {
        switch (mode)
        {
            case ShowMode.Fixed:
                return fixedRoot;
                break;
            case ShowMode.Main:
                return mainRoot;
                break;
            case ShowMode.Normal:
                return normalRoot;
                break;
            case ShowMode.PopUp:
                return popupRoot;
                break;
        }
        return null;
    }

    public GameObject GetCacheRoot()
    {
        return cacheRoot;
    }

    private List<WindowBase> GetAffectWindowList(WindowInfo windowInfo)
    {
        List<WindowBase> list = null;
        switch (windowInfo.OpenAction)
        {
            case OpenAction.DoNothing:
                break;
            case OpenAction.HideAll:
                list = new List<WindowBase>();
                foreach (var windowBase in showWindowDic)//windowinfo windowbase
                {
                    if (windowBase.Key == windowInfo)
                    {
                        continue;
                    }
                    WindowBase tmpwindow = windowBase.Value;
                    if (tmpwindow.IsActivied == false)
                    {
                        continue;
                    }
                    list.Add(tmpwindow);
                }
                break;
            case OpenAction.HideNormalAndMain:
                list = new List<WindowBase>();
                foreach (var basewindow in showWindowDic)
                {
                    if (basewindow.Key == windowInfo)
                    {
                        continue;
                    }
                    if (basewindow.Key.ShowMode != ShowMode.Normal)
                    {
                        continue;
                    }
                    if (basewindow.Key.ShowMode != ShowMode.Normal)
                    {
                        continue;
                    }
                    WindowBase tmpwindow = basewindow.Value;
                    if (tmpwindow.IsActivied == false)
                    {
                        continue;
                    }
                    list.Add(tmpwindow);
                }
                break;
            default:
                break;
        }
        return list;
    }

    private void SetTopWindow(GameObject window)
    {
        var count = window.transform.parent.childCount;
        window.transform.SetSiblingIndex(count - 1);
    }

    public void MakeWindowCollider(WindowInfo windowInfo, GameObject obj)
    {
        GameObject go = null;
        Image image = null;
        Button button = null;
        WindowBase windowBase = null;
        switch (windowInfo.ColliderMode)
        {
            case ColliderMode.Node:
                break;
            case ColliderMode.Dark:
                go = new GameObject("DarkCollider", typeof(RectTransform), typeof(Image), typeof(Button));
                image = go.GetComponent<Image>();
                image.color = new Color(0, 0, 0, 100 / 255f);
                image.raycastTarget = true;
                button = go.GetComponent<Button>();
                button.transition = Selectable.Transition.SpriteSwap;
                button.targetGraphic = image;
                windowBase = obj.GetComponent<WindowBase>();
                button.onClick.AddListener(windowBase.ColliderCallBack);
                break;
            case ColliderMode.Transparent:
                go = new GameObject("ransparencyCollider", typeof(RectTransform), typeof(Image), typeof(Button));
                image = go.GetComponent<Image>();
                image.color = new Color(0, 0, 0, 0);
                image.raycastTarget = true;
                button = go.GetComponent<Button>();
                button.transition = Selectable.Transition.SpriteSwap;
                button.targetGraphic = image;
                windowBase = obj.GetComponent<WindowBase>();
                button.onClick.AddListener(windowBase.ColliderCallBack);
                break;
        }
        if (go != null)
        {
            var rectTran = go.GetComponent<RectTransform>();
            rectTran.transform.SetParent(obj.transform);
            rectTran.transform.SetSiblingIndex(0);
            rectTran.localPosition = Vector3.zero;
            rectTran.anchorMin = new Vector2(0.5f, 0.5f);
            rectTran.anchorMax = new Vector2(0.5f, 0.5f);
            rectTran.pivot = new Vector2(0.5f, 0.5f);
            rectTran.sizeDelta = new Vector2(4000, 4000);
        }
    }

    public WindowBase CreateWindowInstance(WindowInfo windowInfo)
    {

        var prefab = Singleton<ResourceManager>.Instance.LoadResource<GameObject>(windowInfo.PerfabPath);
        var go = GameObject.Instantiate(prefab);
        if (go == null)
        {
            Debug.LogError("实例化失败" + windowInfo.PerfabPath);
            return null;
        }
        go.name = windowInfo.PerfabPath.Substring(windowInfo.PerfabPath.LastIndexOf('/') + 1);
        WindowBase windowBase = go.GetComponent<WindowBase>();
        if (windowBase == null)
        {
            windowBase = go.AddComponent(windowInfo.Script) as WindowBase;
        }
        var modeRoot = GetModeRoot(windowInfo.ShowMode);
        var rectTran = go.GetComponent<RectTransform>();
        rectTran.SetParent(modeRoot.transform);
        rectTran.transform.localPosition = Vector3.zero;
        MakeWindowCollider(windowInfo, go);
        windowBase.Initantiate();
        return windowBase;
    }

    public void PushToCache(WindowBase windowBase, bool isexit, WindowContext windowContext = null)
    {
        showWindowDic.Remove(windowBase.windowInfo);
        cacheWindowDic.Add(windowBase.windowInfo, windowBase);
        var root = GetCacheRoot();
        var rectTran = windowBase.GetComponent<RectTransform>();
        rectTran.SetParent(root.transform);
        rectTran.localPosition = Vector3.zero;
        if (isexit)
        {
            windowBase.Exit(windowContext);
        }
        else
        {
            windowBase.Pause(windowContext);
        }
    }

    public WindowBase PullFromCache(WindowInfo windowInfo, bool isenter, WindowContext windowContext = null)
    {
        WindowBase windowBase = null;
        if (cacheWindowDic.ContainsKey(windowInfo))
        {
            windowBase = cacheWindowDic[windowInfo];
            cacheWindowDic.Remove(windowInfo);
            showWindowDic.Add(windowInfo, windowBase);
            var root = GetModeRoot(windowInfo.ShowMode);
            var rectTran = windowBase.GetComponent<RectTransform>();
            rectTran.SetParent(root.transform);
            rectTran.transform.localPosition = Vector3.zero;
        }
        else
        {
            windowBase = CreateWindowInstance(windowInfo);
            showWindowDic.Add(windowInfo, windowBase);
        }
        if (isenter)
        {
            windowBase.Enter(windowContext);
        }
        else
        {
            windowBase.Resume(windowContext);
        }
        SetTopWindow(windowBase.CacheGameObject);
        return windowBase;
    }

    public void BackToMain()
    {
        List<WindowBase> select = new List<WindowBase>();
        foreach (var windowBase in showWindowDic)
        {
            if (windowBase.Key.ShowMode != ShowMode.Normal)
            {
                continue;
            }
            select.Add(windowBase.Value);
        }
        for (int i = 0; i < select.Count; i++)
        {
            WindowBase temp = select[i];
            PushToCache(temp, true, null);
        }
        windowStackDatas.Clear();
        OpenWindow(_mainWindowInfo, null);
    }

    public WindowBase GetWindow(WindowInfo info)
    {
        WindowBase basewindow = null;
        showWindowDic.TryGetValue(info, out basewindow);
        return basewindow;
    }

    public WindowBase OpenWindow(WindowInfo windowInfo, WindowContext context = null)
    {
        WindowBase sscript = null;
        if (showWindowDic.ContainsKey(windowInfo))
        {
            Debug.LogError("窗口已经被打开了");
            return showWindowDic[windowInfo];
        }
        sscript = PullFromCache(windowInfo, true, context);
        if (windowInfo.ShowMode == ShowMode.Normal)
        {
            List<WindowBase> history = GetAffectWindowList(windowInfo);
            for (int i = 0; i < history.Count; i++)
            {
                PushToCache(history[i], false);
            }
            WindowStackData windowStackData = new WindowStackData();
            windowStackData.HistoryWindowBases = history;
            windowStackData.WindowInfo = windowInfo;
            windowStackData.WindowBase = sscript;
            windowStackDatas.Push(windowStackData);
        }
        return sscript;
    }

    public void CloseWindow(WindowBase windowBase)
    {
        WindowInfo windowInfo = windowBase.windowInfo;
        if (showWindowDic.ContainsKey(windowInfo) == false)
        {
            Debug.LogError("要关闭的窗口当前没有显示");
            return;
        }
        if (windowInfo.ShowMode == ShowMode.Normal)
        {
            if (windowStackDatas.Count > 0)
            {
                WindowStackData windowStackData = windowStackDatas.Peek();
                if (windowStackData.WindowInfo != windowInfo)
                {
                    Debug.LogError("关闭出错");
                    return;
                }
                PushToCache(windowBase, true, null);
                switch (windowInfo.OpenAction)
                {
                    case OpenAction.DoNothing:
                        break;
                    case OpenAction.HideAll:
                        for (int i = 0; i < windowStackData.HistoryWindowBases.Count; i++)
                        {
                            PullFromCache(windowStackData.HistoryWindowBases[i].windowInfo, false, null);
                        }
                        break;
                    case OpenAction.HideNormalAndMain:
                        for (int i = 0; i < windowStackData.HistoryWindowBases.Count; i++)
                        {
                            PullFromCache(windowStackData.HistoryWindowBases[i].windowInfo, false, null);
                        }
                        break;
                    default:
                        break;
                }
                windowStackDatas.Pop();
            }
            else
            {
                Debug.LogError("关闭出错");
                return;
            }
        }
        else if (windowInfo.ShowMode == ShowMode.PopUp)
        {
            PushToCache(windowBase, true, null);
        }
    }

    public void CloseWindow(WindowInfo windowInfo)
    {
        WindowBase windowBase = GetWindow(windowInfo);
        CloseWindow(windowBase);
    }
}