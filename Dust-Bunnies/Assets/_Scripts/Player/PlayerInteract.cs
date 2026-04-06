using UnityEngine;
using PrimeTween;
using System.Collections;

/// <summary>
/// Handles player interaction with Interactables (objects)
/// </summary>
public class PlayerInteract : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform holdPoint;

    [Header("Settings")]
    [SerializeField] private float moveTime = 1f;        // how long it takes to hold an object
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float pickUpDistance = 10f;

    private Interactable _interactedObject;   // what was prev interacted with
                                            // TODO: this is a p sh*t way of 

    private bool _isPickingUp = false;
    public bool IsPickingUp => _isPickingUp;

    /// <summary>
    /// Called when the player hits the interact button
    /// </summary>
    public Interactable TryInteract(Collider collider) {
        // shoot ray from the center of camera
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        collider.enabled = false;       // disable collider on self when querying hit object
        bool hitObject = Physics.Raycast(ray, out RaycastHit hit, pickUpDistance);
        collider.enabled = true;

        if (hitObject) {
            if (hit.transform.TryGetComponent(out Interactable interactable)) {
                Interactable obj = interactable;
                _interactedObject = obj;

                // TODO: TEST interact here
                if (obj != null) {
                    _interactedObject.Interact(playerCam, moveTime);
                }

                return obj;
            }
        }
        return null;
    }

    //public PickupAble TryPickUp() {

    //    oi.ChangeState();
    //    return null;
    //}

    /// <summary>
    /// First time you interact the player picks up the obj
    /// </summary>
    public void PickUpObj() {
        if (_interactedObject == null) { Debug.Log("No object to pick up??"); return; }

        _isPickingUp = true;        // TODO: replace
        //_interactedObject.Interact(holdPoint.position, playerCam.position, moveTime);
        _isPickingUp = false;
        // TODO: make sure you cant rotate while moving
        // make sure to set isPicking up to false after
    }

    public void PutDownObj() {
        if (_interactedObject == null) { Debug.Log("Not holding an object???"); return; }

        _interactedObject.InteractEnd(moveTime);
    }

    public void Rotate(Vector2 rot) {
        Transform t = _interactedObject.transform;
        t.Rotate(playerCam.up ,-rot.x * rotationSpeed, Space.World);
        t.Rotate(playerCam.right, - rot.y * rotationSpeed, Space.World);
    }
}
