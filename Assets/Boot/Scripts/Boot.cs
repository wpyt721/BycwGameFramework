using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boot : UnitySingleton<Boot>
{
    public override void Awake()
    {
        base.Awake();
        
        StartCoroutine(BootStartup());
    }

    private IEnumerator CheckHotUpdate()
    {
        yield break;
    }

    private IEnumerator InitFramework()
    {
        this.gameObject.AddComponent<SceneMgr>().Init();
        this.gameObject.AddComponent<GameApp>().Init();
        yield break;
    }

    IEnumerator BootStartup()
    {
        //检查热跟新
        yield return this.CheckHotUpdate();
        //end
        
        //框架初始化
        yield return this.InitFramework();
        
        //进入游戏
        GameApp.Instance.EnterGame();
        yield break;
    }
}
