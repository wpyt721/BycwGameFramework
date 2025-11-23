using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneMgr : UnitySingleton<SceneMgr>
{
    public void Init()
    {
        
        
        
    }

    public void EnterScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public async Task RunScene(string sceneName)
    {
        var h = ResMgr.Instance.LoadSceneAsync(sceneName, "DefaultPackage");
        await h.Task;
        h.Dispose();
    }

    public IEnumerator IE_RunScene(string sceneName)
    {
        YooAsset.SceneHandle h = ResMgr.Instance.LoadSceneAsync(sceneName, "DefaultPackage");
        yield return h;
    }
}
