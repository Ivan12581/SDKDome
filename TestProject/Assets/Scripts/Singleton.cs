using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T:class,new() 
{
    // 用于lock块的对象
    private static readonly object _synclock = new object();
    static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_synclock)
                {
                    if (_instance == null)
                    {
                        // 若T class具有私有构造函数,那么则无法使用SingletonProvider<T>来实例化new T();
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
    // 提供外部访问的静态方法，来对内部唯一实例进行访问 //
    public static T GetInstance()
    {
        return Instance;
    }
    public static void Clear()
    {
        _instance = default;
    }
}
