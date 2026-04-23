using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;

public class SnapshotUIScript : MonoBehaviour
{
    [SerializeField] Image forward;
    [SerializeField] Image backward;
    [SerializeField] Image vector;
    [SerializeField] TextMeshProUGUI seasontext;
    [SerializeField] TextMeshProUGUI agetext;
    [SerializeField] Image background;
    [SerializeField] Vector2 movevector; //starts positive
    [SerializeField] float speed = 3.0f;
    [SerializeField] List<string> seasons;
    [SerializeField] List<string> age;
    [SerializeField] List<TextMeshProUGUI> numbers;
    private List<Vector2> numberpos;
    private int snapshot = 0;
    private Boolean moving = false;
    private Boolean expanded = true;
    private Vector2 initialposbg;
    private int direction = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialposbg = background.rectTransform.anchoredPosition - (movevector/2);
        numberpos = new List<Vector2>();
        for (int i = 0; i < numbers.Count; i++)
        {
            numberpos.Add(numbers[i].rectTransform.anchoredPosition);
            Debug.Log(numberpos[i]);
        }
        Expand();


        // Edits - Jazz man
        GameManager.OnLoadNextSnapshot += AdvanceSnapshot;
        snapshot = GameManager.CurrentSnapshotNumber;
        UpdateSnapshotUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            if (Vector2.Distance(background.rectTransform.anchoredPosition, initialposbg + movevector) < 0.5)
            {
                moving = false;
                Debug.Log("stopped moving");
            }
            else
            {
                background.rectTransform.anchoredPosition = Vector2.MoveTowards(background.rectTransform.anchoredPosition, initialposbg + movevector, speed * Time.deltaTime);
                //snapshottext.rectTransform.anchoredPosition = Vector2.MoveTowards(snapshottext.rectTransform.anchoredPosition, initialpossnap + movevector, speed * Time.deltaTime);
            }
            if (expanded)
            {
                for (int i = 0; i < numbers.Count; i++)
                {
                    if (i+direction < numbers.Count && i+direction >= 0)
                    {
                        numbers[i].rectTransform.anchoredPosition = Vector2.MoveTowards(numbers[i].rectTransform.anchoredPosition, numberpos[i + direction], Time.deltaTime);
                    }
                }
            }
        }   
    }
    public void Expand()
    {
        //assumes UI is in a compressed state
        if (movevector.x < 0)
        {
            movevector = -movevector;
            moving = true;
            forward.enabled = true;
            backward.enabled = true;
            vector.enabled = true;
            seasontext.enabled = true;
            agetext.enabled = true;
            for (int i = 0; i < numbers.Count; i++)
            {
                numbers[i].enabled = true;
            }
            seasontext.text = seasons[snapshot];
            agetext.text = "" + age[snapshot];
            if (snapshot == 0)
            {
                backward.enabled = false;
            } else if (snapshot == seasons.Count - 1)
            {
                forward.enabled = false; 
            }
            StopCoroutine(WaitCompress());
            StartCoroutine(WaitCompress());
        }
        else
        {
            Debug.LogWarning("Cannot expand from expanded state");
            seasontext.text = seasons[snapshot];
            agetext.text = "" + age[snapshot];
            if (snapshot == 0)
            {
                backward.enabled = false;
            }
            else if (snapshot == seasons.Count - 1)
            {
                forward.enabled = false;
            }
            for (int i = 0; i < numbers.Count; i++)
            {
                numbers[i].text = "" + (snapshot - 2 + i);
                if (snapshot - 2 + i < 0)
                {
                    numbers[i].text = "";
                }
                else if (snapshot - 2 + i >= seasons.Count)
                {
                    numbers[i].text = "";
                }
            }
            StopCoroutine(WaitCompress());
            StartCoroutine(WaitCompress());
        }
        expanded = true;
        Debug.LogWarning("Expanding");
    }
    public void Compress()
    {
        if (movevector.x > 0){
            movevector = -movevector;
            moving = true;
            forward.enabled = false;
            backward.enabled = false;
            vector.enabled = false;
            seasontext.enabled = false;
            agetext.enabled = false;
            for (int i = 0; i < numbers.Count; i++)
            {
                if (i != 2)
                {
                    numbers[i].enabled = false;
                }
                numbers[i].rectTransform.anchoredPosition = numberpos[i];
            }
        }
        else
        {
            Debug.LogWarning("Cannot compress from compressed state");
        }
        expanded = false;
    }
    public void AdvanceSnapshot()
    {
        if (snapshot == seasons.Count - 1)
        {
            return;
        }
        snapshot++;
        for (int i = 0; i < numbers.Count; i++)
        {
            numbers[i].text = "" + (snapshot - 2 + i);
            if (snapshot - 2 + i < 0)
            {
                numbers[i].text = "";
            }
            else if (snapshot - 2 + i >= seasons.Count)
            {
                numbers[i].text = "";
            }
        }
        direction = 1;
        Expand();

        // EDITS: jazz man
        GameManager.OnLoadNextSnapshot -= AdvanceSnapshot;
    }
    public void DecrementSnapshot()
    {
        if (snapshot == 0)
        {
            return;
        }
        snapshot--;
        direction = -1;
        for (int i = 0; i < numbers.Count; i++)
        {
            numbers[i].text = "" + (snapshot - 2 + i);
            if (snapshot - 2 + i < 0)
            {
                numbers[i].text = "";
            }
            else if (snapshot - 2 + i >= seasons.Count)
            {
                numbers[i].text = "";
            }
        }
        Expand();
    }

    private void UpdateSnapshotUI() {
        seasontext.text = seasons[snapshot];
        agetext.text = "" + age[snapshot];

        // Update arrows
        backward.enabled = snapshot != 0;
        forward.enabled = snapshot != seasons.Count - 1;

        // Update number strip
        for (int i = 0; i < numbers.Count; i++) {
            int value = snapshot - 2 + i;

            if (value < 0 || value >= seasons.Count) {
                numbers[i].text = "";
            }
            else {
                numbers[i].text = value.ToString();
            }
        }
    }

    IEnumerator WaitCompress()
    {
        yield return new WaitForSeconds(2.5f);
        Compress();
    }
}
