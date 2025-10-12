using System;
using System.Linq;
using Mirror;
using UI;
using UnityEngine;
namespace Lobby
{
    public class GameManager : NetworkRoomManager
    {
        static CustomNetworkRoomPlayer LocalRoomPlayer => FindObjectsByType<CustomNetworkRoomPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).First(roomPlayer => roomPlayer.isLocalPlayer);
    
        public void CreateRoom()
        {
            Console.Write("Starting room...");
            StartHost();
            // SteamManager.GetInstance().SetRichPresence("steam_display", "#Status_AtMainMenu");
        }

        public void LeaveRoom()
        {
            if (isNetworkActive)
            {
                StopHost();
            }
        }

        public static void Ready()
        {
            if (!LocalRoomPlayer)
            {
                return;
            }
        
            LocalRoomPlayer.CmdChangeReadyState(!LocalRoomPlayer.readyToBegin);
        }

        public override void OnRoomClientConnect()
        {
            UIDocumentController.GetInstance().OpenRoomMenu();
        
            base.OnRoomClientConnect();
        }

        public override void OnRoomServerPlayersReady()
        {
            print("All players ready: " + pendingPlayers.Count);
            foreach (PendingPlayer pendingPlayer in pendingPlayers)
            {
                print("Player: " + pendingPlayer.conn.connectionId);
                pendingPlayer.roomPlayer.GetComponent<CustomNetworkRoomPlayer>().OnClientPlayersReady();
            }
        
            base.OnRoomServerPlayersReady();
        }

        public override void OnClientDisconnect()
        {
            UIDocumentController.GetInstance().OpenMainMenu();
        
            base.OnClientDisconnect();
        }
    }
}
