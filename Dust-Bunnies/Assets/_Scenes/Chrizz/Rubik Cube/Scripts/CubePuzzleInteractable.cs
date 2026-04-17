using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubePuzzleInteractable : Interactable {

    [Header("Puzzle Control")] 
    [SerializeField] private CubePuzzleBuilder puzzleBuilder;
    [SerializeField] private float inspectSpeed = 1f;
    [SerializeField] private float minDragDist = 50f;
    
    [Header("Animation")]
    [SerializeField] private float turnDuration = 0.25f;
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Scramble")]
    [SerializeField] private int scrambleMoves = 20;
    [SerializeField] public float scrambleDelay = 0.05f; 
    
    #region Interactables
    private Vector3 _startPos;
    private Quaternion _startRot;
    private Collider _collider;

    public Vector3 StartPos => _startPos;
    public Quaternion StartRot => _startRot;

    private Transform t;
    
    #endregion Interactables
    
    #region Puzzle
    private Vector2 _dragStart;
    private Vector2 _dragEnd;
    
    private CubePiece _activePiece = null;
    private Vector3 _activeNormal = Vector3.zero;

    private Vector3 _activeRotationAxis = Vector3.zero;
    private GameObject _activePivot;

    private bool _inRotation = false;
    private bool? _inspectedOnCube = null;
    private bool _hasScrambled = false;
    
    public bool Busy { get; private set; } = false;
    #endregion Puzzle
    
    #region Events
    public delegate void InteractAction(bool enabled);
    public event InteractAction OnInteractAction;

    public delegate void Solved();
    public event Solved OnSolved;
    #endregion Events
    
    void Start() {
        _startPos = transform.position;
        _startRot = transform.rotation;
        _collider = GetComponent<Collider>();
        t = transform;
    }
    
    #region Base Interaction
    public override void Interact(Transform playerCam, float moveTime) {
        base.Interact(playerCam, moveTime);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        puzzleBuilder.ToggleCollisions(false);
        
        StartCoroutine(PickUp(playerCam, moveTime));
    }

    public override void InteractEnd(float moveTime) {
        base.InteractEnd(moveTime);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        puzzleBuilder.ToggleCollisions(true);
        
        PutDown(moveTime);
    }

    public override PlayerState GetNextState(PlayerController p, InputReader i) {
        base.GetNextState(p, i);
        return new CubePuzzleState(p, i, this);
    }

    private IEnumerator PickUp(Transform playerCam, float moveTime) {
        float holdDistance = 1 + _collider.bounds.size.y;

        Vector3 holdPoint = playerCam.position + playerCam.forward * holdDistance;
        Tween.Position(t, holdPoint, moveTime, Ease.InSine);

        Quaternion rot = Quaternion.LookRotation(playerCam.position - t.position, Vector3.up);
        Tween.Rotation(t, rot, moveTime, Ease.InSine);

        yield return new WaitForSeconds(moveTime);
        
        //Scramble cube here
        if (!_hasScrambled) {
            _hasScrambled = true;
            StartCoroutine(ScrambleCube(scrambleMoves));
        }
        else {
            OnInteractAction?.Invoke(true);
        }
    }

    public virtual void PutDown(float moveTime) {
        Tween.Position(t, StartPos, moveTime, Ease.OutSine);
        Tween.Rotation(t, StartRot, moveTime, Ease.OutSine);
        OnInteractAction?.Invoke(false);
    }
    #endregion Base Interaction
    
    #region Puzzle Interaction

    public void InspectAndRotate(Vector2 rot) {

        if (_inspectedOnCube == null) {
            _inspectedOnCube = ValidateMouseOver();

            //Player clicked over Rubik?
            if (_inspectedOnCube == true) {
                
                HandleDragProjection();
                
                return;
            }
        }
        
        if (_inspectedOnCube == true) {
            return;
        }

        if (Busy) return;
        
        t.Rotate(Camera.main.transform.up ,-rot.x * inspectSpeed, Space.World);
        t.Rotate(Camera.main.transform.right, rot.y * inspectSpeed, Space.World);
    }

    public void ClearMouseInput() {
        //Player released rubik
        if (_inspectedOnCube == true) {
            _dragEnd = Input.mousePosition;
            DetectDirection();
        }
        
        _inspectedOnCube = null;
    }
    
    private bool ValidateMouseOver() 
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            return hit.collider.gameObject.GetComponent<CubePiece>() != null;
        }

        return false;
    }
    #endregion Puzzle Interaction
    
    #region Puzzle Core
    private IEnumerator ScrambleCube(int moves) {
        Busy = true;
        
        turnDuration *= 0.5f;
        
        yield return null; // wait one frame so everything initializes

        for (int i = 0; i < moves; i++)
        {
            yield return new WaitUntil(() => !_inRotation);

            int dimension = puzzleBuilder.Dimension;
            
            _activePiece = puzzleBuilder.CubeConstruct[
                Random.Range(0, dimension),
                Random.Range(0, dimension),
                Random.Range(0, dimension)
            ].GetComponent<CubePiece>();

            Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
            Vector3 axis = axes[Random.Range(0, axes.Length)];

            float dir = Random.value > 0.5f ? 1f : -1f;

            _activeRotationAxis =
                transform.TransformDirection(axis) * dir;

            _activeNormal = _activeRotationAxis.normalized;

            InitiateRotate();

            if (scrambleDelay > 0f)
                yield return new WaitForSeconds(scrambleDelay);
        }

        turnDuration *= 2f;

        yield return new WaitForSeconds(turnDuration);
        OnInteractAction?.Invoke(true);
        
        Busy = false;
    }

    private void HandleDragProjection() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.GetComponent<CubePiece>() != null)
            {
                _dragStart = Input.mousePosition;
                _activePiece = hit.transform.GetComponent<CubePiece>();
                _activeNormal = hit.normal;
                _activePiece = hit.transform.GetComponent<CubePiece>();
            }
        }
    }
    
    private void DetectDirection()
    {
        if (_inRotation) return;

        Vector2 drag = _dragEnd - _dragStart;
        if (drag.magnitude < minDragDist) return;

        Camera cam = Camera.main;

        Vector3 worldDrag =
            cam.transform.right * drag.x +
            cam.transform.up * drag.y;
        worldDrag.Normalize();

        Vector3 planeDrag = Vector3.ProjectOnPlane(worldDrag, _activeNormal);
        if (planeDrag.sqrMagnitude < 0.001f) return;
        planeDrag.Normalize();

        Vector3 tangent = Vector3.Cross(_activeNormal, cam.transform.forward);
        if (tangent.sqrMagnitude < 0.001f)
            tangent = Vector3.Cross(_activeNormal, cam.transform.up);
        tangent.Normalize();

        Vector3 bitangent = Vector3.Cross(_activeNormal, tangent);

        float t = Vector3.Dot(planeDrag, tangent);
        float b = Vector3.Dot(planeDrag, bitangent);

        Vector3 planeAxis =
            Mathf.Abs(t) > Mathf.Abs(b)
                ? Mathf.Sign(t) * tangent
                : Mathf.Sign(b) * bitangent;

        _activeRotationAxis = -Vector3.Cross(planeAxis, _activeNormal).normalized;
        _activeRotationAxis = SnapToNearestTransformAxis(_activeRotationAxis);
        
        //Start rotate if the rotation axis doesn't explode
        InitiateRotate();
    }
    
    //Helper to snap rotation axis to an angle orthonormal to the cube basis
    // ...even if the drag vector isn't 100% accurate
    private Vector3 SnapToNearestTransformAxis(Vector3 axis) 
    {
        axis.Normalize();

        Vector3[] candidates =
        {
            transform.right,
            -transform.right,
            transform.up,
            -transform.up,
            transform.forward,
            -transform.forward
        };

        Vector3 bestAxis = candidates[0];
        float bestDot = Vector3.Dot(axis, bestAxis);

        for (int i = 1; i < candidates.Length; i++)
        {
            float dot = Vector3.Dot(axis, candidates[i]);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestAxis = candidates[i];
            }
        }

        return bestAxis;
    }
    
    private void InitiateRotate() {
        Busy = true;
        if (_activePivot != null)
            Destroy(_activePivot);

        _inRotation = true;

        _activePivot = new GameObject("Pivot");
        _activePivot.transform.position = transform.position;

        List<GameObject> slice = GetSlice();

        foreach (GameObject cube in slice)
            cube.transform.SetParent(_activePivot.transform, true);

        StartCoroutine(RotateSlice(
            _activePivot,
            _activeRotationAxis,
            90f,
            turnDuration
        ));
    }
    
    private IEnumerator RotateSlice (
        GameObject pivot,
        Vector3 axis,
        float angle,
        float duration
    )
    {
        Quaternion startRotation = pivot.transform.rotation;
        Quaternion endRotation =
            Quaternion.AngleAxis(angle, axis) * startRotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float curvedT = rotationCurve.Evaluate(t);

            pivot.transform.rotation =
                Quaternion.Slerp(startRotation, endRotation, curvedT);

            yield return null;
        }

        pivot.transform.rotation = endRotation;

        FinalizeRotation(pivot);
    }
    
    private void FinalizeRotation(GameObject pivot)
    {
        while (pivot.transform.childCount > 0)
        {
            Transform child = pivot.transform.GetChild(0);
            child.SetParent(transform, true);

            SnapToGrid(child);
        }

        Destroy(pivot);
        _activePivot = null;
        _inRotation = false;
        Busy = false;

        if (VerifySolution()) {
            OnSolved.Invoke();
        }
    }

    private void SnapToGrid(Transform cube)
    {
        Vector3 localPos = transform.InverseTransformPoint(cube.position);

        float spacing = puzzleBuilder.Spacing;
        
        localPos.x = Mathf.Round(localPos.x / spacing) * spacing;
        localPos.y = Mathf.Round(localPos.y / spacing) * spacing;
        localPos.z = Mathf.Round(localPos.z / spacing) * spacing;

        cube.position = transform.TransformPoint(localPos);

        Vector3 euler = cube.localEulerAngles;
        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;
        cube.localEulerAngles = euler;
    }
    
    private List<GameObject> GetSlice()
    {
        float spacing = puzzleBuilder.Spacing;
        
        Vector3 localRotationAxis =
            transform.InverseTransformDirection(_activeRotationAxis).normalized;

        Vector3Int sliceAxis;

        if (Mathf.Abs(localRotationAxis.x) > Mathf.Abs(localRotationAxis.y) &&
            Mathf.Abs(localRotationAxis.x) > Mathf.Abs(localRotationAxis.z))
        {
            sliceAxis = Vector3Int.right; // YZ plane
        }
        else if (Mathf.Abs(localRotationAxis.y) > Mathf.Abs(localRotationAxis.z))
        {
            sliceAxis = Vector3Int.up;    // XZ plane
        }
        else
        {
            sliceAxis = Vector3Int.forward; // XY plane
        }

        Vector3 localPos =
            transform.InverseTransformPoint(_activePiece.transform.position);

        float layerValue;

        if (sliceAxis == Vector3Int.right)
            layerValue = localPos.x;
        else if (sliceAxis == Vector3Int.up)
            layerValue = localPos.y;
        else
            layerValue = localPos.z;

        List<GameObject> slice = new List<GameObject>();

        foreach (GameObject cube in puzzleBuilder.CubeConstruct)
        {
            Vector3 cubeLocalPos =
                transform.InverseTransformPoint(cube.transform.position);

            float cubeLayer;

            if (sliceAxis == Vector3Int.right)
                cubeLayer = cubeLocalPos.x;
            else if (sliceAxis == Vector3Int.up)
                cubeLayer = cubeLocalPos.y;
            else
                cubeLayer = cubeLocalPos.z;

            if (Mathf.Abs(cubeLayer - layerValue) < spacing * 0.5f)
            {
                slice.Add(cube);
            }
        }

        return slice;
    }
    #endregion Puzzle Core
    
    #region Verification

    private bool VerifySolution() {
        Transform reference = puzzleBuilder.CubeConstruct[0,0,0].transform;

        foreach (GameObject cubie in puzzleBuilder.CubeConstruct) {
            if (Quaternion.Angle(cubie.transform.localRotation, reference.localRotation) > 0.1f) {
                return false;
            }
        }
        return true;
    }
    #endregion Verification
}