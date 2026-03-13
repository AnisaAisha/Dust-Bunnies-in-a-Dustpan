using UnityEngine;
using UnityEngine.SceneManagement;

public class OverlayLoader : MonoBehaviour
{
    public void LoadOverlayScene(string sceneName) {
        Debug.Log("Load: " +  sceneName);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void UnloadOverlayScene(string sceneName) {
        SceneManager.UnloadSceneAsync(sceneName);
    }
}
