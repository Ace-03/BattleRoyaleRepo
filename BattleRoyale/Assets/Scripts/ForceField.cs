using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ForceField : MonoBehaviourPun
{
    public float shrinkWaitTime;
    public float shrinkAmount;
    public float shrinkDuration;
    public float minShrinkAmount;
    public Vector3 targetCenterLocation;
    public float centerLocationContstraint;
    public int playerDamage;

    private float lastShrinkEndTime;
    private bool shrinking;
    private float targetDiameter;
    private float lastPlayerCheckTime;

    void Start()
    {
        lastShrinkEndTime = Time.time;
        targetDiameter = transform.localScale.x;
    }

    void Update()
    {
        if (shrinking)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * targetDiameter, (shrinkAmount / shrinkDuration) * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetCenterLocation, (shrinkAmount / shrinkDuration) * Time.deltaTime);
            if (transform.localScale.x == targetDiameter)
                shrinking = false;
        }
        else 
        {
            // can we shrink again?
            if (Time.time - lastShrinkEndTime >= shrinkWaitTime && transform.localScale.x > minShrinkAmount)
                Shrink();
        }

        CheckPlayers();
    }

    void Shrink()
    {
        shrinking = true;

        // make sure we don't shrink below the min amount
        if (transform.localScale.x - shrinkAmount > minShrinkAmount)
            targetDiameter -= shrinkAmount;
        else
            targetDiameter = minShrinkAmount;
        if (PhotonNetwork.IsMasterClient)
        {
            targetCenterLocation = new Vector3(Random.Range(-50, 50), 0, Random.Range(-50, 50));
            photonView.RPC("SetOthercCenterLocation", RpcTarget.Others, targetCenterLocation);
        }
        lastShrinkEndTime = Time.time + shrinkAmount;
    }

    [PunRPC]
    void SetOthercCenterLocation(Vector3 target)
    {
        targetCenterLocation = target;
    }
    void CheckPlayers()
    {
        if (Time.time - lastPlayerCheckTime > 1.0f)
        {
            lastPlayerCheckTime = Time.time;

            // loop through all players
            foreach(PlayerController player in GameManager.instance.players)
            {
                if (player.dead || !player)
                    continue;
                if (Vector3.Distance(this.transform.position, player.transform.position) >= transform.localScale.x)
                {
                    player.photonView.RPC("TakeDamage", player.photonPlayer, 0, playerDamage);
                }
            }
        }
    }
}
