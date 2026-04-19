using UnityEngine;

public class TestingScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        DialogueManager.Instance.RunDialogue("hello test dialogue");
    }
}
