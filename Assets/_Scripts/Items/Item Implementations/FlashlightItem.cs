using UnityEngine;
using Unity.Netcode;

public class FlashlightItem : Item, ItemUseToolTip
{
    Player player;
    [SerializeField] private GameObject lightComp;
    [SerializeField] private Renderer renderer;
    [SerializeField] private Material off, on;


    public string ItemToolTip => $"Hold {Player.Input?.Player.UseItemPrimary?.activeControl?.displayName ?? "Left Mouse"} to teleport the selected (blue) player, Mouse Wheel to scroll between players";

    public override void OnEquip(object character)
    {
        base.OnEquip(character);
        if (player == null && character is Player playerComponent)
        {
            player = playerComponent;
        }
    }

    private void UpdateView()
    {

    }

    public override void OnUnequip(object character)
    {
        base.OnUnequip(character);
        renderer.material = off;
        lightComp.SetActive(false);

    }

    public override void OnEquipUpdate(object character)
    {
        base.OnEquipUpdate(character);


        if (player != null && player.IsOwner)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (lightComp.activeSelf)
                {
                    ToggleFlashlightRPC(false);
                }
                else
                {
                    ToggleFlashlightRPC(true);
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ToggleFlashlightRPC(bool isEnabled)
    {
        lightComp.SetActive(isEnabled);
        if (isEnabled)
        {
            renderer.material = on;
        }
        else
        {
            renderer.material = off;
        }
    }
}
