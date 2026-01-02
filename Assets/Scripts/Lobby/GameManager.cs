using System.Collections;
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
using UnityEngine.SceneManagement;
using Steamworks;

namespace Lobby
{
	[RequireComponent(typeof(NetworkDiscovery))]
	public class GameManager : NetworkRoomManager
	{
		public LevelGenerator LevelGenerator;

		static CustomNetworkRoomPlayer LocalRoomPlayer => FindObjectsByType<CustomNetworkRoomPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).First(roomPlayer => roomPlayer.isLocalPlayer);

		protected Callback<LobbyCreated_t> LobbyCreated;
		protected Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequested;
		protected Callback<LobbyEnter_t> LobbyEntered;
		protected Callback<LobbyMatchList_t> LobbyList;

		public CSteamID CurrentLobbyID;
		const string HostAddressKey = "HostAddress";

		NetworkDiscovery networkDiscovery;

		Dictionary<NetworkStartPosition, CustomNetworkRoomPlayer> spawnerStates;
		Dictionary<NetworkConnectionToClient, GameObject> connections;
		Coroutine checkingAllDead;

		public override void Start()
		{
			LevelGenerator = GetComponent<LevelGenerator>();
			connections = new Dictionary<NetworkConnectionToClient, GameObject>();

			networkDiscovery = GetComponent<NetworkDiscovery>();
			networkDiscovery.StartDiscovery();

			if (SteamManager.Initialized)
			{
				LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
				GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
				LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
				LobbyList = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
			}

			base.Start();
		}

		public void CreateRoom()
		{
			StartHost();

			if (SteamManager.Initialized)
			{
				SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, maxConnections);
			}

			networkDiscovery.AdvertiseServer();
		}

		void OnLobbyCreated(LobbyCreated_t callback)
		{
			if (callback.m_eResult != EResult.k_EResultOK) return;

			CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);

			SteamMatchmaking.SetLobbyData(CurrentLobbyID, HostAddressKey, SteamUser.GetSteamID().ToString());
			SteamMatchmaking.SetLobbyData(CurrentLobbyID, "name", SteamFriends.GetPersonaName() + "'s Room");
		}

		public void OnServerFound(ServerResponse response)
		{
			UIDocumentController.GetInstance().AddServerToList(response);
		}

		void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
		{
			SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
		}

		void OnLobbyEntered(LobbyEnter_t callback)
		{
			if (NetworkServer.active)
			{
				return;
			}

			CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
			string hostAddress = SteamMatchmaking.GetLobbyData(CurrentLobbyID, HostAddressKey);

			networkAddress = hostAddress;
			StartClient();
		}

		public void SearchSteamLobbies()
		{
			SteamMatchmaking.RequestLobbyList();
		}

		void OnLobbyMatchList(LobbyMatchList_t callback)
		{
			for (int i = 0; i < callback.m_nLobbiesMatching; i++)
			{
				CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
				string lobbyData = SteamMatchmaking.GetLobbyData(lobbyID, "name");

				UIDocumentController.GetInstance().AddSteamLobbyToUI(lobbyID, lobbyData);
			}
		}

		public void LeaveRoom()
		{
			if (!isNetworkActive)
			{
				return;
			}

			if (NetworkServer.active)
			{
				if (CurrentLobbyID != CSteamID.Nil)
				{
					SteamMatchmaking.LeaveLobby(CurrentLobbyID);
				}
				networkDiscovery.StopDiscovery();
			}
			StopHost();
		}

		public static void Ready()
		{
			if (!LocalRoomPlayer)
			{
				return;
			}

			LocalRoomPlayer.CmdChangeReadyState(!LocalRoomPlayer.readyToBegin);
		}

		public override void OnRoomServerSceneChanged(string sceneName)
		{
			if (sceneName.Contains("RoomScene"))
			{
				if (CurrentLobbyID != CSteamID.Nil)
				{
					SteamMatchmaking.SetLobbyJoinable(CurrentLobbyID, true);
				}

				networkDiscovery.AdvertiseServer();

				NetworkStartPosition[] newSpawners = FindObjectsByType<NetworkStartPosition>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
				spawnerStates = newSpawners.ToDictionary(sp => sp, _ => (CustomNetworkRoomPlayer)null);

				int index = 0;
				foreach (NetworkRoomPlayer player in roomSlots.Where(player => player))
				{
					if (index < newSpawners.Length)
					{
						spawnerStates[newSpawners[index]] = player.GetComponent<CustomNetworkRoomPlayer>();
					}
					index++;
				}
			}
			else if (sceneName.Contains("GameScene"))
			{
				if (CurrentLobbyID != CSteamID.Nil)
				{
					SteamMatchmaking.SetLobbyJoinable(CurrentLobbyID, false);
				}

				networkDiscovery.StopDiscovery();

				if (checkingAllDead is not null)
				{
					StopCoroutine(checkingAllDead);
				}

				checkingAllDead = StartCoroutine(StartGame());
			}

			base.OnRoomServerSceneChanged(sceneName);
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
			(NetworkStartPosition spawner, _) = spawnerStates.FirstOrDefault(x => !x.Value);
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
			GameObject gamePlayer = base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
			connections.Add(conn, gamePlayer);
			return gamePlayer;
		}

		public override void OnClientDisconnect()
		{
			UIDocumentController.GetInstance().OpenMainMenu();
			base.OnClientDisconnect();
		}

		public override void OnRoomClientSceneChanged()
		{
			if (SceneManager.GetActiveScene().name.Contains("RoomScene"))
			{
				UIDocumentController.GetInstance().OpenRoomMenu();
			}

			base.OnRoomClientSceneChanged();
		}

		[Server]
		IEnumerator StartGame()
		{
			yield return new WaitForSeconds(1.0f);

			while (true)
			{
				yield return new WaitForSeconds(0.5f);

				if (!SceneManager.GetActiveScene().name.Contains("GameScene"))
				{
					yield break;
				}

				if (connections.Count == 0)
				{
					continue;
				}

				bool allDead = FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID) is {} players
				               && players.Length != 0
				               && players.Count(player => player.IsDead) == players.Length
				               && SceneManager.GetActiveScene().name.Contains("GameScene");

				if (allDead)
				{
					EndGameAndReturnToLobby();
					yield break;
				}
			}
		}

		[Server]
		void EndGameAndReturnToLobby()
		{
			foreach (NetworkRoomPlayer roomPlayer in roomSlots)
			{
				if (roomPlayer == null)
				{
					continue;
				}
				
				NetworkConnectionToClient conn = roomPlayer.connectionToClient;

				if (conn != null && connections.TryGetValue(conn, out GameObject gamePlayer))
				{
					NetworkServer.ReplacePlayerForConnection(conn, roomPlayer.gameObject, ReplacePlayerOptions.Destroy);

					(roomPlayer as CustomNetworkRoomPlayer)?.SrvShowAll();
					(roomPlayer as CustomNetworkRoomPlayer)?.SrvChangeReadyState(false);

					if (gamePlayer != null)
					{
						NetworkServer.Destroy(gamePlayer);
					}
				}
			}

			connections.Clear();
			ServerChangeScene("RoomScene");
		}

		[Server]
		public void SrvMovePlayerToLevel(GameObject player, int levelNumber, int difference)
		{
			print("Moving player to level " + levelNumber + difference);
			
			LevelGrid oldLevel = LevelGenerator.GetOrAddLevel(levelNumber);
			LevelGrid newLevel = LevelGenerator.GetOrAddLevel(levelNumber + difference);

			if (oldLevel)
			{
				oldLevel.RemovePlayer();
			}

			if (newLevel)
			{
				newLevel.AddPlayer();
			
				int3 stairPosition = difference == 1 ? newLevel.StartPosition : newLevel.ExitPosition;
				float x = newLevel.transform.position.x + stairPosition.x * newLevel.RoomSize;
				float y = newLevel.transform.position.y - 5.0f;
				float z = newLevel.transform.position.z + stairPosition.y * newLevel.RoomSize;
				Vector3 targetPosition = new Vector3(x, y, z);
				
				NavMeshAgent agent = player.GetComponent<NavMeshAgent>();

				if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
				{
					agent.Warp(hit.position);
					agent.SetDestination(hit.position);
				}
				else
				{
					agent.Warp(targetPosition);
				}
			}
		}
	}
}