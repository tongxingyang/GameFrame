using System.Collections;
using UnityEngine;

namespace GameFrame
{
    public class Interface:Singleton<Interface>
    {
        private bool isFinish = false;
        public override void Init()
        {
            base.Init();
            InitPlugin();
        }

        public void InitPlugin()
        {
            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(ChangeFinish());
        }

        IEnumerator ChangeFinish()
        {
            yield return new WaitForSeconds(25);
            isFinish = true;
        }
        public bool IsCaheckSDKFinish()
        {
            return isFinish;
        }
    }
}