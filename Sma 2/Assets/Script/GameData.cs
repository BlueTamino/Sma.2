using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public bool MultiplayerActive;
    public GameData(GameManager gameManager)
    {
        MultiplayerActive = gameManager.MutiplayerActive;
    }
}
