using MLAPI;
using UnityEngine;

public class NetworkCameraController : NetworkBehaviour
{
    //[SerializeField] private new Camera gunCamera;
    [SerializeField] private new Camera mainCamera;
    [SerializeField] private AudioListener audioListener;

    public override void NetworkStart()
    {
        base.NetworkStart();
        if (IsLocalPlayer)
        {
            mainCamera.enabled = true;
            //gunCamera.enabled = true;
            audioListener.enabled = true;
        }
    }
}
