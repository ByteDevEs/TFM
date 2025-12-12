using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Level;
using Mirror;
using Mirror.Discovery;
using UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Lobby
{
	[RequireComponent(typeof(NetworkDiscovery))]
	public class GameManager : NetworkRoomManager
	{
		public LevelGenerator levelGenerator;
		
		static CustomNetworkRoomPlayer LocalRoomPlayer => FindObjectsByType<CustomNetworkRoomPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).First(roomPlayer => roomPlayer.isLocalPlayer);
		NetworkDiscovery networkDiscovery;
		Dictionary<NetworkStartPosition, CustomNetworkRoomPlayer> spawnerStates;

		public override void Start()
		{
			levelGenerator = GetComponent<LevelGenerator>();
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

		public override void OnRoomServerSceneChanged(string sceneName)
		{
			// if (sceneName != "GameScene")
			// {
			// 	GameObject enemyGo = Instantiate(Prefabs.GetInstance().prototypeEnemy, new Vector3(-3, -99, -2), Quaternion.identity);
			// 	NetworkServer.Spawn(enemyGo);
			// }

			base.OnRoomServerSceneChanged(sceneName);
		}

		public override void OnClientDisconnect()
		{
			UIDocumentController.GetInstance().OpenMainMenu();

			base.OnClientDisconnect();
		}

		[Server]
		public void SrvMovePlayerToLevel(GameObject player, int levelNumber, int difference)
		{
			print("Moving player to level " + levelNumber + difference);
			
			LevelGrid oldLevel = levelGenerator.GetOrAddLevel(levelNumber);
			LevelGrid newLevel = levelGenerator.GetOrAddLevel(levelNumber + difference);

			if (oldLevel)
			{
				oldLevel.RemovePlayer();
			}

			if (newLevel)
			{
				newLevel.AddPlayer();
			
				int3 stairPosition = difference == 1 ? newLevel.startPosition : newLevel.exitPosition;
				float x = newLevel.transform.position.x + stairPosition.x * newLevel.roomSize;
				float y = newLevel.transform.position.y + 1;
				float z = newLevel.transform.position.z + stairPosition.y * newLevel.roomSize;
				Vector3 position = new Vector3(x, y, z);
				player.GetComponent<NavMeshAgent>().enabled = false;
				player.transform.position = position;
				player.GetComponent<NavMeshAgent>().enabled = true;
				player.GetComponent<NavMeshAgent>().SetDestination(position);
			}
		}
	}
}