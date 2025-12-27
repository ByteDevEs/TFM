using System.Collections.Generic;
using Controllers;
using Helpers;
using Lobby;
using Mirror;
using Mirror.Discovery;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
namespace UI
{
	[RequireComponent(typeof(UIDocument))]
	public class UIDocumentController : MonoBehaviour
	{
		public static UIDocumentController GetInstance() => instance;
		static UIDocumentController instance;
		
		UIDocument document;
		Dictionary<long, ServerResponse> serversFound;

		[SerializeField]
		VisualTreeAsset MainMenu;

		[SerializeField]
		VisualTreeAsset PlayMenu;

		[SerializeField]
		VisualTreeAsset OptionsMenu;

		[SerializeField]
		VisualTreeAsset RoomMenu;

		[SerializeField]
		VisualTreeAsset GameMenu;
		
		[SerializeField]
		VisualTreeAsset RoomButtonTemplate;

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
			UpdateVisualTree(MainMenu);
		}

		void UpdateVisualTree(VisualTreeAsset asset)
		{
			document.visualTreeAsset = asset;
			RefreshButtons();
			RegisterButtonSounds();
		}

		void RegisterButtonSounds()
		{
			List<Button> allButtons = document.rootVisualElement.Query<Button>().ToList();

			foreach (Button btn in allButtons)
			{
				btn.RegisterCallback<MouseEnterEvent>(OnButtonHover);
				btn.clicked -= BtnOnClicked;
				btn.clicked += BtnOnClicked;
			}
		}
		static void BtnOnClicked()
		{
			Prefabs.GetInstance().PlaySound("Click");
		}

		static void OnButtonHover(MouseEnterEvent e)
		{
			if (e.currentTarget is not Button { enabledSelf: true })
			{
				return;
			}
			
			Prefabs.GetInstance().PlaySound("Hover");
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

			Button backToMainMenuButton = document.rootVisualElement.Q<Button>("BackToMainMenuButton");
			if (backToMainMenuButton is not null)
			{
				backToMainMenuButton.clicked -= BackToMainMenuButtonClicked;
				backToMainMenuButton.clicked += BackToMainMenuButtonClicked;
			}

			Button backToSearchMenuButton = document.rootVisualElement.Q<Button>("BackToSearchMenuButton");
			if (backToSearchMenuButton is not null)
			{
				backToSearchMenuButton.clicked -= BackToSearchMenuButtonClicked;
				backToSearchMenuButton.clicked += BackToSearchMenuButtonClicked;
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
					TemplateContainer roomFound = RoomButtonTemplate.Instantiate();
					
					Button button = roomFound.Q<Button>("ServerFoundButton");
					if (button != null)
					{
						button.text = $"Join {serverResponse.Value.uri.Host}";
						button.clickable = new Clickable(() => NetworkManager.singleton.StartClient(serverResponse.Value.uri));
					}
            
					container.hierarchy.Add(roomFound);
				}
			}
			
			VisualElement healthMask = document.rootVisualElement.Q<VisualElement>("HealthMask");

			if (healthMask is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}
				
				DataBinding dataBinding = new DataBinding
				{
					bindingMode = BindingMode.ToTarget,
					dataSource = PlayerController.LocalPlayer.HealthController, 
					dataSourcePath = new PropertyPath(nameof(PlayerController.LocalPlayer.HealthController.HealthPercentage)),
					updateTrigger = BindingUpdateTrigger.OnSourceChanged
				};

				dataBinding.sourceToUiConverters.AddConverter((ref float v) => 
					new StyleLength(new Length(v * 100f, LengthUnit.Percent)));
				
				healthMask.SetBinding("style.height", dataBinding);
			}
			
			RadialProgress potionCooldownProgress = document.rootVisualElement.Q<RadialProgress>("PotionCooldownProgress");

			if (potionCooldownProgress is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				potionCooldownProgress.schedule.Execute(() => 
				{
					potionCooldownProgress.Progress = PlayerController.LocalPlayer.HealthController.PotionCooldownPercentage;
        
				}).Every(0);
			}
			
			Label potionCountLabel = document.rootVisualElement.Q<Label>("PotionCountLabel");
			
			if (potionCountLabel is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				potionCountLabel.schedule.Execute(() =>
				{
					potionCountLabel.text = $"{PlayerController.LocalPlayer.HealthController.PotionCount}";
				}).Every(0);
			}
			
			Button backToLobbyButton = document.rootVisualElement.Q<Button>("BackToLobbyButton");

			if (backToLobbyButton is not null)
			{
				backToLobbyButton.clicked -= BackToLobbyButtonClicked;
				backToLobbyButton.clicked += BackToLobbyButtonClicked;
			}
			
			Button speedButton = document.rootVisualElement.Q<Button>("SpeedButton");

			if (speedButton is not null)
			{
				speedButton.clicked -= SpeedButtonClicked;
				speedButton.clicked += SpeedButtonClicked;
			}
			
			Button strengthButton = document.rootVisualElement.Q<Button>("StrengthButton");

			if (strengthButton is not null)
			{
				strengthButton.clicked -= StrengthButtonClicked;
				strengthButton.clicked += StrengthButtonClicked;
			}
			
			Button agilityButton = document.rootVisualElement.Q<Button>("AgilityButton");

			if (agilityButton is not null)
			{
				agilityButton.clicked -= AgilityButtonClicked;
				agilityButton.clicked += AgilityButtonClicked;
			}
			
			Label speedLvLabel = document.rootVisualElement.Q<Label>("SpeedLvLabel");

			if (speedLvLabel is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				speedLvLabel.schedule.Execute(() => 
				{
					speedLvLabel.text = $"Lvl. {PlayerController.LocalPlayer.AttackController.Stats.Speed}";
        
				}).Every(0);
			}
			
			Label strengthLvLabel = document.rootVisualElement.Q<Label>("StrengthLvLabel");

			if (strengthLvLabel is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				strengthLvLabel.schedule.Execute(() => 
				{
					strengthLvLabel.text = $"Lvl. {PlayerController.LocalPlayer.AttackController.Stats.Strength}";
        
				}).Every(0);
			}
			
			Label agilityLvLabel = document.rootVisualElement.Q<Label>("AgilityLvLabel");

			if (agilityLvLabel is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				agilityLvLabel.schedule.Execute(() => 
				{
					agilityLvLabel.text = $"Lvl. {PlayerController.LocalPlayer.AttackController.Stats.Agility}";
        
				}).Every(0);
			}
			
			VisualElement levelUpMenu = document.rootVisualElement.Q<VisualElement>("LevelUpMenu");

			if (levelUpMenu is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				levelUpMenu.schedule.Execute(() => 
				{
					levelUpMenu.visible = PlayerController.LocalPlayer.AttackController.Stats.CanLevelUp > 0;
        
				}).Every(0);
			}
			
			VisualElement reviveContainer = document.rootVisualElement.Q<VisualElement>("ReviveContainer");
			
			if (reviveContainer is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				reviveContainer.schedule.Execute(() =>
					{
						reviveContainer.visible = PlayerController.LocalPlayer.CanReviveNearPlayer;
					}
				).Every(0);
			}
			
			ProgressBar reviveProgressBar = document.rootVisualElement.Q<ProgressBar>("ReviveProgressBar");
			
			if (reviveProgressBar is not null)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				reviveProgressBar.schedule.Execute(() =>
					{
						if (PlayerController.LocalPlayer.NearestPlayer)
						{
							float value = (PlayerController.LocalPlayer.NearestPlayer.ReviveTimer / PlayerController.LocalPlayer.NearestPlayer.ReviveTime) * 100.0f;
							Debug.Log(value);
							reviveProgressBar.value = value;
						}
					}
				).Every(0);
			}
		}

		void SpeedButtonClicked()
		{
			if (!PlayerController.LocalPlayer)
			{
				return;
			}

			PlayerController.LocalPlayer.AttackController.Stats.LevelUpProperty(nameof(CharacterStats.Speed));
		}
		void StrengthButtonClicked()
		{
			if (!PlayerController.LocalPlayer)
			{
				return;
			}

			PlayerController.LocalPlayer.AttackController.Stats.LevelUpProperty(nameof(CharacterStats.Strength));
		}
		void AgilityButtonClicked()
		{
			if (!PlayerController.LocalPlayer)
			{
				return;
			}

			PlayerController.LocalPlayer.AttackController.Stats.LevelUpProperty(nameof(CharacterStats.Agility));
		}
		void PlayButtonClicked() => UpdateVisualTree(PlayMenu);
		void SettingsButtonClicked() => UpdateVisualTree(OptionsMenu);
		void BackToMainMenuButtonClicked() => UpdateVisualTree(MainMenu);
		void BackToSearchMenuButtonClicked()
		{
			((GameManager)NetworkManager.singleton).LeaveRoom();
			UpdateVisualTree(PlayMenu);
		}
		void CreateRoomButtonClicked() => ((GameManager)NetworkManager.singleton).CreateRoom();
		void ReadyButtonClicked() => GameManager.Ready();
		void BackToLobbyButtonClicked()
		{
			((GameManager)NetworkManager.singleton).LeaveRoom();
			UpdateVisualTree(PlayMenu);
		}

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
		public void OpenMainMenu() => UpdateVisualTree(MainMenu);
		public void OpenGameMenu() => UpdateVisualTree(GameMenu);
		public void OpenRoomMenu() => UpdateVisualTree(RoomMenu);
		public void HideUI() => UpdateVisualTree(null);
	}
}
