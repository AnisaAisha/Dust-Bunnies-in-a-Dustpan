using PrimeTween;
using System.Collections;
using UnityEngine;

/// <summary>
/// Base Object for everything that can be interacted with.
/// </summary>
public class PickupAble : Interactable
{
    private Vector3 _startPos;  // store initial start pos to return back to
    private Quaternion _startRot;

    public Vector3 StartPos => _startPos;
    public Quaternion StartRot => _startRot;

    private Transform t;

    void Start() {
        _startPos = transform.position;
        _startRot = transform.rotation;
        t = transform;
    }

    public override void Interact(Vector3 holdPoint, Vector3 playerCam, float moveTime) {
        base.Interact(holdPoint, playerCam, moveTime);
        StartCoroutine(PickUp(holdPoint, playerCam, moveTime));
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

    private IEnumerator PickUp(Vector3 holdPoint, Vector3 playerCam, float moveTime) {
        // lets do this scuff first
        Tween.Position(t, holdPoint, moveTime, Ease.InSine);    // move to player hold point

        // calculate rotation to point towards player cam
        Quaternion rot = Quaternion.LookRotation(playerCam - t.position, Vector3.up);
        Tween.Rotation(t, rot, moveTime, Ease.InSine);

        yield return new WaitForSeconds(moveTime);
    }

    public virtual void PutDown(float moveTime) {
        Tween.Position(t, StartPos, moveTime, Ease.OutSine);

        //Quaternion rot = Quaternion.LookRotation()
        Tween.Rotation(t, StartRot, moveTime, Ease.OutSine);
    }
}
