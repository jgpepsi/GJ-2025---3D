using System.Collections;
using UnityEngine;

public class CameraZoomEffect : MonoBehaviour
{
    
    [Header("Configuração Padrão")]
    public float zoomInZ = -5f;         // Posição Z para o zoom in
    public float zoomOutZ = -10f;       // Posição Z para o zoom out (posição normal)
    public float zoomInSpeed = 10f;     // Velocidade do zoom in
    public float zoomOutSpeed = 5f;     // Velocidade do zoom out
    public float zoomDuration = 0.5f;   // Quanto tempo permanece no zoom in

    private Coroutine zoomRoutine;
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
        if (Mathf.Abs(originalPos.z - zoomOutZ) > 0.01f)
            originalPos.z = zoomOutZ;
    }

    public void ZoomInAndOut(float? customZoomInZ = null, float? customZoomOutZ = null, float? customInSpeed = null, float? customOutSpeed = null, float? customDuration = null)
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        float targetInZ = customZoomInZ ?? zoomInZ;
        float targetOutZ = customZoomOutZ ?? zoomOutZ;
        float inSpeed = customInSpeed ?? zoomInSpeed;
        float outSpeed = customOutSpeed ?? zoomOutSpeed;
        float duration = customDuration ?? zoomDuration;

        zoomRoutine = StartCoroutine(ZoomCoroutine(targetInZ, targetOutZ, inSpeed, outSpeed, duration));
    }

    private IEnumerator ZoomCoroutine(float targetInZ, float targetOutZ, float inSpeed, float outSpeed, float duration)
    {
        Vector3 start = transform.localPosition;
        Vector3 zoomInTarget = new Vector3(start.x, start.y, targetInZ);

        // Zoom In
        while (Mathf.Abs(transform.localPosition.z - targetInZ) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, zoomInTarget, inSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        transform.localPosition = zoomInTarget;

        // Espera no zoom in
        yield return new WaitForSecondsRealtime(duration);

        // Zoom Out
        Vector3 zoomOutTarget = new Vector3(start.x, start.y, targetOutZ);
        while (Mathf.Abs(transform.localPosition.z - targetOutZ) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, zoomOutTarget, outSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        transform.localPosition = zoomOutTarget;
    }
}
