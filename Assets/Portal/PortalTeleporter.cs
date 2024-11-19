using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour {

	public Transform player;
	public Transform reciever;

	private bool playerIsOverlapping = false;

	// Update is called once per frame
	void Update () {
		if (player == null)
		{
			if (Player.LocalPlayer == null) return;
			player = Player.LocalPlayer.transform;
		}

		if (playerIsOverlapping)
		{
			Vector3 portalToPlayer = player.transform.position - transform.position;
			float dotProduct = Vector3.Dot(Player.LocalPlayer.HeadTransform.up, portalToPlayer);

			// If this is true: The player has moved across the portal
			if (dotProduct < 0f)
			{
				// Teleport him!
				float rotationDiff = -Quaternion.Angle(transform.rotation, reciever.rotation);
				rotationDiff += 180;
				//player.Rotate(Vector3.up, rotationDiff);

				Vector3 positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * portalToPlayer;
				Player.LocalPlayer.GetComponent<PlayerMovement>().TeleportRpc(reciever.position + positionOffset);

				playerIsOverlapping = false;
			}
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Player")
		{
			playerIsOverlapping = true;
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.tag == "Player")
		{
			playerIsOverlapping = false;
		}
	}
}
