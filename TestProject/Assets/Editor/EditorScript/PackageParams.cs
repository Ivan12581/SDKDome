using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PackageParams")]
public class PackageParams : ScriptableObject
{
    //public SDKType SDKType;
    public int GameId;
    public string AppKey;
    public string PayKey;
    public int AppId;
    public int CchId;
    public int MdId;
    public string DeviceID;
    public string AppleAppId;
}
