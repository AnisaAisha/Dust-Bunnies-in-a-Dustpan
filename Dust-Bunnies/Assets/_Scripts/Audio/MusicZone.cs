using UnityEngine;
using Cinemachine;

public class MusicZone : MonoBehaviour
{
    [SerializeField] private CinemachinePathBase path;
    [SerializeField] private Transform player;
    [SerializeField] private CinemachinePathBase.PositionUnits units = CinemachinePathBase.PositionUnits.PathUnits;

    private float positionOnPath;

    private void Update()
    {
        if (path == null || player == null) return;

        float closest = path.FindClosestPoint(player.position, 0, -1, 10);
        positionOnPath = path.StandardizeUnit(closest, units);

        transform.position = path.EvaluatePositionAtUnit(positionOnPath, units);
        transform.rotation = path.EvaluateOrientationAtUnit(positionOnPath, units);
    }
}
