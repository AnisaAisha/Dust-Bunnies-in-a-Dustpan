using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AudioEvents events;

    [Header("Music Switching")]
    [SerializeField] private SceneMusicTable sceneMusicTable;
    [SerializeField] private bool switchMusicOnSceneLoad = true;

    [Header("Ambience")]
    [SerializeField] private float ambienceCrossfadeSeconds = 1.0f;

    [Header("Dialogue")]
    [Tooltip("If empty, will try to use events.snapDialogueDuck.")]
    [SerializeField] private EventReference dialogueDuckSnapshot;

    private EventInstance musicInstance;
    private EventInstance ambienceInstance;
    private EventInstance dialogueInstance;

    private EventReference currentMusicEvent;
    private EventReference currentAmbienceEvent;

    // Snapshot stack (mix states)
    private readonly Stack<EventInstance> snapshotStack = new();

    // Dialogue queue
    public enum DialogueMode { Interrupt, Queue }

    private struct DialogueRequest
    {
        public EventReference evt;
        public DialogueMode mode;
        public bool duck;
        public Action onEnded;

        // Optional label param
        public bool useLabel;
        public string paramName;
        public string label;
    }

    private readonly Queue<DialogueRequest> dialogueQueue = new();
    private Coroutine dialogueWorker;

    // ----------------------------
    // Lifecycle
    // ----------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        if (events == null)
        {
            Debug.LogError("[AudioManager] AudioEvents is not assigned.");
            return;
        }

        // Default duck snapshot from AudioEvents if none assigned in inspector
        if (dialogueDuckSnapshot.IsNull && !events.snapDialogueDuck.IsNull)
            dialogueDuckSnapshot = events.snapDialogueDuck;

        // Optional: start a default ambience
        StartRoomAmbience();

        if (!switchMusicOnSceneLoad)
            StartMusicZone1();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        StopDialogue(true);
        StopMusic(true);
        StopAmbience(true);
        ClearSnapshots(true);

        Instance = null;
    }

    // ----------------------------
    // Scene-based music switching
    // ----------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!switchMusicOnSceneLoad) return;

        // MUSIC
        if (sceneMusicTable != null &&
            sceneMusicTable.TryGet(scene.name, out var musicEvent) &&
            !musicEvent.IsNull)
        {
            StartMusic(musicEvent);
        }
        else
        {
            StopMusic(false);
        }

        // Optional: stop ambience by default on scene load
        StopAmbience(false);
    }


    // ----------------------------
    // One-shots (generic)
    // ----------------------------
    public void PlayOneShot(EventReference evt)
    {
        if (evt.IsNull) return;
        RuntimeManager.PlayOneShot(evt);
    }

    public void PlayOneShot(EventReference evt, Vector3 position)
    {
        if (evt.IsNull) return;
        RuntimeManager.PlayOneShot(evt, position);
    }

    public void PlayOneShot(EventReference evt, string paramName, int paramValue)
    {
        if (evt.IsNull) return;
        var inst = RuntimeManager.CreateInstance(evt);
        inst.setParameterByName(paramName, paramValue);
        inst.start();
        inst.release();
    }

    public void PlayOneShot(EventReference evt, string paramName, string label)
    {
        if (evt.IsNull) return;
        var inst = RuntimeManager.CreateInstance(evt);
        inst.setParameterByNameWithLabel(paramName, label);
        inst.start();
        inst.release();
    }

    public EventInstance Play3DLoop(EventReference evt, Vector3 pos)
    {
        if (evt.IsNull) return default;

        var inst = RuntimeManager.CreateInstance(evt);
        inst.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        inst.start();
        return inst;
    }



    public void StopInstance(EventInstance inst, bool immediate = false)
    {
        if (!inst.isValid()) return;

        inst.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        inst.release();
        inst.clearHandle();
    }

    // ----------------------------
    // Global parameter helpers (optional)
    // ----------------------------
    public void SetGlobalParameter(string name, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(name, value);
    }

    public void SetGlobalParameterLabel(string name, string label)
    {
        RuntimeManager.StudioSystem.setParameterByNameWithLabel(name, label);
    }

    // ----------------------------
    // Snapshots (mix states)
    // ----------------------------
    public void PushSnapshot(EventReference snapshotEvent)
    {
        if (snapshotEvent.IsNull) return;

        var inst = RuntimeManager.CreateInstance(snapshotEvent);
        inst.start();
        snapshotStack.Push(inst);
    }

    public void PopSnapshot(bool immediate = false)
    {
        if (snapshotStack.Count == 0) return;

        var inst = snapshotStack.Pop();
        inst.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        inst.release();
        inst.clearHandle();
    }

    public void ClearSnapshots(bool immediate = false)
    {
        while (snapshotStack.Count > 0)
            PopSnapshot(immediate);
    }

    // Convenience snapshot wrappers (from AudioEvents)
    public void PushPauseSnapshot() { if (events != null) PushSnapshot(events.snapPause); }
    public void PushCutsceneSnapshot() { if (events != null) PushSnapshot(events.snapCutscene); }
    public void PushDialogueDuckSnapshot() { if (!dialogueDuckSnapshot.IsNull) PushSnapshot(dialogueDuckSnapshot); }

    // ----------------------------
    // Music
    // ----------------------------
    public void StartMusic(EventReference musicEvent)
    {
        if (musicEvent.IsNull) return;

        if (musicInstance.isValid() && currentMusicEvent.Guid == musicEvent.Guid)
            return;

        StopMusic(false);

        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();

        currentMusicEvent = musicEvent;
    }

    public void StopMusic(bool immediate = false)
    {
        if (!musicInstance.isValid()) return;

        musicInstance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();

        currentMusicEvent = default;
    }

    public void SetMusicParameter(string paramName, float value)
    {
        if (!musicInstance.isValid()) return;
        musicInstance.setParameterByName(paramName, value);
    }

    public void SetMusicParameterLabel(string paramName, string label)
    {
        if (!musicInstance.isValid()) return;
        musicInstance.setParameterByNameWithLabel(paramName, label);
    }

    // Convenience music wrappers
    public void StartMusicZone1() { if (events != null) StartMusic(events.musicZone1); }
    public void StartMusicZone2() { if (events != null) StartMusic(events.musicZone2); }
    public void StartMusicZone3() { if (events != null) StartMusic(events.musicZone3); }

    // ----------------------------
    // Ambience (crossfade + params)
    // ----------------------------
    public void StartOrSwitchAmbience(EventReference ambienceEvent)
    {
        if (ambienceEvent.IsNull) return;

        if (ambienceInstance.isValid() && currentAmbienceEvent.Guid == ambienceEvent.Guid)
            return;

        if (!ambienceInstance.isValid())
        {
            ambienceInstance = RuntimeManager.CreateInstance(ambienceEvent);
            ambienceInstance.start();
            currentAmbienceEvent = ambienceEvent;
            return;
        }

        StartCoroutine(CrossfadeAmbience(ambienceEvent, ambienceCrossfadeSeconds));
    }

    public void StopAmbience(bool immediate = false)
    {
        if (!ambienceInstance.isValid()) return;

        ambienceInstance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
        ambienceInstance.clearHandle();

        currentAmbienceEvent = default;
    }

    public void SetAmbienceParameter(string paramName, float value)
    {
        if (!ambienceInstance.isValid()) return;
        ambienceInstance.setParameterByName(paramName, value);
    }

    public void EnsureRoomAmbienceStarted()
    {
        if (ambienceInstance.isValid()) return;
        if (events == null) return;
        if (events.roomAmbience.IsNull) return;

        StartOrSwitchAmbience(events.roomAmbience);
    }



    private IEnumerator CrossfadeAmbience(EventReference newEvent, float seconds)
    {
        var oldInst = ambienceInstance;
        var newInst = RuntimeManager.CreateInstance(newEvent);
        newInst.setVolume(0f);
        newInst.start();

        float t = 0f;
        float oldVol = 1f;
        oldInst.getVolume(out oldVol);

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, seconds);
            float a = Mathf.Clamp01(t);

            oldInst.setVolume(Mathf.Lerp(oldVol, 0f, a));
            newInst.setVolume(Mathf.Lerp(0f, 1f, a));
            yield return null;
        }

        oldInst.stop(STOP_MODE.ALLOWFADEOUT);
        oldInst.release();
        oldInst.clearHandle();

        ambienceInstance = newInst;
        currentAmbienceEvent = newEvent;
    }

    // Convenience ambience wrappers
    public void StartRoomAmbience() { if (events != null) StartOrSwitchAmbience(events.roomAmbience); }

    // ----------------------------
    // Dialogue (tracked instance + queue/interrupt + optional duck)
    // ----------------------------
    public void PlayDialogue(EventReference dialogueEvent,
        DialogueMode mode = DialogueMode.Interrupt,
        bool duck = true,
        Action onEnded = null)
    {
        if (dialogueEvent.IsNull) return;

        EnqueueDialogue(new DialogueRequest
        {
            evt = dialogueEvent,
            mode = mode,
            duck = duck,
            onEnded = onEnded,
            useLabel = false
        });
    }

    public void PlayDialogueWithLabel(EventReference dialogueEvent,
        string paramName, string label,
        DialogueMode mode = DialogueMode.Interrupt,
        bool duck = true,
        Action onEnded = null)
    {
        if (dialogueEvent.IsNull) return;

        EnqueueDialogue(new DialogueRequest
        {
            evt = dialogueEvent,
            mode = mode,
            duck = duck,
            onEnded = onEnded,
            useLabel = true,
            paramName = paramName,
            label = label
        });
    }

    private void EnqueueDialogue(DialogueRequest req)
    {
        // If interrupt, stop current and clear queue
        if (req.mode == DialogueMode.Interrupt)
        {
            StopDialogue(false);
            dialogueQueue.Clear();
            dialogueQueue.Enqueue(req);
        }
        else
        {
            // Queue mode
            dialogueQueue.Enqueue(req);
        }

        if (dialogueWorker == null)
            dialogueWorker = StartCoroutine(DialogueWorker());
    }

    private IEnumerator DialogueWorker()
    {
        while (dialogueQueue.Count > 0)
        {
            // Wait until no dialogue is playing
            while (dialogueInstance.isValid() && IsPlaying(dialogueInstance))
                yield return null;

            // Cleanup any stopped instance
            CleanupDialogueInstance();

            var req = dialogueQueue.Dequeue();
            PlayDialogueInternal(req);

            yield return null; // let FMOD start
        }

        dialogueWorker = null;
    }

    private void PlayDialogueInternal(DialogueRequest req)
    {
        if (req.duck && !dialogueDuckSnapshot.IsNull)
            PushSnapshot(dialogueDuckSnapshot);

        dialogueInstance = RuntimeManager.CreateInstance(req.evt);

        if (req.useLabel && !string.IsNullOrEmpty(req.paramName))
            dialogueInstance.setParameterByNameWithLabel(req.paramName, req.label);

        dialogueInstance.start();

        // Monitor end and handle duck + callback
        StartCoroutine(WaitForDialogueEnd(req.onEnded, req.duck));
    }

    private IEnumerator WaitForDialogueEnd(Action onEnded, bool ducked)
    {
        while (dialogueInstance.isValid() && IsPlaying(dialogueInstance))
            yield return null;

        CleanupDialogueInstance();

        if (ducked && !dialogueDuckSnapshot.IsNull)
            PopSnapshot(false);

        onEnded?.Invoke();
    }

    public void StopDialogue(bool immediate = false)
    {
        if (!dialogueInstance.isValid()) return;

        dialogueInstance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        CleanupDialogueInstance();

        // If you interrupted, you probably want to remove the duck snapshot.
        // This pops one snapshot; assumes duck snapshot is top-most when used.
        if (!dialogueDuckSnapshot.IsNull)
            PopSnapshot(false);
    }

    private void CleanupDialogueInstance()
    {
        if (!dialogueInstance.isValid()) return;

        dialogueInstance.release();
        dialogueInstance.clearHandle();
    }

    private bool IsPlaying(EventInstance inst)
    {
        if (!inst.isValid()) return false;

        inst.getPlaybackState(out PLAYBACK_STATE state);
        return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING;
    }

    // ----------------------------
    // Convenience wrappers for your AudioEvents
    // ----------------------------

    // UI
    public void PauseMenuOpen() => PlayOneShot(events.pauseMenuOpen);
    public void PauseMenuClose() => PlayOneShot(events.pauseMenuClose);
    public void PauseMenuClick() => PlayOneShot(events.pauseMenuClick);
    public void PageFlip() => PlayOneShot(events.pageFlip);

    // Player
    public void PlayerFootstep(Vector3 pos) => PlayOneShot(events.playerFootstep, pos);

    // Environment
    public void DoorOpen(Vector3 pos) => PlayOneShot(events.doorOpen, pos);
    public void DoorClose(Vector3 pos) => PlayOneShot(events.doorClose, pos);
    public void BookPickup(Vector3 pos) => PlayOneShot(events.bookPickup, pos);
    public void BookDrop(Vector3 pos) => PlayOneShot(events.bookDrop, pos);
    public void PlasticTap(Vector3 pos) => PlayOneShot(events.plasticTap, pos);
    public void PlushySqueeze(Vector3 pos) => PlayOneShot(events.plushySqueeze, pos);
    public void PencilPickup(Vector3 pos) => PlayOneShot(events.pencilPickup, pos);
    public void PencilDrop(Vector3 pos) => PlayOneShot(events.pencilDrop, pos);
    public EventInstance StartWindowAmbience(Vector3 pos) => Play3DLoop(events.windowAmbience, pos);

}
