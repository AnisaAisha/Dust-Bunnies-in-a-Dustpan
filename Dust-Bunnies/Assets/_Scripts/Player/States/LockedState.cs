using UnityEngine;

/// <summary>
/// The player is not allowed to interact with anything
/// </summary>
public class LockedState : PlayerState {
    public LockedState(PlayerController player, InputReader input) : base(player, input) {
    }

    public override void Enter() {
        base.Enter();
        player.DisableCrosshair();
    }
}
