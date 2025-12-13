using System;
using Mirror;
using TMPro;
using UI;
using UnityEngine;
namespace Lobby
{
	public class CustomNetworkRoomPlayer : NetworkRoomPlayer
	{
		[SerializeField] TMP_Text nameText;
		[SerializeField] TMP_Text stateText;
		[SerializeField] GameObject visuals;

		[SyncVar(hook = nameof(OnNameChanged))] string playerName;

		public override void Start()
		{
			nameText.text = playerName;
			stateText.text = readyToBegin ? "Ready" : "Not Ready";
			stateText.color = readyToBegin ? Color.green : Color.red;

			base.Start();
		}

		public override void OnStartLocalPlayer()
		{
			CmdChangePlayerName(SteamManager.GetInstance().GetSteamName());
			stateText.text = readyToBegin ? "Ready" : "Not Ready";
			stateText.color = readyToBegin ? Color.green : Color.red;
			base.OnStartLocalPlayer();
		}

		[Command]
		void CmdChangePlayerName(string newName)
		{
			playerName = newName;
		}

		void OnNameChanged(string _, string __)
		{
			nameText.text = playerName;
		}

		public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
		{
			stateText.text = newReadyState ? "Ready" : "Not Ready";
			stateText.color = newReadyState ? Color.green : Color.red;
			base.ReadyStateChanged(oldReadyState, newReadyState);
		}

		[ClientRpc]
		public void OnClientPlayersReady()
		{
			Console.Write("Starting game...");
			UIDocumentController.GetInstance().HideUI();
			HideAll();
		}

		void HideAll()
		{
			CustomNetworkRoomPlayer[] roomPlayers = FindObjectsByType<CustomNetworkRoomPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
			foreach (CustomNetworkRoomPlayer roomPlayer in roomPlayers)
			{
				roomPlayer.visuals.SetActive(false);
			}
		}
	}
}
