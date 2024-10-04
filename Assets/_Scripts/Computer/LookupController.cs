using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class LookupController : MonoBehaviour, IInteractable
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera _camera;

    [Header("References")]
    [SerializeField] private TMP_InputField commandInputField;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private Color errorColor;

    [Header("Text Display Variables")]
    [SerializeField] private float typingDelay = 0.01f;
    [SerializeField] private float timeBetweenDots = 1f;
    [SerializeField] private int dotMin = 2;
    [SerializeField] private int dotMax = 5;

    [Header("Item Tag Entries")]
    [SerializeField] private List<ItemTag> infoEntries = new List<ItemTag>();


    private Dictionary<string, ItemTag> infoQuery = new Dictionary<string, ItemTag>();

    public bool Interactible => true;

    private void Awake()
    {
        foreach (var entry in infoEntries)
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
        StopAllCoroutines();
        string input = commandInputField.text;
        List<IEnumerator> coroutines = new List<IEnumerator>();
        input = input.ToLower();

        if (input.StartsWith("cmds"))
        {
            string textToDisplay = $"<color=#909090>info [search]</color> - Get information on something\n";
            textToDisplay += $"<color=#909090>help</color> - instructions\n";
            textToDisplay += $"<color=#909090>list</color> - List all attribues\n";
            textToDisplay += $"<color=#909090>clear</color> - Clear the screen\n";
            textToDisplay += $"<color=#909090>exit</color> - Close the computer\n";
            ClearDisplay();
            coroutines.Add(DisplayText($"<color=#909090>>{input}</color>"));
            coroutines.Add(AddSpace());
            coroutines.Add(DisplayText(textToDisplay));
            StartCoroutine(ProcessCoroutines(coroutines));
            return;
        }
        if (input.StartsWith("clear"))
        {
            ClearDisplay();
            return;
        }
        if (input.StartsWith("help"))
        {
            string textToDisplay = $"With the handheld scanner, (<color=#909090>1</color>) you can find the notable features of items\n";
            textToDisplay += $"\n";
            textToDisplay += $"Use this console to search up notable features of items.\n";
            textToDisplay += $"\n";
            textToDisplay += $"Use the information to deduce where the item is meant to go\n";
            textToDisplay += $"\n";
            textToDisplay += $"Some items are meant to stay in this time period, do not trash those.\n";
            textToDisplay += $"\n";
            textToDisplay += $"Good luck Chrono Custodians\n";
            ClearDisplay();
            coroutines.Add(DisplayText($"<color=#909090>>{input}</color>"));
            coroutines.Add(AddSpace());
            coroutines.Add(DisplayText(textToDisplay));
            StartCoroutine(ProcessCoroutines(coroutines));
            return;
        }
        if (input.StartsWith("exit"))
        {
            CloseMenu();
            return;
        }
        if (input.StartsWith("list"))
        {
            ClearDisplay();
            coroutines.Add(DisplayText($"<color=#909090>>List</color>"));
            coroutines.Add(AddSpace());
            foreach (var entry in infoEntries)
            {
                coroutines.Add(DisplayText(entry.Name + "\n"));
            }
            StartCoroutine(ProcessCoroutines(coroutines));
            return;
        }
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
            return;
        }
        ClearDisplay();
        coroutines.Add(DisplayText($"<color=#{ColorUtility.ToHtmlStringRGB(errorColor)}>Command not found: {input}</color>"));
        coroutines.Add(AddSpace());
        coroutines.Add(DisplayText($"<color=#909090>Try typing 'help' for a list of commands</color>"));
        StartCoroutine(ProcessCoroutines(coroutines));
    }

    private void ClearDisplay()
    {
        outputText.text = "";
    }

    private IEnumerator ProcessCoroutines(List<IEnumerator> coroutines)
    {
        for (int i = 0; i < coroutines.Count; i++)
        {
            Debug.Log($"Processing Coroutine {i}");
            yield return StartCoroutine(coroutines[i]);
        }
    }

    private IEnumerator AddSpace()
    {
        WaitForSeconds wait = new WaitForSeconds(timeBetweenDots);
        int dots = Random.Range(dotMin, dotMax + 1);
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


    public void Interact(Player player)
    {
        OpenMenu();
    }

    private void OpenMenu()
    {
        _camera.enabled = true;
        Menu.ActiveMenu = Menu.MenuType.Custom;
        Menu.CustomMenuCloseAttempt.AddListener(CloseMenu);
        Hud.Hidden = true;
        commandInputField.Select();
    }

    private void CloseMenu()
    {
        _camera.enabled = false;
        Menu.ActiveMenu = Menu.MenuType.Closed;
        Menu.CustomMenuCloseAttempt.RemoveListener(CloseMenu);
        Hud.Hidden = false;
        EventSystem.current.SetSelectedGameObject(null);
        StopAllCoroutines();
        ClearDisplay();
    }


}
