using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

class UserLogin
{
    public string uname;
    public string upwd;
    public int status;
}

public class GameApp : UnitySingleton<GameApp>
{
    public void Init()
    {
    }


    public IEnumerator EnterGame() //进入游戏
    {
        Debug.Log("Enter Game");
        
        //测试同步资源加载
        TextAsset t = ResMgr.Instance.LoadAssetSync<TextAsset>("Datas/突发公共卫生事件_CaseSurvey");
        Debug.Log(t.text);
        //end
        
        //异步加载
        var h = ResMgr.Instance.LoadAssetAsync<TextAsset>("Datas/突发公共卫生事件_Disinfect");
        yield return h;
        t = h.AssetObject as TextAsset;
        h.Dispose();//h.Release();
        Debug.Log(t.text);
        //end
        
        //编写游戏逻辑
        // yield return SceneMgr.Instance.IE_RunScene("Scenes/main.unity");
        TestGame();
        yield break;
    }
    
    void TestGame()
    {
        SceneMgr.Instance.RunScene("Scenes/main");
        // Debugger.Log("test game1");
        // Debugger.Log("test game2");
        // Debugger.Log("test game3");
        // Debugger.Log("test game4");
        // Debugger.Log("test game5");
        
        // EventMgr.Instance.AddListener("test", this.OnTestCall);
        // EventMgr.Instance.RemoveListener("test", this.OnTestCall);
        // EventMgr.Instance.Emit("test", 77777);
        
        //Timer测试
        // TimerMgr.Instance.ScheduleOnce((o =>
        // {
        //     Debug.Log("one time ###");
        // }), 5f);
        //
        // int timerId = TimerMgr.Instance.Schedule(((object udata) =>
        // {
        //     Debug.Log("every time###");
        // }), -1, 1, 5f);
        //
        // TimerMgr.Instance.ScheduleOnce(((object udata) =>
        // {
        //     Debug.Log("cancel time");
        //     TimerMgr.Instance.Unschedule(timerId);
        // }), 10);
        
        //测试声音
        // SoundMgr.Instance.PlayMusic("Sounds/回答1_01");
        // SoundMgr.Instance.PlaySound("Sounds/回答1_01");
        // SoundMgr.Instance.PlayOneShot("Sounds/回答1_01");
        
        
        // //json序列化与反序列化的测试
        // UserLogin user = new UserLogin();
        // user.uname = "test";
        // user.upwd = "123456";
        // user.status = -101;
        //
        // string jsonStr = JsonMapper.ToJson(user);
        // Debug.Log(jsonStr);
        //
        // var byteData = jsonStr.ToCharArray();
        //
        // //反序列化
        // string jsonFileStr = ResMgr.Instance.LoadAssetSync<TextAsset>("Datas/JsonText").text;
        // JsonData jsonData = JsonMapper.ToObject(jsonFileStr);
        // Debug.Log(jsonData.ToString());
        // var resCode = jsonData["rst"]["rstCode"].ToString();
        // Debug.Log(resCode);
        
        GameDataMgr.Instance.ReadConfigData<fragment>(null, false);
        fragment data = GameDataMgr.Instance.GetConfigData<fragment>("100101");
        Debug.Log(data.fruitId);
        // yield break;
    }

    private void OnTestCall(string test, object udata)
    {
        Debug.Log("OnTestCall");
        Debug.Log("udata");
    }
}