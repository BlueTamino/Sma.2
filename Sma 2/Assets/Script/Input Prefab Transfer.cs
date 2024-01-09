using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPrefabTransfer : MonoBehaviour
{
    private GameObject Player;
    public void RegisterPlayer(GameObject player)
    {
        Player = player;
    }
    public void Jump()
    {
        Player.GetComponent<PlayerMovement>().Jump();
    }
}
