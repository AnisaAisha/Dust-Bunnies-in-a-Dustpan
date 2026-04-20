using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    [SerializeField] Canvas canvas;
    [SerializeField] TextMeshProUGUI textbox;
    [SerializeField] float typespeed = 0.1f;
    [SerializeField] float waittime = 5.0f;

    public static DialogueManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Debug.Log("Dialogue manager instance created");
        }
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    public void RunDialogue(string text)
    {
        if (text == null)
        {
            return;
        }
        StopAllCoroutines();
        canvas.gameObject.SetActive(true);
        textbox.text = "";
        StartCoroutine(TypeLine(text));
    }
    IEnumerator TypeLine(string line)
    {
        foreach (char c in line.ToCharArray())
        {
            textbox.text += c;
            yield return new WaitForSeconds(typespeed);
        }
        yield return new WaitForSeconds(waittime);
        EndDialogue();
    }
    public void EndDialogue()
    {
        canvas.gameObject.SetActive(false);
    }
}
