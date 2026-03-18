using UnityEngine;

public class Minigame : Interactable
{
    [SerializeField] private OverlayLoader overlayLoader;
    [SerializeField] private string sceneName;      // name of the minigame scene to load
    //public override void OnPickUp() {
    //    base.OnPickUp();

    //    overlayLoader.LoadOverlayScene(sceneName);
    //    Cursor.lockState = CursorLockMode.None;
    //    Cursor.visible = true;
    //}

    //public override void OnPutDown() {
    //    base.OnPutDown();
        
    //    overlayLoader.UnloadOverlayScene(sceneName);
    //    Cursor.lockState = CursorLockMode.Locked;
    //}
}
