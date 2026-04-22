using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Mirror : Interactable
{
    [SerializeField] private SceneFader sceneFader;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Camera playerCam;
    [SerializeField] private Camera mirrorCam;
    [SerializeField] private Image reflection;

    void Start() {
        reflection.enabled = false;
        mirrorCam.enabled = false;
    }

    public override void Interact(Transform playerCam, float moveTime) {
        base.Interact(playerCam, moveTime);
        StartCoroutine(CiciAppears());
    }

    public override PlayerState GetNextState(PlayerController p, InputReader i) {
        base.GetNextState(p, i);
        return new LockedState(p, i);
    }

    /// <summary>
    /// Sequence for Cici appearing in the mirror, and
    /// the first snapshot being loaded.
    /// </summary>
    private IEnumerator CiciAppears() {
        // start by fading out
        yield return StartCoroutine(sceneFader.FadeOut());

        yield return new WaitForSeconds(0.5f);

        playerCam.enabled = false;  // switch cams
        mirrorCam.enabled = true;
        reflection.enabled = true;  // load image of cici reflection

        StartCoroutine(sceneFader.FadeIn());

        yield return new WaitForSeconds(1f);
        dialogueManager.RunDialogue("Alright, let's get to work.");
        yield return new WaitForSeconds(4f);

        sceneFader.FadeTo(1);   // TODO: x__x
    }
}
