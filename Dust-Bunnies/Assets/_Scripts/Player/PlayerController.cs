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

    //void Awake() {
    //    PlayerInit();
    //}

    void Start() {
        PlayerInit();       // TODO: not good here
        // begin the game in default state
        SwitchState(new DefaultState(this, input));
    }

    private void PlayerInit() {
        GameManager.Player = this;
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


    // TODO: seperate this eventually into a scene loader class that sends an event to store these coords
}
