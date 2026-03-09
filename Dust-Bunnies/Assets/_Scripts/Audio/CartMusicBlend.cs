using UnityEngine;

public class CartMusicBlend : MonoBehaviour
{
    [Header("FMOD Parameter")]
    [SerializeField] private string parameterName = "Proximity";
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float maxValue = 1f;
    [SerializeField] private bool useGlobalParameter = false;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform cart;

    private void Update()
    {
        if (player == null || cart == null) return;

        var am = AudioManager.Instance;
        if (am == null || string.IsNullOrWhiteSpace(parameterName)) return;

        float distance = Vector3.Distance(player.position, cart.position);
        float safeMaxDistance = Mathf.Max(0.01f, maxDistance);
        float blendValue = Mathf.Clamp01(1f - (distance / safeMaxDistance)) * maxValue;

        if (useGlobalParameter)
            am.SetGlobalParameter(parameterName, blendValue);
        else
            am.SetAmbienceParameter(parameterName, blendValue);
    }
}
