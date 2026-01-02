using System.Collections.Generic;
using System.Linq;
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

		public UIDocument Document { get; private set; }
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
			if (!instance)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}

			Document = GetComponent<UIDocument>();
			serversFound = new Dictionary<long, ServerResponse>();
			UpdateVisualTree(MainMenu);
		}

		void UpdateVisualTree(VisualTreeAsset asset)
		{
			Document.visualTreeAsset = asset;
			RefreshButtons();
			RegisterButtonSounds();
		}

		void RegisterButtonSounds()
		{
			List<Button> allButtons = Document.rootVisualElement.Query<Button>().ToList();

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
			if (Document.rootVisualElement.Q<Button>("PlayButton") is {} playButton)
			{
				playButton.clicked -= PlayButtonClicked;
				playButton.clicked += PlayButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("SettingsButton") is {} settingsButton)
			{
				settingsButton.clicked -= SettingsButtonClicked;
				settingsButton.clicked += SettingsButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("QuitButton") is {} quitButton)
			{
				quitButton.clicked -= QuitButtonClicked;
				quitButton.clicked += QuitButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("BackToMainMenuButton") is {} backToMainMenuButton)
			{
				backToMainMenuButton.clicked -= BackToMainMenuButtonClicked;
				backToMainMenuButton.clicked += BackToMainMenuButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("BackToSearchMenuButton") is {} backToSearchMenuButton)
			{
				backToSearchMenuButton.clicked -= BackToSearchMenuButtonClicked;
				backToSearchMenuButton.clicked += BackToSearchMenuButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("CreateRoomButton") is {} createRoomButton)
			{
				createRoomButton.clicked -= CreateRoomButtonClicked;
				createRoomButton.clicked += CreateRoomButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("ReadyButton") is {} readyButton)
			{
				readyButton.clicked -= ReadyButtonClicked;
				readyButton.clicked += ReadyButtonClicked;
			}

			if (Document.rootVisualElement.Q<ListView>("ServerListView") is {} container)
			{
				container.hierarchy.Clear();

				foreach (KeyValuePair<long, ServerResponse> serverResponse in serversFound)
				{
					TemplateContainer roomFound = RoomButtonTemplate.Instantiate();
					
					Button button = roomFound.Q<Button>("ServerFoundButton");
					if (button != null)
					{
						if (button.Children().First() is Label label)
						{
							label.text = $"Join {serverResponse.Value.uri.Host}";
						}
						button.clickable = new Clickable(() => NetworkManager.singleton.StartClient(serverResponse.Value.uri));
					}
            
					container.hierarchy.Add(roomFound);
				}
			}

			if (Document.rootVisualElement.Q<VisualElement>("HealthMask") is {} healthMask)
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

			if (Document.rootVisualElement.Q<RadialProgress>("PotionCooldownProgress") is {} potionCooldownProgress)
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

			if (Document.rootVisualElement.Q<Label>("PotionCountLabel") is {} potionCountLabel)
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

			if (Document.rootVisualElement.Q<Button>("BackToLobbyButton") is {} backToLobbyButton)
			{
				backToLobbyButton.clicked -= BackToLobbyButtonClicked;
				backToLobbyButton.clicked += BackToLobbyButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("SpeedButton") is {} speedButton)
			{
				speedButton.clicked -= SpeedButtonClicked;
				speedButton.clicked += SpeedButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("StrengthButton") is {} strengthButton)
			{
				strengthButton.clicked -= StrengthButtonClicked;
				strengthButton.clicked += StrengthButtonClicked;
			}

			if (Document.rootVisualElement.Q<Button>("MaxHealthButton") is {} maxHealthButton)
			{
				maxHealthButton.clicked -= MaxHealthButtonClicked;
				maxHealthButton.clicked += MaxHealthButtonClicked;
			}

			if (Document.rootVisualElement.Q<Label>("SpeedLvLabel") is {} speedLvLabel)
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

			if (Document.rootVisualElement.Q<Label>("StrengthLvLabel") is {} strengthLvLabel)
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

			if (Document.rootVisualElement.Q<Label>("MaxHealthLvLabel") is {} maxHealthLvLabel)
			{
				if (!PlayerController.LocalPlayer)
				{
					return;
				}

				maxHealthLvLabel.schedule.Execute(() => 
				{
					maxHealthLvLabel.text = $"Lvl. {PlayerController.LocalPlayer.AttackController.Stats.Health}";
        
				}).Every(0);
			}

			if (Document.rootVisualElement.Q<VisualElement>("LevelUpMenu") is {} levelUpMenu)
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

			if (Document.rootVisualElement.Q<VisualElement>("ReviveContainer") is {} reviveContainer)
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

			if (Document.rootVisualElement.Q<ProgressBar>("ReviveProgressBar") is {} reviveProgressBar)
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

			if (Document.rootVisualElement.Q<Slider>("MusicSlider") is {} musicSlider)
			{
				musicSlider.value = Settings.GetInstance().MusicVolume;
				musicSlider.schedule.Execute(() => {
					Settings.GetInstance().MusicVolume = musicSlider.value;
					PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
				}).Every(0);
			}

			if (Document.rootVisualElement.Q<Slider>("SFXSlider") is {} sfxSlider)
			{
				sfxSlider.value = Settings.GetInstance().SfxVolume;
				sfxSlider.schedule.Execute(() => {
					Settings.GetInstance().SfxVolume = sfxSlider.value;
					PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
				}).Every(0);
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
		void MaxHealthButtonClicked()
		{
			if (!PlayerController.LocalPlayer)
			{
				return;
			}

			PlayerController.LocalPlayer.AttackController.Stats.LevelUpProperty(nameof(CharacterStats.Health));
		}
		void PlayButtonClicked() => UpdateVisualTree(PlayMenu);
		void SettingsButtonClicked() => UpdateVisualTree(OptionsMenu);
		void QuitButtonClicked() => Application.Quit();
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

			ListView container = Document.rootVisualElement.Q<ListView>("ServerListView");
			serversFound.Add(response.serverId, response);
			if (container is null)
			{
				return;
			}

			TemplateContainer roomFound = RoomButtonTemplate.Instantiate();
					
			Button button = roomFound.Q<Button>("ServerFoundButton");
			if (button != null)
			{
				if (button.Children().First() is Label label)
				{
					label.text = $"Join {response.uri.Host}";
				}
				button.clickable = new Clickable(() => NetworkManager.singleton.StartClient(response.uri));
			}
            
			container.hierarchy.Add(roomFound);
		}
		public void OpenMainMenu() => UpdateVisualTree(MainMenu);
		public void OpenGameMenu() => UpdateVisualTree(GameMenu);
		public void OpenRoomMenu() => UpdateVisualTree(RoomMenu);
		public void HideUI() => UpdateVisualTree(null);
	}
}
