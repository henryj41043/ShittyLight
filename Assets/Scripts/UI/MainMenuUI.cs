using MLAPI;
using MLAPI.Transports.UNET;
using ShittyLight.Lobby.Networking;
using TMPro;
using UnityEngine;

namespace ShittyLight.Lobby
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_InputField displayNameInputField;
        [SerializeField] private TMP_InputField ipAddressInputField;

        private void Start()
        {
            //TODO: if server build, start server...
            PlayerPrefs.GetString("PlayerName");
        }

        public void OnHostClicked()
        {
            PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
            GameNetPortal.Instance.StartHost();
        }

        public void OnClientClicked()
        {
            PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
            if(ipAddressInputField.text == null || ipAddressInputField.text.Length <= 0)
            {
                NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "127.0.0.1";
            }
            else
            {
                NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ipAddressInputField.text;
            }
            ClientGameNetPortal.Instance.StartClient();
        }
    }
}

