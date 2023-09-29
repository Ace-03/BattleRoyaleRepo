using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PickUpType
{
    Health,
    Ammo
}

public class Pickup : MonoBehaviourPun
{
    public PickUpType type;
    public int value;

   
    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (other.CompareTag("Player"))
        {
            // get the player
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (type == PickUpType.Health)
                player.photonView.RPC("Heal", player.photonPlayer, value);
            else if (type == PickUpType.Ammo)
                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);

            // destory the object
            photonView.RPC("DestroyPickup", RpcTarget.AllBuffered);
        }

        
    }

    [PunRPC]
    public void DestroyPickup()
    {
        Destroy(gameObject);
    }
}
