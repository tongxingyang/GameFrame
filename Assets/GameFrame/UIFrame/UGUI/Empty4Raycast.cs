using UnityEngine.UI;

namespace GameFrame.UGUI
{
    /// <summary>
    /// 接收点击事件但是不去渲染降低OverDraw 原作者钱康来
    /// </summary>
    public class Empty4Raycast : MaskableGraphic
    {
        protected Empty4Raycast()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();

        }
    }
}