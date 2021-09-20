using UnityEngine;
using System.Collections;
using MLAPI;

namespace ShittyLight.Lobby.Networking
{
	public class BulletScript : NetworkBehaviour
	{

		[Range(5, 100)]
		[Tooltip("After how long time should the bullet prefab be destroyed?")]
		public float destroyAfter;

		[Header("Impact Effect Prefabs")]
		public NetworkObject[] bloodImpactPrefabs;
		public NetworkObject[] metalImpactPrefabs;
		public NetworkObject[] dirtImpactPrefabs;
		public NetworkObject[] concreteImpactPrefabs;

		private void Start()
		{
			StartCoroutine(DestroyAfter());
		}

		private void OnCollisionEnter(Collision collision)
		{
			Vector3 normal = collision.GetContact(0).normal;
            switch (collision.gameObject.tag)
            {
				case "Player":
					collision.transform.GetComponent<PlayerHealth>().TakeDamage(10);
					NetworkObject playerImpact = Instantiate(bloodImpactPrefabs[Random.Range(0, bloodImpactPrefabs.Length)], transform.position, Quaternion.LookRotation(normal));
					playerImpact.Spawn();
					break;
				case "Blood":
					NetworkObject bloodImpact = Instantiate(bloodImpactPrefabs[Random.Range(0, bloodImpactPrefabs.Length)], transform.position, Quaternion.LookRotation(normal));
					bloodImpact.Spawn();
					break;
				case "Metal":
					NetworkObject metalImpact = Instantiate(metalImpactPrefabs[Random.Range(0, metalImpactPrefabs.Length)], transform.position, Quaternion.LookRotation(normal));
					metalImpact.Spawn();
					break;
				case "Dirt":
					NetworkObject dirtImpact = Instantiate(dirtImpactPrefabs[Random.Range(0, dirtImpactPrefabs.Length)], transform.position, Quaternion.LookRotation(normal));
					dirtImpact.Spawn();
					break;
				case "Concrete":
					NetworkObject concreteImpact = Instantiate(concreteImpactPrefabs[Random.Range(0, concreteImpactPrefabs.Length)], transform.position, Quaternion.LookRotation(normal));
					concreteImpact.Spawn();
					break;
			}

			Destroy(gameObject);
		}

		private IEnumerator DestroyAfter()
		{
			yield return new WaitForSeconds(destroyAfter);
			Destroy(gameObject);
		}
	}
}