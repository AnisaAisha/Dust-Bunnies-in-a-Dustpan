using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Player Motor
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AnimationCurve accelerationCurve;

    [Header("Movement Params")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxAcceleration = 1f;

    [Header("Audio")]
    [SerializeField] float stepInterval = 0.1f;
    
    float stepTimer;
    private float time;     // for walking accel

    public void Move(Vector2 inputVector)
    {
        // TODO: movement accel & deccel

        // TODO: jank a$$ way to keep track of time x__x
        if (inputVector == Vector2.zero) {
            time = 0;
            return;
        }
        time += Time.deltaTime;

        // Get movement direction
        Vector3 moveDir = new(inputVector.x, 0, inputVector.y);
        moveDir = transform.TransformDirection(moveDir);
        moveDir.Normalize();

        // Get speed + acceleration
        float targetAccel = maxAcceleration * accelerationCurve.Evaluate(time);
        float accel = Mathf.Clamp(targetAccel, 0, maxAcceleration); // should prolly max speed

        characterController.SimpleMove(moveDir * (speed + accel));

        // TODO: update this SFX system
        if (inputVector.magnitude > 0 && SceneManager.GetActiveScene().buildIndex != 2) { SetMovementState(true); }
        else { SetMovementState(false); }   // TODO: bad
    }


    public void SetMovementState(bool moving)   // TODO: Implement this into new system
    {
        if (!moving)
        {
            stepTimer = 0;
            return;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0;
            SFXManager.PlaySFX(SFXManager.Events.Footstep);
        }
    }
}
