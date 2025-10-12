using Lobby;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIDocumentController : MonoBehaviour
    {
        static UIDocumentController _instance;
        public static UIDocumentController GetInstance() => _instance;
        
        
        UIDocument document;

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
            if (_instance is null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
            document = GetComponent<UIDocument>();
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
        
        
        public void OpenMainMenu() => UpdateVisualTree(mainMenu);
        public void OpenGameMenu() => UpdateVisualTree(gameMenu);
        public void OpenRoomMenu() => UpdateVisualTree(roomMenu);
    }
}
