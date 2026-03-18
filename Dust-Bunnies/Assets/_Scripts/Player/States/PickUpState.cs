using UnityEngine;

/// <summary>
/// When the player picks up an object
/// </summary>
public class PickUpState : PlayerState
{
    public PickUpState(PlayerController p, InputReader i) : base(p, i) { }

    private bool _isRotating = false;

    public override void Enter() {
        base.Enter();

        input.SwitchMaps(input.Maps.Interact);
        input.OnInteractExit += ExitInteract;
        input.OnRotatePerformed += StartRotate;
        input.OnRotateExit += ExitRotate;
        input.OnZoomPerformed += Zoom;
        input.OnZoomReset += ZoomReset;

        player.Interact.PickUpObj();
    }

    public override void Exit() {
        input.OnInteractExit -= ExitInteract;
        input.OnRotatePerformed -= StartRotate;
        input.OnRotateExit -= ExitRotate;
        input.OnZoomPerformed -= Zoom;
        input.OnZoomReset -= ZoomReset;

        base.Exit();
    }

    public override void Update() {
        base.Update();

        if (_isRotating && !player.Interact.IsPickingUp)
            player.Interact.Rotate(input.Rotate);
    }

    private void ExitInteract() {
        player.Interact.PutDownObj();
        player.SwitchState(new DefaultState(player, input));
    }

    // TODO: inoptimal fix later
    private void StartRotate() { _isRotating = true; }
    private void ExitRotate() { _isRotating = false; }

    private void Zoom(float scroll) { 
        if (!player.Interact.IsPickingUp)   player.Camera.Zoom(scroll); }
    private void ZoomReset() { player.Camera.ZoomReset(); }
}
