using System;
using TMPro;
using UnityEngine;

public class TimerUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text[] textToUpdate;

    [SerializeField] private Color colorToGoTo = Color.red;

    [SerializeField] private float timerColorIntensity = 0;

    private void Start()
    {
        GameManager.instance.timer.OnValueChanged += UpdateText;
    }

    private void UpdateText(float previousTime, float currentTime)
    {
        if (MathF.Floor(previousTime) != MathF.Floor(currentTime))
        {
            timerColorIntensity = 0;
        }
        Color newColor = Color.white;
        if (currentTime < 60)
        {
            newColor = Color.Lerp(Color.white, colorToGoTo, timerColorIntensity);
            timerColorIntensity += Time.deltaTime * 0.5f;
            Hud.TimerText.color = newColor;
        }

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        string timerString = $"{minutes:0}:{seconds:00}";
        Hud.TimerText.text = timerString;
        foreach(var text in textToUpdate)
        {
            text.color = newColor;
            text.text = timerString;
        }
    }
}
