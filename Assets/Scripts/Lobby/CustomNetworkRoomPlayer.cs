using System;
using Mirror;
using TMPro;
using UI;
using UnityEngine;
namespace Lobby
{
	public class CustomNetworkRoomPlayer : NetworkRoomPlayer
	{
		[SerializeField] TMP_Text NameText;
		[SerializeField] TMP_Text StateText;
		[SerializeField] GameObject Visuals;

		[SyncVar(hook = nameof(OnNameChanged))] string playerName;

		public override void Start()
		{
			NameText.text = playerName;
			StateText.text = readyToBegin ? "Ready" : "Not Ready";
			StateText.color = readyToBegin ? Color.green : Color.red;

			base.Start();
		}

		public override void OnStartLocalPlayer()
		{
			CmdChangePlayerName(SteamManager.GetInstance().GetSteamName());
			StateText.text = readyToBegin ? "Ready" : "Not Ready";
			StateText.color = readyToBegin ? Color.green : Color.red;
			base.OnStartLocalPlayer();
		}

		[Command]
		void CmdChangePlayerName(string newName)
		{
			playerName = newName;
		}

		void OnNameChanged(string _, string newName)
		{
			NameText.text = newName;
		}

		public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
		{
			StateText.text = newReadyState ? "Ready" : "Not Ready";
			StateText.color = newReadyState ? Color.green : Color.red;
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
				roomPlayer.Visuals.SetActive(false);
			}
		}
	}
}
