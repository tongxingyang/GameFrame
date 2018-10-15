using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GameFrame
{
	public  class Util {

		public static void SetResolution(Vector2 resolution)
		{
			#if UNITY_STANDALONE || UNITY_STANDALONE_OSX
				Screen.SetResolution((int)resolution.x, (int)resolution.y, false);
			#else
				Screen.SetResolution((int)resolution.x, (int)resolution.y, true);
			#endif
		}

		#region GameObject

		public static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null)
            {
                return target.AddComponent<T>();
            }
            return comp;
        }

        public static Component EnsureComponent(GameObject target, Type type)
        {
            Component comp = target.GetComponent(type);
            if (comp == null)
            {
                return target.AddComponent(type);
            }
            return comp;
        }

        public static T FindComponent<T>(GameObject target, string path) where T : Component
        {
            GameObject obj = FindGameObject(target, path);
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }
            return default(T);
        }

        public static GameObject FindGameObject(GameObject target, string path)
        {
            if (target != null)
            {
                Transform t = target.transform.Find(path);
                if (t != null)
                {
                    return t.gameObject;
                }
            }

            return null;

        }
        

        public static GameObject FindGameObjbyName(string name, GameObject root)
        {
            if (root == null)
            {
                return GameObject.Find(name);
            }

            Transform[] childs = root.GetComponentsInChildren<Transform>();

            foreach (Transform trans in childs)
            {
                if (trans.gameObject.name.Equals(name))
                {
                    return trans.gameObject;
                }
            }

            return null;
        }


        public static GameObject FindFirstGameObjByPrefix(string prefix, GameObject root)
        {
            Transform[] childs;
            if (root != null)
            {
                childs = root.GetComponentsInChildren<Transform>();
            }
            else
            {
                childs = GameObject.FindObjectsOfType<Transform>();
            }

            foreach (Transform trans in childs)
            {
                if (trans.gameObject.name.Length >= prefix.Length)
                {
                    if (trans.gameObject.name.Substring(0, prefix.Length) == prefix)
                    {
                        return trans.gameObject;
                    }
                }

            }

            return null;
        }


        public static void SetActiveRecursively(GameObject target, bool bActive)
        {
            for (int n = target.transform.childCount - 1; 0 <= n; n--)
                if (n < target.transform.childCount)
                    SetActiveRecursively(target.transform.GetChild(n).gameObject, bActive);
            target.SetActive(bActive);
        }
        
        
        public static void SetLayerRecursively(GameObject target, int layer)
        {
            for (int n = target.transform.childCount - 1; 0 <= n; n--)
            {
                if (n < target.transform.childCount)
                {
                    SetLayerRecursively(target.transform.GetChild(n).gameObject, layer);
                }
            }
            target.layer = layer;
        }
		//获取
	    public static T Get<T>(GameObject go, string subnode) where T : Component
	    {
	        if (go != null)
	        {
	            Transform sub = go.transform.Find(subnode);
	            if (sub != null) return sub.GetComponent<T>();
	        }
	        return null;
	    }

	    public static T Get<T>(Transform go, string subnode) where T : Component
	    {
	        if (go != null)
	        {
	            Transform sub = go.Find(subnode);
	            if (sub != null) return sub.GetComponent<T>();
	        }
	        return null;
	    }

	    public static T Get<T>(Component go, string subnode) where T : Component
	    {
	        return go.transform.Find(subnode).GetComponent<T>();
	    }   
	    //添加
	    public static T Add<T>(GameObject go) where T : Component
	    {
	        if (go != null)
	        {
	            T[] ts = go.GetComponents<T>();
	            for (int i = 0; i < ts.Length; i++)
	            {
	                if (ts[i] != null) UnityEngine.Object.Destroy(ts[i]);
	            }
	            return go.gameObject.AddComponent<T>();
	        }
	        return null;
	    }

	    public static T Add<T>(Transform go) where T : Component
	    {
	        return Add<T>(go.gameObject);
	    }
	    
		public static GameObject Child(GameObject go, string subnode)
		{
			return Child(go.transform, subnode);
		}

		public static GameObject Child(Transform go, string subnode)
		{
			Transform tran = go.Find(subnode);
			if (tran == null) return null;
			return tran.gameObject;
		}

		public static GameObject Peer(GameObject go, string subnode)
		{
			return Peer(go.transform, subnode);
		}

		public static GameObject Peer(Transform go, string subnode)
		{
			Transform tran = go.parent.Find(subnode);
			if (tran == null) return null;
			return tran.gameObject;
		}
	    //base64 编码
		public static string Encode(string message)
		{
			byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
			return Convert.ToBase64String(bytes);
		}
		//base64 解码
		public static string Decode(string message)
		{
			byte[] bytes = Convert.FromBase64String(message);
			return Encoding.GetEncoding("utf-8").GetString(bytes);
		}

		public static bool NetAvailable
		{
			get
			{
				return Application.internetReachability != NetworkReachability.NotReachable;
			}
		}

		public static bool IsWifi
		{
			get
			{
				return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
			}
		}
		public static bool IsCarrier
		{
			get
			{
				return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;
			}
		}
	    
		//根据assembly加载组件
		public static Component AddComponent(GameObject go, string assembly, string classname)
		{
			Assembly asmb = Assembly.Load(assembly);
			Type t = asmb.GetType(assembly + "." + classname);
			return go.AddComponent(t);
		}
		//属性设置
		public static void SetPos(GameObject go, Vector3 pos)
		{
			if (go != null)
			{
				go.transform.position = pos;
			}
		}
		public static void SetRotation(GameObject go, float x, float y, float z)
		{
			if (go != null)
			{
				go.transform.rotation = Quaternion.Euler(x, y, z);
			}
		}
		public static void SetRotation(Component go, float x, float y, float z)
		{
			if (go != null)
			{
				go.transform.rotation = Quaternion.Euler(x, y, z);            
			}
		}
	
		public static void SetLocalRotation(GameObject go, float x, float y, float z)
		{
			if (go != null)
			{
				go.transform.localRotation = Quaternion.Euler(x, y, z);
			}
		}
		public static void SetLocalRotation(Component go, float x, float y, float z)
		{
			if (go != null)
			{
				go.transform.localRotation = Quaternion.Euler(x, y, z);
			}
		}
	
		public static void SetPos(Component co, Vector3 pos)
		{
			if (co != null)
			{
				co.transform.position = pos;
			}
		}
	
		public static void SetPos(GameObject go, float x, float y, float z)
		{
			if (go != null)
			{
				go.transform.position = new Vector3(x, y, z);
			}
		}
	
		public static void SetPos(Component co, float x, float y, float z)
		{
			if (co != null)
			{
				co.transform.position = new Vector3(x, y, z);
			}
		}
	
		public static void SetLocalPos(Component co, float x, float y, float z)
		{
			if (co != null)
			{
				co.transform.localPosition = new Vector3(x, y, z);
			}
		}
		public static void Translate(Transform co, float x, float y, float z)
		{
			if (co != null)
			{
				co.Translate(x, y, z);
			}
		}
		public static void SetLocalPos(Component co, Vector3 pos)
		{
			if (co != null)
			{
				co.transform.localPosition = pos;
			}
		}
	
		public static void SetLocalPos(GameObject go, float x, float y, float z)
		{
			if (go != null)
			{
				go.transform.localPosition = new Vector3(x, y, z);
			}
		}
	
		public static void SetLocalPos(GameObject go, Vector3 pos)
		{
			if (go != null)
			{
				go.transform.localPosition = pos;
			}
		}
		//设置物体的层级
		public static void RecursiveSetLayer(GameObject obj, int layer)
		{
			if (obj == null) return;
			Transform trans = obj.transform;
			for (int i = 0; i < trans.childCount; i++)
			{
				Transform childtrans = trans.GetChild(i);
				RecursiveSetLayer(childtrans.gameObject, layer);
			}
			obj.layer = layer;
		}
		#endregion
	}

}
































