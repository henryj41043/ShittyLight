using UnityEngine;
using MLAPI;
using TMPro;
using MLAPI.Messaging;

namespace ShittyLight.Lobby.Networking
{
    public class PlayerNameTag : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;

        public override void NetworkStart()
        {
            base.NetworkStart();
            if (IsLocalPlayer)
            {
                SetPlayerNameServerRpc(PlayerPrefs.GetString("PlayerName", "No Player Name :("));
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerNameServerRpc(string name)
        {
            SetPlayerNameClientRpc(name);
        }

        [ClientRpc]
        private void SetPlayerNameClientRpc(string name)
        {
            if (IsLocalPlayer) { return; }
            nameText.text = name;
        }
    }
}