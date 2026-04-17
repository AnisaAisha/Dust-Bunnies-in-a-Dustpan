using UnityEngine;

public class CubePuzzleState : PlayerState {
    
    private CubePuzzleInteractable _interactableSelf;

    public CubePuzzleState(PlayerController p, InputReader i, CubePuzzleInteractable self) 
        : base(p, i) 
    { _interactableSelf = self; }
    
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
            _interactableSelf.InspectAndRotate(input.Rotate);
        else
            _interactableSelf.ClearMouseInput();
    }

    private void ExitInteract() {

        if (_interactableSelf.Busy) return;
        
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
