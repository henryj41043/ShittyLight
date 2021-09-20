using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using UnityEngine.UI;

namespace ShittyLight.Lobby.Networking
{
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] NetworkVariableInt health = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly }, 100);
        private bool isAlive = true;
        public int maxHealth = 100;
        public Slider healthSlider;
        private Renderer[] renderers;
        private GameObject playerSpawnSystem;
        public Behaviour[] scripts;
        public Behaviour characterControllerScript;
        public ParticleSystem deathParticleSystem;
        public Canvas nameTagCanvas;
        private CharacterController characterController;
        private Behaviour playerShootingScript;
        private CapsuleCollider collider;

        private void Start()
        {
            healthSlider.maxValue = maxHealth;
            renderers = GetComponentsInChildren<Renderer>();
            characterController = GetComponent<CharacterController>();
            collider = GetComponent<CapsuleCollider>();
            playerShootingScript = GetComponentInChildren<AutomaticGunScriptLPFP>();
            playerSpawnSystem = GameObject.Find("PlayerSpawnSystem");
            if (IsLocalPlayer)
            {
                nameTagCanvas.enabled = false;
                characterControllerScript.enabled = true;
            }
        }

        private void Update()
        {
            healthSlider.value = health.Value;
            if (IsLocalPlayer && health.Value <= 0 && isAlive)
            {
                isAlive = false;
                characterControllerScript.enabled = false;
                playerShootingScript.enabled = false;
                characterController.enabled = false;
                collider.enabled = false;
                Die();
            }
            if (IsLocalPlayer && !isAlive && Input.GetKeyDown(KeyCode.Alpha1))
            {
                health.Value = maxHealth;
                isAlive = true;
                characterController.enabled = true;
                collider.enabled = true;
                Respawn();
                characterControllerScript.enabled = true;
                playerShootingScript.enabled = true;
            }
        }

        public void TakeDamage(int damage)
        {                            
            health.Value -= damage;
        }

        public void Die()
        {
            DieServerRpc();
        }

        [ServerRpc]
        void DieServerRpc()
        {
            DieClientRpc();
        }

        [ClientRpc]
        void DieClientRpc()
        {
            SetPlayerState(false);
            Instantiate(deathParticleSystem, transform.position, transform.rotation);
        }

        public void Respawn()                    
        {
            RespawnServerRpc();
        }

        [ServerRpc]
        void RespawnServerRpc()
        {
            Transform spawnPoint = playerSpawnSystem.GetComponent<PlayerSpawnSystem>().getRandomSpawnPoint();
            RespawnClientRpc(spawnPoint.position, spawnPoint.rotation);
        }

        [ClientRpc]
        void RespawnClientRpc(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            SetPlayerState(true);
        }

        void SetPlayerState(bool state)
        {
            if (!IsLocalPlayer) { nameTagCanvas.enabled = state; }
            foreach (var renderer in renderers)
            {
                renderer.enabled = state;
            }
            foreach (var script in scripts)
            {
                script.enabled = state;
            }
        }
    }
}
