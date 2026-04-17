using PrimeTween;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Base Object for everything that can be interacted with.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PickupAble : Interactable
{
    private Vector3 _startPos;  // store initial start pos to return back to
    private Quaternion _startRot;
    private Collider _collider;

    public Vector3 StartPos => _startPos;
    public Quaternion StartRot => _startRot;

    private Transform t;

    void Start() {
        _startPos = transform.position;
        _startRot = transform.rotation;
        _collider = this.GetComponent<Collider>();
        t = transform;
    }

    public override void Interact(Transform playerCam, float moveTime) {
        base.Interact(playerCam, moveTime);
        StartCoroutine(PickUp(playerCam, moveTime));
    }

    public override void InteractEnd(float moveTime) {
        base.InteractEnd(moveTime);
        PutDown(moveTime);
    }

    public override PlayerState GetNextState(PlayerController p, InputReader i) {
        base.GetNextState(p, i);
        return new PickUpState(p, i);
    }

    //public virtual void OnPickUp(Vector3 holdPoint, Vector3 playerCam, float moveTime) {
    //    StartCoroutine(PickUp(holdPoint, playerCam, moveTime));
    //}

    private IEnumerator PickUp(Transform playerCam, float moveTime) {
        //Debug.Log("Collider size: " + _collider.bounds.size);

        //Vector3 testHoldPoint = holdPoint - new Vector3(0, 0, _collider.bounds.size.y);

        // TODO: object based on front surface rather than the object center
        float holdDistance = 1 + _collider.bounds.size.y;   //

        Vector3 holdPoint = playerCam.position + playerCam.forward * holdDistance;
        holdPoint += -playerCam.up * _collider.bounds.extents.y;

        // lets do this scuff first
        Tween.Position(t, holdPoint, moveTime, Ease.InSine);    // move to player hold point

        // calculate rotation to point towards player cam
        Quaternion rot = Quaternion.LookRotation(t.position - playerCam.position, Vector3.up);
        Tween.Rotation(t, rot, moveTime, Ease.InSine);

        yield return new WaitForSeconds(moveTime);
    }

    public virtual void PutDown(float moveTime) {
        Tween.Position(t, StartPos, moveTime, Ease.OutSine);

        //Quaternion rot = Quaternion.LookRotation()
        Tween.Rotation(t, StartRot, moveTime, Ease.OutSine);
    }
}
