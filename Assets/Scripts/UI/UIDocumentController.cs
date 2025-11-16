using System.Collections.Generic;
using Lobby;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UIElements;
namespace UI
{
	[RequireComponent(typeof(UIDocument))]
	public class UIDocumentController : MonoBehaviour
	{
		static UIDocumentController instance;
		public static UIDocumentController GetInstance() => instance;


		UIDocument document;
		Dictionary<long, ServerResponse> serversFound;

		[SerializeField]
		VisualTreeAsset mainMenu;

		[SerializeField]
		VisualTreeAsset playMenu;

		[SerializeField]
		VisualTreeAsset optionsMenu;

		[SerializeField]
		VisualTreeAsset roomMenu;

		[SerializeField]
		VisualTreeAsset gameMenu;

		void Awake()
		{
			if (instance is null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}

			document = GetComponent<UIDocument>();
			serversFound = new Dictionary<long, ServerResponse>();
			UpdateVisualTree(mainMenu);
		}

		void RefreshButtons()
		{
			Button playButton = document.rootVisualElement.Q<Button>("PlayButton");
			if (playButton is not null)
			{
				playButton.clicked -= PlayButtonClicked;
				playButton.clicked += PlayButtonClicked;
			}

			Button settingsButton = document.rootVisualElement.Q<Button>("SettingsButton");
			if (settingsButton is not null)
			{
				settingsButton.clicked -= SettingsButtonClicked;
				settingsButton.clicked += SettingsButtonClicked;
			}

			Button backToMenuButton = document.rootVisualElement.Q<Button>("BackToMenuButton");
			if (backToMenuButton is not null)
			{
				backToMenuButton.clicked -= BackToMenuButtonClicked;
				backToMenuButton.clicked += BackToMenuButtonClicked;
			}

			Button createRoomButton = document.rootVisualElement.Q<Button>("CreateRoomButton");
			if (createRoomButton is not null)
			{
				createRoomButton.clicked -= CreateRoomButtonClicked;
				createRoomButton.clicked += CreateRoomButtonClicked;
			}

			Button readyButton = document.rootVisualElement.Q<Button>("ReadyButton");
			if (readyButton is not null)
			{
				readyButton.clicked -= ReadyButtonClicked;
				readyButton.clicked += ReadyButtonClicked;
			}

			ListView container = document.rootVisualElement.Q<ListView>("ServerListView");
			if (container is not null)
			{
				container.hierarchy.Clear();

				foreach (KeyValuePair<long, ServerResponse> serverResponse in serversFound)
				{
					Button joinButton = new Button
					{
						text = $"Join {serverResponse.Value.uri.Host}"
					};

					joinButton.clicked += () =>
					{
						NetworkManager.singleton.StartClient(serverResponse.Value.uri);
					};

					container.hierarchy.Add(joinButton);
				}
			}
		}

		void UpdateVisualTree(VisualTreeAsset asset)
		{
			document.visualTreeAsset = asset;
			RefreshButtons();
		}

		void PlayButtonClicked() => UpdateVisualTree(playMenu);
		void SettingsButtonClicked() => UpdateVisualTree(optionsMenu);
		void BackToMenuButtonClicked() => ((GameManager)NetworkManager.singleton).LeaveRoom();
		void CreateRoomButtonClicked() => ((GameManager)NetworkManager.singleton).CreateRoom();
		void ReadyButtonClicked() => GameManager.Ready();

		public void AddServerToList(ServerResponse response)
		{
			if (serversFound.ContainsKey(response.serverId))
			{
				return;
			}

			ListView container = document.rootVisualElement.Q<ListView>("ServerListView");
			serversFound.Add(response.serverId, response);
			if (container is null)
			{
				return;
			}

			Button joinButton = new Button
			{
				text = $"Join {response.uri.Host}"
			};

			joinButton.clicked += () =>
			{
				NetworkManager.singleton.StartClient(response.uri);
			};

			container.hierarchy.Add(joinButton);
		}
		public void OpenMainMenu() => UpdateVisualTree(mainMenu);
		public void OpenGameMenu() => UpdateVisualTree(gameMenu);
		public void OpenRoomMenu() => UpdateVisualTree(roomMenu);
	}
}
