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

        public void InitPlugin()
        {
            
        }
        
        public IEnumerator CheckSDK()
        {
            yield return new WaitForSeconds(5);
        }
        public IEnumerator InitSDK()
        {
            yield return new WaitForSeconds(5);
        }
        
       
    }
}