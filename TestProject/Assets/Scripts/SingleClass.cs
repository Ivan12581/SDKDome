using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace celia.game
{
    public class SingleClass<T> where T : new()
    {
        private static T _gi;
        public static T gi
        {
            get
            {
                return _gi;
            }
        }
        public static void Create()
        {
            if (_gi == null)
            {
                _gi = new T();
            }
        }
    }
    public class SingleMonoBehaviour<T> : MonoBehaviour where T : Component
    {
        private static T _gi;
        public static T gi
        {
            get
            {
                return _gi;
            }
        }

        public static void Create(bool dontDestroy = true)
        {
            if (_gi == null)
            {
                _gi = FindObjectOfType<T>();
                if (_gi == null)
                {
                    GameObject obj = new GameObject();
                    _gi = obj.AddComponent<T>();
                    obj.name = typeof(T).Name;
                    if (dontDestroy)
                    {
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(obj);
                        }

                    }
                }
            }
        }

        public static void Create(GameObject holder)
        {
            if (_gi == null)
            {
                _gi = FindObjectOfType<T>();
                if (_gi == null)
                {
                    _gi = holder.AddComponent<T>();
                }
            }
        }

        public static void Create(T com)
        {
            _gi = com;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(_gi.gameObject);
            }
        }
    }
}