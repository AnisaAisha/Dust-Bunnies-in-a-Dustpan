using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Vector3 playerCoords;       // used for when the player teleports to a new scene and coords need to be preserved
    private Quaternion playerRotation;  // ^
     
    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private PlayerController player;
    public static PlayerController Player {
        get => Instance.player;
        set { 
            if (Instance.player == null && value) {
                Instance.player = value;
                // can add an event here that notifies systems that the player has been assigned
            }
        }
    }

    /// <summary>
    /// Store position and rotation based on the players position
    /// relative to the room to be used when they next spawned in
    /// </summary>
    public static void StorePlayerPosition() {

    }
}
