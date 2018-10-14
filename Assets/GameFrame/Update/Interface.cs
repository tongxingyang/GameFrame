using System.Collections;
using UnityEngine;

namespace GameFrame
{
    public class Interface:Singleton<Interface>
    {
        public override void Init()
        {
            base.Init();
            InitPlugin();
        }

        //初始化sdk
        public void InitPlugin()
        {
        }

        public IEnumerator CheckSDK()
        {
            // TODO
            yield return new WaitForSeconds(5);
        }
        public IEnumerator InitSDK()
        {
            //TODO
            yield return new WaitForSeconds(5);
        }
        
       
    }
}