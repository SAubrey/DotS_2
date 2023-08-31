using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public float time = 1100f;
    public Transform SunTransform, MoonTransform;
    public Light Sun, Moon;

    public const int DayDuration = 600; // num seconds
    public const int DayDurationHalf = DayDuration / 2;
    public float intensity;
    public Color fogday = Color.gray;
    public Color fognight = Color.black;

    //[Tooltip("The Amount Of 24hour Periods in Each Day, Default is 4")]
    public int timeMultiplier = 1;
    public int speed;

    private void Start()
    {
        speed = timeMultiplier;
    }

    void Update()
    {
        ChangeTime();
    }

    public void ChangeTime()
    {
        time += Time.deltaTime * speed;

        SunTransform.rotation = Quaternion.Euler(new Vector3((time - DayDuration / 4f) / DayDuration * 360, 0, 0));
        MoonTransform.rotation = Quaternion.Euler(new Vector3(((time - DayDuration / 4f) / DayDuration * 360) + 180, 0, 0));
        
        intensity = 1 - ((DayDurationHalf - time) / DayDurationHalf * ((Mathf.Sign(time - DayDurationHalf)) * -1));

        RenderSettings.fogColor = Color.Lerp(fognight, fogday, intensity * intensity);
        if (time > DayDuration)
        {
            time = 0;
        }

        Sun.intensity = intensity * .65f;
        Moon.intensity = .4f - intensity * .4f; // invert due to opposite phase
    }
}