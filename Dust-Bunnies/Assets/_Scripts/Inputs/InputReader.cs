using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// All Inputs routed here
/// </summary>
public class InputReader : MonoBehaviour
{
    private InputActionMaps _inputs;

    // CONTINUOUS INPUTS    ---
    public Vector2 Move => _inputs.Default.Move.ReadValue<Vector2>();
    public Vector2 Look =>  _inputs.Default.Look.ReadValue<Vector2>();
    public Vector2 Rotate => _inputs.Interact.Rotate.ReadValue<Vector2>();
    // sprint

    // EVENTS   ---
    public event System.Action OnSprintPerformed;
    public event System.Action OnInteractPerformed;
    public event System.Action OnNextSnapshotPerformed;

    public event System.Action OnInteractExit;
    public event System.Action OnRotatePerformed;
    public event System.Action OnRotateExit;
    public event System.Action<float> OnZoomPerformed;
    public event System.Action OnZoomReset;

    // OTHER    ---
    public InputActionMaps Maps => _inputs;       // TODO: bad practice
    private InputActionMap currentMap;

    void Awake() {
        _inputs = new InputActionMaps();
    }

    void OnEnable() {
        _inputs.Default.Sprint.performed += SprintPerformed;
        _inputs.Default.Interact.performed += InteractPerformed;
        _inputs.Default.NextSnapshot.performed += NextSnapshotPerformed;

        _inputs.Interact.Return.performed += InteractExitPerformed;
        _inputs.Interact.StartRotate.performed += RotatePerformed;
        _inputs.Interact.StartRotate.canceled += RotatePerformed;
        _inputs.Interact.Zoom.performed += ZoomPerformed;
        _inputs.Interact.ResetZoom.performed += ZoomReset;

        // DEBUG
        //_inputs.DEBUG.NextScene.performed += NextScenePerformed;
    }

    void OnDisable() {
        _inputs.Default.Sprint.performed -= SprintPerformed;
        _inputs.Default.Interact.performed -= InteractPerformed;
        _inputs.Default.NextSnapshot.performed -= NextSnapshotPerformed;

        _inputs.Interact.Return.performed -= InteractExitPerformed;
        _inputs.Interact.StartRotate.performed -= RotatePerformed;
        _inputs.Interact.StartRotate.canceled -= RotatePerformed;
        _inputs.Interact.Zoom.performed -= ZoomPerformed;
        _inputs.Interact.ResetZoom.performed -= ZoomReset;

        // DEBUG
        //_inputs.DEBUG.NextScene.performed -= NextScenePerformed;

        _inputs.Dispose();
    }
    //for pause menu to disable/enable locked inputs
    public void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            _inputs.Default.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            _inputs.Default.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void SwitchMaps(InputActionMap newMap) {
        // DEBUG --
        if (newMap.name == "Default") {
            Maps.DEBUG.Enable();
        } else {
            Maps.DEBUG.Disable();
        }
            
        currentMap?.Disable();
        currentMap = newMap;
        currentMap.Enable();
    }

    private void SprintPerformed(InputAction.CallbackContext _) =>
        OnSprintPerformed?.Invoke();

    private void InteractPerformed(InputAction.CallbackContext _) => 
        OnInteractPerformed?.Invoke();

    private void NextSnapshotPerformed(InputAction.CallbackContext _) =>
        OnNextSnapshotPerformed?.Invoke();

    private void InteractExitPerformed(InputAction.CallbackContext _) =>
        OnInteractExit?.Invoke();

    private void RotatePerformed(InputAction.CallbackContext context) {
        if (context.performed) OnRotatePerformed?.Invoke();
        else if (context.canceled) OnRotateExit?.Invoke();
    }

    private void ZoomPerformed(InputAction.CallbackContext context) =>
        OnZoomPerformed?.Invoke(context.ReadValue<float>());

    private void ZoomReset(InputAction.CallbackContext _) =>
        OnZoomReset?.Invoke();
}
