using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using celia.game;

[CreateAssetMenu(menuName = "SDKParams")]
public class SDKParams : ScriptableObject
{
    public SDKType SDKType;
    public string AppKey;
    public string PayKey;
}