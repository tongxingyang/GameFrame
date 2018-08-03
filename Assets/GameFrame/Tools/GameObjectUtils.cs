using System;
using UnityEngine;

namespace GameFrame
{
    public class GameObjectUtils
    {
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
    }
}