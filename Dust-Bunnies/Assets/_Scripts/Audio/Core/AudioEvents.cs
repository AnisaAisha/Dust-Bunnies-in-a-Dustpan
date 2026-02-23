using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/Audio Events")]
public class AudioEvents : ScriptableObject
{
    [Header("UI")]
    public EventReference pauseMenuClick;
    public EventReference pauseMenuOpen;
    public EventReference pauseMenuClose;
    public EventReference pageFlip;

    [Header("Player - Movement")]
    public EventReference playerFootstep;

    [Header("Music")]
    public EventReference musicZone1;
    public EventReference musicZone2;
    public EventReference musicZone3;

    [Header("Ambience")]
    public EventReference windowAmbience;
    public EventReference roomAmbience;

    [Header("Dialogue (optional placeholders for now)")]
    public EventReference dialogueNarrator;
    public EventReference dialogueCharacter;

    [Header("Snapshots (highly recommended for story-heavy)")]
    public EventReference snapDialogueDuck;
    public EventReference snapCutscene;
    public EventReference snapPause;

    [Header("Environment")]
    public EventReference doorOpen;
    public EventReference doorClose;
    public EventReference bookPickup;
    public EventReference bookDrop;
    public EventReference plasticTap;
    public EventReference plushySqueeze;
    public EventReference pencilPickup;
    public EventReference pencilDrop;
}
