using UnityEngine;

/// <summary>
/// Want to name this base-object but i think carlos used that name for swappables lol
/// 
/// Base class for all interactable objects
/// </summary>
[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    public string Dialogue = null;
    // TODO: temp to make the system work again then revisit a better way to pass info
    public virtual void Interact(Transform playerCam, float moveTime) {
        try {
            DialogueManager.Instance.RunDialogue(Dialogue);
        }
        catch {
            Debug.LogWarning("Dialogue manager instance does not exist");
        }
    }

    public virtual void InteractEnd(float moveTime) { }

    /// <summary>
    /// The state to switch too if interacting with the object changes state
    /// </summary>
    public virtual PlayerState GetNextState(PlayerController p, InputReader i) {
        return null;
    }
}
