using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneFader sceneFader;

    void Start() {
        GameManager.OnLoadNextSnapshot += LoadNextScene;
    }

    public void LoadNextScene() {
        int index = SceneManager.GetActiveScene().buildIndex + 1;
        StartCoroutine(NextScene(index));
        GameManager.OnLoadNextSnapshot -= LoadNextScene;
    }

    private IEnumerator NextScene(int index) {
        yield return StartCoroutine(sceneFader.FadeOut());

        switch (index) {
            case 0:
                AkUnitySoundEngine.SetState("Scene", "Mirror");
                break;

            case 1:
                AkUnitySoundEngine.SetState("Scene", "Key1");
                break;

            case 2:
                AkUnitySoundEngine.SetState("Scene", "Key2");
                break;

            case 3:
                AkUnitySoundEngine.SetState("Scene", "Key3");
                break;

            default:
                break;
        }
        SceneManager.LoadScene(index);
    }
}
