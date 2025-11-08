using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class Boot : UnitySingleton<Boot>
{
    public EPlayMode playMode = EPlayMode.EditorSimulateMode;
    public string remoteURL = "http://127.0.0.1:6080";
    public bool isShowDebugLogWindow = false;
    
    public override void Awake()
    {
        base.Awake();
        
        Application.targetFrameRate = 60;
        // Application.targetFrameRate = -1;
        Application.runInBackground = true;
        
        StartCoroutine(BootStartup());
    }

    private IEnumerator CheckHotUpdate()
    {
        YooAssetHotUpdate.Instance.Init(this.playMode, this.remoteURL);
        yield return YooAssetHotUpdate.Instance.GameHotUpdate();
        yield break;
    }

    private IEnumerator InitFramework() 
    {
        Debug.Log("InitFramework");
        this.gameObject.AddComponent<ResMgr>().Init();
        this.gameObject.AddComponent<EventMgr>().Init();
        this.gameObject.AddComponent<TimerMgr>().Init();
        
        this.gameObject.AddComponent<SceneMgr>().Init();
        this.gameObject.AddComponent<GameApp>().Init();

#if RELEASE_BUILD
#else
        if (this.isShowDebugLogWindow)
        {
            this.gameObject.AddComponent<Debugger>().Init();
        }
#endif
        yield break;
    }

    IEnumerator BootStartup()
    {
        //初始化YooAsset
        YooAssets.Initialize();
        YooAssets.SetOperationSystemMaxTimeSlice(30);
        //end
        
        //检查热跟新
        yield return this.CheckHotUpdate();
        //end
        
        //框架初始化
        yield return this.InitFramework();
        
        //进入游戏
        yield return GameApp.Instance.EnterGame();
    }
}
