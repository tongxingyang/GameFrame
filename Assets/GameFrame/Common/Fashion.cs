using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Common
{
    /// <summary>
    /// 更换角色时装实现简单逻辑
    /// </summary>
    public class Fashion:MonoBehaviour
    {
        private CombineInstance[] CombineInstances = new CombineInstance[8];
        private SkinnedMeshRenderer Renderer;
        List<Texture2D> list = new List<Texture2D>();
        Mesh[] Meshes = new Mesh[8];
        List<Vector2[]> oldUV = null;
        private string[] Parts = new[]
        {
            "Player_archer_body", "Player_archer_boots", "Player_archer_glove", "Player_archer_hair", "Player_archer_head",
            "Player_archer_leg", "Player_archer_quiver","ar_ydd1_d01_helmet"
        };
        private string[] Parts1 = new[]
        {
            "ar_ydd1_d01_body", "ar_ydd1_d01_boots", "ar_ydd1_d01_glove", "Player_archer_hair", "Player_archer_head",
            "ar_ydd1_d01_leg", "ar_ydd1_d01_quiver","ar_ydd1_d01_helmet"
        };
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeRole(Parts1);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                ChangeRole(Parts);
            }
        }
        void ChangeRole(string[] Parts)
        {
            list.Clear();
            oldUV = new List<Vector2[]>();
            Renderer = transform.GetComponent<SkinnedMeshRenderer>();
            for (int i = 0; i < Parts.Length; i++)
            {
                Mesh mesh = Resources.Load<Mesh>("Equipments/" + Parts[i]);
                Meshes[i] = mesh;
                Texture2D texture2D = Resources.Load<Texture2D>("Equipments/" + Parts[i]);
                list.Add(texture2D);
                CombineInstance instance = new CombineInstance();
                instance.mesh = mesh;
                CombineInstances[i] = instance;
            }
            if (Renderer.sharedMesh != null)
            {
                Renderer.sharedMesh.Clear();
            }
            var newtexture2D = new Texture2D(1024, 1024, TextureFormat.RGBA32, true);
            Rect[] uvs = newtexture2D.PackTextures(list.ToArray(), 0);
            Vector2[] uva, uvb;
            //修改UV坐标
            for (int i = 0; i < CombineInstances.Length; i++)
            {
                uva = CombineInstances[i].mesh.uv;
                //   0.0   0.9      (x:0.00, y:0.50, width:0.25, height:0.50)
                uvb = new Vector2[uva.Length];
                for (int j = 0; j < uva.Length; j++)
                {
                    uvb[j] =  new Vector2((uva[j].x * uvs[i].width) + uvs[i].x, (uva[j].y * uvs[i].height) + uvs[i].y);
                }
                oldUV.Add(uva);
                CombineInstances[i].mesh.uv = uvb;
            }
            var tempMesh = new Mesh();
            Renderer.enabled = false;
            tempMesh.CombineMeshes(CombineInstances,true,false);
            Renderer.sharedMesh = tempMesh;
            Renderer.enabled = true;
		
            Material material = Renderer.material;
            material.mainTexture = newtexture2D;
		
            for (int i = 0; i < CombineInstances.Length; i++)
            {
                CombineInstances[i].mesh.uv = oldUV[i];
            }
        }
    }
}