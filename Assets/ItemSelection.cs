using Unity.Netcode;
using UnityEngine;

public class ItemSelection : NetworkBehaviour, ILongInteractable, IInteractionMessage
{
    [SerializeField] private GameObject itemPrefab;

    public float InteractTime => 2;

    [SerializeField] private string interactMessage = "Hold E to equip item";
    public string InteractionMessage => interactMessage;

    public string CantInteractMessage => "Cannot equip items during playthrough";

    private float cooldown = 5;

    public void LongInteract(Player player)
    {
        if (player.Inventory.TryRemoveItem(0))
        {
            GameObject GO = Instantiate(itemPrefab);
            NetworkObject networkItem = GO.GetComponent<NetworkObject>();
            networkItem.Spawn();
            player.Inventory.TryAddItem(networkItem, true);
            cooldown = 5;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void Update()
    {
        if (cooldown < 0) return;
        cooldown -= Time.deltaTime;
        if (cooldown < 0)
        {
            GetComponent<Collider>().enabled = true;
        }
    }
}
