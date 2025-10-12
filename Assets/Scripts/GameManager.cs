using System;
using System.Linq;
using Mirror;
using UnityEngine;

public class GameManager : NetworkRoomManager
{
    NetworkRoomPlayer localRoomPlayer;
    
    public void CreateRoom()
    {
        Console.Write("Starting room...");
        StartHost();
        // SteamManager.GetInstance().SetRichPresence("steam_display", "#Status_AtMainMenu");
    }

    public override void OnRoomClientEnter()
    {
        base.OnRoomClientEnter();
        
        localRoomPlayer = FindObjectsByType<NetworkRoomPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).First(roomPlayer => roomPlayer.isLocalPlayer);
    }

    public void Ready()
    {
        if (!localRoomPlayer)
        {
            return;
        }
        
        localRoomPlayer.CmdChangeReadyState(!localRoomPlayer.readyToBegin);
    }
}
