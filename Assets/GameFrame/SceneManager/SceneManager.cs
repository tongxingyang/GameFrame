using System;
using System.Collections;
using UIFrameWork;
using UnityEngine;

namespace GameFrame.Scene
{
    public class SceneManager:SingletonMono<SceneManager>
    {
        public Action BeforLoadScene = null;
        public Action AfterLoadScene = null;
        public Action<int> ProgressLoad = null;
        public AsyncOperation AsyncOperation = null;
        public string CurrentSceneName = string.Empty;

        public void Clear()
        {
             BeforLoadScene = null;
             AfterLoadScene = null;
             ProgressLoad = null;
             AsyncOperation = null;
             CurrentSceneName = string.Empty;
        }

        public void LoadScene(string scencename,Action befor,Action after,Action<int> progress)
        {
            Clear();
            CurrentSceneName = scencename;
            AsyncOperation = null;
            BeforLoadScene = befor;
            AfterLoadScene = after;
            ProgressLoad = progress;
            //打开loading界面
            Singleton<WindowManager>.GetInstance().OpenWindow("Loading", true,true ,new LoadingContent(scencename, () =>
            {
                StartCoroutine(StartLoading(scencename));
            }));
        }

        protected IEnumerator StartLoading(string name)
        {
            int displayProgress = 0;
            int toProgress = 0;
            if (BeforLoadScene != null)
            {
                BeforLoadScene.Invoke();
            }
            AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);
            AsyncOperation.allowSceneActivation = false;
            while (AsyncOperation.progress < 0.9f)
            {
                toProgress = (int)AsyncOperation.progress * 100;
                while (displayProgress < toProgress)
                {
                    ++displayProgress;
                    if (ProgressLoad != null)
                    {
                        ProgressLoad.Invoke(displayProgress);
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            toProgress = 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                if (ProgressLoad != null)
                {
                    ProgressLoad.Invoke(displayProgress);
                }
                yield return new WaitForEndOfFrame();
            }
            AsyncOperation.allowSceneActivation = true;
            AsyncOperation = null;
            //切换完成的回调
            if (AfterLoadScene != null)
            {
                AfterLoadScene.Invoke();
            }
            //关闭Loading界面
            Singleton<WindowManager>.GetInstance().CloseWindow(false, "Loading");
        }

    }
}