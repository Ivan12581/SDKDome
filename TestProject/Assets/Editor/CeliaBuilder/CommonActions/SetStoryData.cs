using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace celia.game.editor
{
    public class SetStoryData : CommonAction
    {
        public override void PreExcute(CeliaBuildOption option)
        {
            try
            {
                //if (GameStoryInspector.CheckeGameStoryContainError())
                //{
                //    GameStoryInspector.UpdateGameStory();
                //}

                //if (GameStoryInspector.CheckeGameStoryContainError())
                //{
                //    Debug.Log("GameStory剧情文件已经损坏");
                //}
            }
            catch (System.Exception)
            {

                throw;
            }

        }

        public override void PostExcute(CeliaBuildOption option)
        {
            
        }
    }
}

