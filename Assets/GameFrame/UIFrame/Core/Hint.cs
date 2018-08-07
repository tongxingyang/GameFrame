using GameFrame;
using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork.Core
{
    public class Hint:WindowBase
    {
        private Image content;
        private HintContent hint;
        private Text HintMessagee;
        public float Speed = 0.4f;
        public float alphaSpeed = 6.0f;
        private bool isUpdate = false;
        private Vector3 startPos;
        protected override void OnInit(Camera UICamera)
        {
            base.OnInit(UICamera);
            content = this.CacheTransform.Find("BG").GetComponent<Image>();
            HintMessagee = content.transform.Find("Text").GetComponent<Text>();
            startPos = content.transform.position;
        }

        protected override void OnAppear(int sequence, int openOrder, WindowContext context)
        {
            base.OnAppear(sequence, openOrder, context);
            hint = context as HintContent;
            SetValue(hint.GetHintMesssge());
        }

        private void SetValue(string message)
        {
            HintMessagee.text = message;
            isUpdate = true;
            content.color = new Color(content.color.a,content.color.g,content.color.b,1);
            content.transform.position = startPos;        }

        private void UpdateView()
        {
            var al = Mathf.Lerp(content.color.a,0,alphaSpeed * Time.deltaTime);
            content.color = new Color(content.color.a,content.color.g,content.color.b,al);
            content.transform.position = Vector3.Lerp( content.transform.position, new Vector3(content.transform.position.x,content.transform.position.y+5,content.transform.position.z), Speed);
            if (al <= 0.01)
            {
                this.isUpdate = false;
                ShowNextMessage();
            }
        }

        protected override void OnHide(WindowContext context)
        {
            isUpdate = false;
            base.OnHide(context);
            hint = null;
        }

        protected override void OnWindowCustomUpdate()
        {
            if (isUpdate)
            {
                base.OnWindowCustomUpdate();
                UpdateView();
            }
        }
        private void ShowNextMessage()
        {
            var message = Singleton<WindowManager>.GetInstance().GetHintContext();
            hint = message;
            if (message != null)
            {
                SetValue(message.GetHintMesssge());
            }
            else
            {
                Singleton<WindowManager>.GetInstance().CloseWindow(false,"Hint");
            }
        }
    }
}