using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameApp : UnitySingleton<GameApp>
{
    public void Init()
    {
    }


    public void EnterGame() //进入游戏
    {
        //编写游戏逻辑
        SceneMgr.Instance.EnterScene("main");
    }
}