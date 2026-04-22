using System.IO;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private PauseMenu instance;

    //temporary, doesn't go thru input control
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Pausing");
            Pause();
        }
    }
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        gameObject.GetComponentInChildren<Canvas>().enabled = false;
    }
    public void Pause()
    {
        gameObject.GetComponentInChildren<Canvas>().enabled = true;
        Time.timeScale = 0f;
        Object.FindFirstObjectByType<InputReader>().SetEnabled(false);
    }
    public void Resume()
    {
        gameObject.GetComponentInChildren<Canvas>().enabled = false;
        Time.timeScale = 1f;
        Object.FindFirstObjectByType<InputReader>().SetEnabled(true);
    }
    public void Restart()
    {
        //reset game manager and journal (probably need custom function in Journal for hard reset
        Debug.LogWarning("Instantiation not complete: must hard-reset journal and game manager (when it exists)");
        QuitHome();
    }
    public void QuitHome()
    {
        Resume();
        //FindFirstObjectByType<SceneFader>().FadeTo(0); //needs to go to title scene
        Debug.LogWarning("Quit to title: Not instantiated yet, no title scene in SceneFader");
    }
}
