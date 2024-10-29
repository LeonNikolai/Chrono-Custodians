using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] MaterialReplacer replacer;
    [SerializeField] Animator animator;

    void Awake()
    {
        player.PlayerIsInMenu.OnValueChanged += OnPlayerIsInMenu;
    }

    void Update()
    {
        
    }
    private void OnPlayerIsInMenu(bool previousValue, bool newValue)
    {
        replacer.Show = newValue;
    }
}