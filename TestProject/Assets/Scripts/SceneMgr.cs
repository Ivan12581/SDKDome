using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class EntryScene : System.Attribute
{
}

namespace celia.game
{
	public class SceneMgr : SingleClass<SceneMgr>
    {
        [EntryScene]
        public const string GAME_START = "GameStart";
    }
}
