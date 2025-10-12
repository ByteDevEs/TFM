using Mirror;
using UI;
namespace Lobby
{
    public class CustomNetworkRoomPlayer : NetworkRoomPlayer
    {
        [ClientRpc]
        public void OnClientPlayersReady()
        {
            UIDocumentController.GetInstance().OpenGameMenu();
        }
    }
}
