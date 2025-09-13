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
}
