using UnityEngine;

/// <summary>
/// Want to name this base-object but i think carlos used that name for swappables lol
/// 
/// Base class for all interactable objects
/// </summary>
public class Interactable : MonoBehaviour
{
    // TODO: temp to make the system work again then revisit a better way to pass info
    public virtual void Interact(Vector3 holdPoint, Vector3 playerCam, float moveTime) { }

    public virtual void InteractEnd(float moveTime) { }

    /// <summary>
    /// The state to switch too if interacting with the object changes state
    /// </summary>
    public virtual PlayerState GetNextState(PlayerController p, InputReader i) {
        return null;
    }
}
