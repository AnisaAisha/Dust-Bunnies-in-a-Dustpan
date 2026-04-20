using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TitleScreenScript : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] List<Button> playButtons;
    [SerializeField] List<TextMeshProUGUI> saveLabels;
    [SerializeField] List<Button> deleteButtons;
    [SerializeField] Canvas loadCanvas;
    [SerializeField] Canvas mainCanvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadCanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PressStart()
    {
        SaveSystem.Preload();
        mainCanvas.gameObject.SetActive(false);
        loadCanvas.gameObject.SetActive(true);
        //# play/delete buttons = SaveSystem.numSaves
        for (int i = 0; i < playButtons.Count; i++)
        {
            if (SaveSystem.saves[i] != null)
            {
                saveLabels[i].text = SaveSystem.saves[i];
                deleteButtons[i].enabled = true;
            }
            else
            {
                deleteButtons[i].enabled = false;
            }
        }
    }
    public void PressPlay(int id)
    {
        SaveSystem.Play(id);
        PressStart(); //only runs if scene change doesn't happen, for debugging
        //this will also call scene change from there
    }
    public void PressDelete(int id)
    {
        SaveSystem.ClearSaveData(id);
        deleteButtons[id].enabled = false;
        saveLabels[id].text = "New Save";
    }
    public void Back()
    {
        loadCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
    }
}
