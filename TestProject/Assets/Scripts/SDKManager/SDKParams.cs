﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using celia.game;

[CreateAssetMenu(menuName = "SDKSetting")]
public class SDKParams : ScriptableObject
{
    public SDKType SDKType;
    public int GameId;
    public string AppKey;
    public string PayKey;
}