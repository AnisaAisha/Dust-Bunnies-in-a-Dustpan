using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PuzzleConfigMode {
    Easy,
    Hard
}

public class CubePuzzleConfigManager : MonoBehaviour {
    [SerializeField] private Key configToggleKey;
    [SerializeField] private Transform easyPuzzle;
    [SerializeField] private Transform hardPuzzle;

    private PuzzleConfigMode _mode = PuzzleConfigMode.Easy;
    private bool _toggled = false;

    private void Update() {
        if (_toggled) return;
        
        if (Keyboard.current != null && Keyboard.current[configToggleKey].wasPressedThisFrame) {
            _toggled = true;
            SetMode();
        }
    }

    private void SetMode() {

        _mode = _mode == PuzzleConfigMode.Easy ? PuzzleConfigMode.Hard : PuzzleConfigMode.Easy;
        
        switch (_mode) {
            case PuzzleConfigMode.Easy:
                easyPuzzle.gameObject.SetActive(true);
                hardPuzzle.gameObject.SetActive(false);
                break;
            case PuzzleConfigMode.Hard:
                easyPuzzle.gameObject.SetActive(false);
                hardPuzzle.gameObject.SetActive(true);
                break;
        }
    }
}
