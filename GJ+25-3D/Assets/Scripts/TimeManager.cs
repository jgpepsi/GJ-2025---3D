using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager TimeInstance { get; private set; }

    [Header("Slow Motion")]
    public float slowTimeScale;
    public float slowDuration;
    private float slowTimer = 0f;
    private bool isSlowed = false;

    private void Awake()
    {
        TimeInstance = this;
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (isSlowed)
        {
            slowTimer += Time.unscaledDeltaTime;
            if (slowTimer >= slowDuration)
            {
                ResetTime();
            }
        }
    }

    public void ActivateSlowMotion(float customScale, float customDuration)
    {
        Time.timeScale = customScale > 0 ? customScale : slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        slowTimer = 0f;
        isSlowed = true;
        slowDuration = customDuration > 0 ? customDuration : slowDuration;
    }

    public void ResetTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isSlowed = false;
        slowTimer = 0f;
    }

}
