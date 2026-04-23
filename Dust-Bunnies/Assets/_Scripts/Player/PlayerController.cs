using UnityEngine;

/// <summary>
/// Player State Machine
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReader input;
    [SerializeField] private PlayerMovement motor;
    [SerializeField] private PlayerInteract interact;
    [SerializeField] private PlayerCamera cam;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private Transform room;
    [SerializeField] private Collider myCollider;
    [SerializeField] private GameObject crosshair;

    [SerializeField] private PlayerDEBUG debug;

    // ACCESSORS ---
    public PlayerMovement Motor => motor;
    public PlayerCamera Camera => cam;
    public PlayerInteract Interact => interact;
    public PlayerDEBUG Debug => debug;
    public Collider MyCollider => myCollider;

    private PlayerState _currentState;
    private Transform _t;

    //void Awake() {
    //    PlayerInit();
    //}

    void Start() {
        _t = transform;
        PlayerInit();       // TODO: not good here
        // begin the game in default state
        SwitchState(new DefaultState(this, input));
    }

    private void PlayerInit() {
        GameManager.OnLoadNextSnapshot += SavePlayerData;

        GameManager.Player = this;
        if (GameManager.hasStoredData == true) {
            _t.position = GameManager.playerCoords;
            _t.rotation = GameManager.playerRotation;
        }
    }

    // TODO: check this later
    public void SwitchState(PlayerState newState) {
        _currentState?.Exit();  // cleanup old state
        _currentState = newState;
        _currentState.Enter();
    }

    void Update() {
        _currentState?.Update();
    }

    public void EnableCrosshair() => crosshair.SetActive(true);
    public void DisableCrosshair() => crosshair.SetActive(false);

    private void SavePlayerData() {
        Vector3 relativePos = room.InverseTransformPoint(_t.position);
        relativePos.y += 0.2f;      // TODO: jank asf. add a floor check or gravity

        Quaternion relativeRot = Quaternion.Inverse(room.rotation)
                                * _t.rotation;

        GameManager.StorePlayerPosition(relativePos, relativeRot);

        GameManager.OnLoadNextSnapshot -= SavePlayerData;
    }
}
