using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using System.Collections;

namespace ShittyLight.Lobby.Networking
{
    public class PlayerShooting : NetworkBehaviour
    {
        [SerializeField] public ParticleSystem bulletParticleSystem;
        private ParticleSystem.EmissionModule em;
        NetworkVariableBool shooting = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly }, false);
        public Transform gunBarrel;
        private bool canShoot = true;
        public float fireRate = 0.1f;
        private Transform mainCameraTransform;

        void Start()
        {
            em = bulletParticleSystem.emission;
            mainCameraTransform = Camera.main.transform;
        }

        void Update()
        {
            if (IsLocalPlayer)
            {
                Vector3 aimSpot = mainCameraTransform.position + mainCameraTransform.forward * 30f;
                gunBarrel.LookAt(aimSpot);
                shooting.Value = Input.GetMouseButton(0);
                if (shooting.Value && canShoot)
                {
                    StartCoroutine(FireRate(fireRate));
                }
            }
            em.rateOverTime = shooting.Value ? 10f : 0f;
        }

        IEnumerator FireRate(float fireRate)
        {
            canShoot = false;
            ShootServerRpc();
            yield return new WaitForSeconds(fireRate);
            canShoot = true;
        }

        [ServerRpc]
        void ShootServerRpc()
        {
            if (Physics.Raycast(gunBarrel.position, gunBarrel.forward, out RaycastHit hit, 200f))
            {
                var enemyHealth = hit.transform.GetComponent<PlayerHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(10);
                }
            }
        }
    }
}