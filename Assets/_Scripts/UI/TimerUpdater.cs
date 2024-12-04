using System;
using TMPro;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class TimerUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text[] textToUpdate;

    [SerializeField] private Color colorToGoTo = Color.red;

    [SerializeField] private float timerColorIntensity = 0;
    private bool isAnimating = false;

    private Color flashColor = Color.white;

    private void Start()
    {
        SetText();
        GameManager.instance.OnLevelEndClient.AddListener(SetText);
        GameManager.instance.timer.OnValueChanged += UpdateText;
    }

    private void SetText()
    {
        foreach (var text in textToUpdate)
        {
            text.text = "--:--";
        }
    }

    private void UpdateText(float previousTime, float currentTime)
    {
        if (previousTime - currentTime < 0)
        {
            isAnimating = true;
            timerColorIntensity = 0;
            flashColor = Color.green;
        }
        else if (previousTime - currentTime > 2)
        {
            isAnimating = true;
            timerColorIntensity = 0;
            flashColor = Color.red;
        }
        Color newColor = Color.white;
        if (isAnimating)
        {
            newColor = Color.Lerp(flashColor, Color.white, timerColorIntensity);
            timerColorIntensity += Time.deltaTime;
            if (timerColorIntensity > 1)
            {
                isAnimating = false;
            }
        }
        else
        {
            if (MathF.Floor(previousTime) != MathF.Floor(currentTime))
            {
                timerColorIntensity = 0;
            }
            if (currentTime < 60)
            {
                newColor = Color.Lerp(Color.white, colorToGoTo, timerColorIntensity);
                timerColorIntensity += Time.deltaTime * 0.5f;
            }
        }

        string timerString = "";

        if (currentTime <= 0)
        {
            timerString = $"--:--";
        }
        else
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerString = $"{minutes:00}:{seconds:00}";
        }

        Hud.TimerText.text = timerString;
        Hud.TimerText.color = newColor;
        foreach (var text in textToUpdate)
        {
            text.color = newColor;
            text.text = timerString;
        }
    }
}
