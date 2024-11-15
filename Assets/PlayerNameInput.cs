using TMPro;
using UnityEngine;

public class PlayerNameInput : MonoBehaviour
{

    public TMP_InputField inputField;

    void Awake()
    {
        Player.CustomUserData.Load();
        inputField.onEndEdit.AddListener(SetPlayerName);
    }
    void OnEnable()
    {
        inputField.text = Player.CustomUserData.PlayerName;
    }
    void OnDestroy()
    {
        inputField.onEndEdit.RemoveListener(SetPlayerName);
    }
    public void SetPlayerName(string name)
    {
        Player.CustomUserData.PlayerName = name;
        Player.CustomUserData.Save();
    }
}
