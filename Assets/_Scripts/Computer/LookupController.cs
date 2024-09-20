using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;


public class LookupController : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera _camera;

    [Header("References")]
    [SerializeField] private TMP_InputField commandInputField;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private Color errorColor;

    [Header("Text Display Variables")]
    [SerializeField] private float typingDelay = 0.04f;
    [SerializeField] private float timeBetweenDots = 1f;
    [SerializeField] private int dotMin = 2;
    [SerializeField] private int dotMax = 5;

    [Header("Item Tag Entries")]
    [SerializeField] private List<ItemTag> infoEntries = new List<ItemTag>();


    private Dictionary<string, ItemTag> infoQuery = new Dictionary<string, ItemTag>();


    private void Awake()
    {
        foreach(var entry in infoEntries)
        {
            bool isExcluded = false;
            string name = "";
            // Add info entries to query
            for (int i = 0; i < entry.Name.Length; i++)
            {
                char c = entry.Name[i];
                if (c == '<')
                {
                    isExcluded = true;
                }

                if (!isExcluded)
                {
                    name += c;
                }

                if (c == '>')
                {
                    isExcluded = false;
                }


            }
            string entryName = "info " + name;
            Debug.Log(entryName);
            entryName = entryName.ToLower();
            infoQuery.Add(entryName, entry);
        }
    }




    public void onSubmitCommand()
    {
        string input = commandInputField.text;
        List<IEnumerator> coroutines = new List<IEnumerator>();
        input = input.ToLower();

        if (input.StartsWith("info"))
        {
            string textToDisplay = $"";
            if (infoQuery.TryGetValue(input, out var info))
            {
                textToDisplay = $"{info.Name}\n...\n{info.Description}";

            }
            else
            {
                textToDisplay = $"<color=#{ColorUtility.ToHtmlStringRGB(errorColor)}>No info found on: {input.Substring(5, input.Length - 5)}";
            }
            ClearDisplay();
            coroutines.Add(DisplayText($"<color=#909090>>{input}</color>"));
            coroutines.Add(AddSpace());
            coroutines.Add(DisplayText(textToDisplay));
            StartCoroutine(ProcessCoroutines(coroutines));
        }
    }

    private void ClearDisplay()
    {
        outputText.text = "";
    }

    private IEnumerator ProcessCoroutines(List<IEnumerator> coroutines)
    {
        for (int i = 0;  i < coroutines.Count; i++)
        {
            Debug.Log($"Processing Coroutine {i}");
            yield return StartCoroutine(coroutines[i]);
        }
    }

    private IEnumerator AddSpace()
    {
        WaitForSeconds wait = new WaitForSeconds(timeBetweenDots);
        int dots = Random.Range(dotMin, dotMax+1);
        while (dots > 0)
        {
            outputText.text += ".";
            yield return wait;
            dots--;
        }
        outputText.text += "\n";
    }

    private IEnumerator DisplayText(string textToDisplay)
    {
        WaitForSeconds delay = new WaitForSeconds(typingDelay);

        for (int i = 0; i < textToDisplay.Length; i++)
        {
            char c = textToDisplay[i];

            // Takes rich tags out of the delay and adds them instantly
            if (c == '<')
            {
                while (c != '>')
                {
                    outputText.text += c;
                    i++;
                    c = textToDisplay[i];
                }
                outputText.text += c;
            }
            else
            {
                outputText.text += c;
            }
            yield return delay;
        }
    }


    public void Interact(PlayerMovement player)
    {
        _camera.enabled = true;
        Menu.ActiveMenu = Menu.MenuType.Custom;
        Menu.CustomMenuCloseAttempt.AddListener(CloseMenu);
    }

    private void CloseMenu()
    {
        _camera.enabled = false;
        Menu.ActiveMenu = Menu.MenuType.Closed;
        Menu.CustomMenuCloseAttempt.RemoveListener(CloseMenu);
    }

    public void HightlightEnter()
    {
        throw new System.NotImplementedException();
    }

    public void HightlightUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void HightlightExit()
    {
        throw new System.NotImplementedException();
    }
}
