using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.Discovery;
using UI;
using UnityEngine;
namespace Lobby
{
    [RequireComponent(typeof(NetworkDiscovery))]
    public class GameManager : NetworkRoomManager
    {
        static CustomNetworkRoomPlayer LocalRoomPlayer => FindObjectsByType<CustomNetworkRoomPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).First(roomPlayer => roomPlayer.isLocalPlayer);

        NetworkDiscovery networkDiscovery;
        
        Dictionary<NetworkStartPosition, CustomNetworkRoomPlayer> spawnerStates;

        public override void Start()
        {
            networkDiscovery = GetComponent<NetworkDiscovery>();
            networkDiscovery.StartDiscovery();
            
            base.Start();
        }

        public void CreateRoom()
        {
            Console.Write("Starting room...");
            StartHost();
            networkDiscovery.AdvertiseServer();
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

        public void OnServerFound(ServerResponse response)
        {
            UIDocumentController.GetInstance().AddServerToList(response);
        }

        public override void OnRoomClientConnect()
        {
            UIDocumentController.GetInstance().OpenRoomMenu();
        
            base.OnRoomClientConnect();
        }

        public override void OnRoomStartServer()
        {
            spawnerStates = FindObjectsByType<NetworkStartPosition>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID)
                .ToDictionary(sp => sp, _ => (CustomNetworkRoomPlayer)null);
            
            base.OnRoomStartServer();
        }

        public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
        {
            (NetworkStartPosition spawner, _) = spawnerStates.FirstOrDefault(x => x.Value is null);

            if (spawner != null)
            {

                GameObject roomPlayer = Instantiate(roomPlayerPrefab.gameObject, spawner.transform.position, spawner.transform.rotation);

                NetworkServer.Spawn(roomPlayer, conn);
                spawnerStates[spawner] = roomPlayer.GetComponent<CustomNetworkRoomPlayer>();

                return roomPlayer;
            }

            conn.Disconnect();
            return null;
        }

        public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity)
            {
                CustomNetworkRoomPlayer roomPlayer = conn.identity.GetComponent<CustomNetworkRoomPlayer>();
        
                KeyValuePair<NetworkStartPosition, CustomNetworkRoomPlayer> spawnerEntry = spawnerStates.FirstOrDefault(sp => sp.Value == roomPlayer);

                if (spawnerEntry.Key)
                {
                    spawnerStates[spawnerEntry.Key] = null;
                }
            }
    
            base.OnRoomServerDisconnect(conn);
        }

        public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
        {
            roomPlayer.GetComponent<CustomNetworkRoomPlayer>().OnClientPlayersReady();
            return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
        }

        public override void OnClientDisconnect()
        {
            UIDocumentController.GetInstance().OpenMainMenu();
        
            base.OnClientDisconnect();
        }
    }
}
