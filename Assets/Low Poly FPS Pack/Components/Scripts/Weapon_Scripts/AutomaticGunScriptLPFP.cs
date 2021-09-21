using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;

public class AutomaticGunScriptLPFP : NetworkBehaviour {

	Animator anim;

	[Header("Gun Camera")]
	public Camera gunCamera;

	[Header("Gun Camera Options")]
	[Tooltip("How fast the camera field of view changes when aiming.")]
	public float fovSpeed = 15.0f;
	[Tooltip("Default value for camera field of view (40 is recommended).")]
	public float defaultFov = 40.0f;

	[Header("Weapon Attachments (Only use one scope attachment)")]
	public bool ironSights;
	public bool alwaysShowIronSights;
	[Range(5, 40)]
	public float ironSightsAimFOV = 25;
	
	[System.Serializable]
	public class weaponAttachmentRenderers 
	{
		public SkinnedMeshRenderer ironSightsRenderer;
	}
	public weaponAttachmentRenderers WeaponAttachmentRenderers;

	[Header("Weapon Sway")]
	[Tooltip("Toggle weapon sway.")]
	public bool weaponSway;

	public float swayAmount = 0.02f;
	public float maxSwayAmount = 0.06f;
	public float swaySmoothValue = 4.0f;

	private float lastFired;
	[Header("Weapon Settings")]
	[Tooltip("How fast the weapon fires, higher value means faster rate of fire.")]
	public float fireRate;
	[Tooltip("Enables auto reloading when out of ammo.")]
	public bool autoReload;
	public float autoReloadDelay;
	private bool isReloading;

	private bool hasBeenHolstered = false;
	private bool holstered;
	private bool isRunning;
	private bool isAiming;
	private bool isInspecting;

	private int currentAmmo;
	[Tooltip("How much ammo the weapon should have.")]
	public int ammo;
	private bool outOfAmmo;

	[Header("Bullet Settings")]
	[Tooltip("How much force is applied to the bullet when shooting.")]
	public float bulletForce = 400.0f;
	[Tooltip("How long after reloading that the bullet model becomes visible " +
		"again, only used for out of ammo reload animations.")]
	public float showBulletInMagDelay = 0.6f;
	[Tooltip("The bullet model inside the mag, not used for all weapons.")]
	public SkinnedMeshRenderer bulletInMagRenderer;

	[Header("Grenade Settings")]
	public float grenadeSpawnDelay = 0.35f;

	[Header("Muzzleflash Settings")]
	public bool enableMuzzleflash = true;
	public ParticleSystem muzzleParticles;
	public bool enableSparks = true;
	public ParticleSystem sparkParticles;
	public int minSparkEmission = 1;
	public int maxSparkEmission = 5;

	[Header("Muzzleflash Light Settings")]
	public Light muzzleflashLight;
	public float lightDuration = 0.02f;

	[Header("Audio Source")]
	public AudioSource mainAudioSource;
	public AudioSource shootAudioSource;

	[Header("UI Components")]
	public Image crosshair;
	public Text currentAmmoText;
	public Text totalAmmoText;

	[System.Serializable]
	public class prefabs
	{  
		[Header("Prefabs")]
		public NetworkObject bulletPrefab;
		public Transform casingPrefab;
		public NetworkObject grenadePrefab;
	}
	public prefabs Prefabs;
	
	[System.Serializable]
	public class spawnpoints
	{  
		[Header("Spawnpoints")]
		public Transform casingSpawnPoint;
		public Transform bulletSpawnPoint;
		public Transform grenadeSpawnPoint;
	}
	public spawnpoints Spawnpoints;

	[System.Serializable]
	public class soundClips
	{
		public AudioClip shootSound;
		public AudioClip silencerShootSound;
		public AudioClip takeOutSound;
		public AudioClip holsterSound;
		public AudioClip reloadSoundOutOfAmmo;
		public AudioClip reloadSoundAmmoLeft;
		public AudioClip aimSound;
	}
	public soundClips SoundClips;

	private bool soundHasPlayed = false;

    public override void NetworkStart()
    {
        base.NetworkStart();

		if (IsLocalPlayer)
		{
			anim = GetComponent<Animator>();
			currentAmmo = ammo;
		}

		muzzleflashLight.enabled = false;

		if (alwaysShowIronSights == true && WeaponAttachmentRenderers.ironSightsRenderer != null) {
			WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
			<SkinnedMeshRenderer> ().enabled = true;
		}

		if (ironSights == true && WeaponAttachmentRenderers.ironSightsRenderer != null) 
		{
			WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
			<SkinnedMeshRenderer> ().enabled = true;
		} else if (!alwaysShowIronSights && 
			WeaponAttachmentRenderers.ironSightsRenderer != null) {
			WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
			<SkinnedMeshRenderer> ().enabled = false;
		}
		
		if (IsLocalPlayer)
		{
			totalAmmoText.text = ammo.ToString();
		}

		shootAudioSource.clip = SoundClips.shootSound;
	}
	
	private void Update () {
		if (!IsLocalPlayer) { return; }

		if (isAiming)
        {
			crosshair.enabled = false;
		}
        else
        {
			crosshair.enabled = true;
        }

		//Aiming
		//Toggle camera FOV when right click is held down
		if(Input.GetButton("Fire2") && !isReloading && !isRunning && !isInspecting) 
		{
			if (ironSights == true) 
			{
				gunCamera.fieldOfView = Mathf.Lerp (gunCamera.fieldOfView,
					ironSightsAimFOV, fovSpeed * Time.deltaTime);
			}

			isAiming = true;

			//If iron sights are enabled, use normal aim
			if (ironSights == true) 
			{
				anim.SetBool ("Aim", true);
			}

			if (!soundHasPlayed) 
			{
				// TODO: Should this sound play for all clients..?
				mainAudioSource.clip = SoundClips.aimSound;
				mainAudioSource.Play ();
	
				soundHasPlayed = true;
			}
		} 
		else 
		{
			//When right click is released
			gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
				defaultFov,fovSpeed * Time.deltaTime);

			isAiming = false;

			//If iron sights are enabled, use normal aim out
			if (ironSights == true) 
			{
				anim.SetBool ("Aim", false);
			}

			soundHasPlayed = false;
		}
		//Aiming end

		//Set current ammo text from ammo int
		currentAmmoText.text = currentAmmo.ToString ();

		//Continosuly check which animation is currently playing
		AnimationCheck ();

		//Play knife attack 1 animation when Q key is pressed
		if (Input.GetKeyDown (KeyCode.Q) && !isInspecting) 
		{
			anim.Play ("Knife Attack 1", 0, 0f);
			// TODO: if knife hits a player... do damage
		}
		//Play knife attack 2 animation when F key is pressed
		if (Input.GetKeyDown (KeyCode.F) && !isInspecting) 
		{
			anim.Play ("Knife Attack 2", 0, 0f);
			// TODO: if knife hits a player... do damage
		}

		//Throw grenade when pressing G key
		if (Input.GetKeyDown (KeyCode.G) && !isInspecting) 
		{
			StartCoroutine (GrenadeSpawnDelay ());
			anim.Play("GrenadeThrow", 0, 0.0f);
		}

		if (currentAmmo == 0) 
		{
			outOfAmmo = true;
			if (autoReload == true && !isReloading) 
			{
				StartCoroutine (AutoReload ());
			}
		} 
		else 
		{
			outOfAmmo = false;
		}
			
		//AUtomatic fire
		//Left click hold 
		if (Input.GetMouseButton (0) && !outOfAmmo && !isReloading && !isInspecting && !isRunning) 
		{
			//Shoot automatic
			if (Time.time - lastFired > 1 / fireRate) 
			{
				lastFired = Time.time;
				currentAmmo -= 1;
				if (!isAiming)
				{
					anim.Play ("Fire", 0, 0f);
				} 
				else
				{
					if (ironSights == true) 
					{
						anim.Play ("Aim Fire", 0, 0f);
					}
				}
				StartCoroutine(Shoot());
			}
		}

		if (Input.GetKeyDown (KeyCode.T)) 
		{
			anim.SetTrigger ("Inspect");
		}

		if (Input.GetKeyDown (KeyCode.E) && !hasBeenHolstered) 
		{
			holstered = true;

			mainAudioSource.clip = SoundClips.holsterSound;
			mainAudioSource.Play();

			hasBeenHolstered = true;
		} 
		else if (Input.GetKeyDown (KeyCode.E) && hasBeenHolstered) 
		{
			holstered = false;

			mainAudioSource.clip = SoundClips.takeOutSound;
			mainAudioSource.Play ();

			hasBeenHolstered = false;
		}

		if (holstered == true) 
		{
			anim.SetBool ("Holster", true);
		} 
		else 
		{
			anim.SetBool ("Holster", false);
		}

		if (Input.GetKeyDown (KeyCode.R) && !isReloading && !isInspecting) 
		{
			Reload ();
		}

		if (Input.GetKey (KeyCode.W) && !isRunning || 
			Input.GetKey (KeyCode.A) && !isRunning || 
			Input.GetKey (KeyCode.S) && !isRunning || 
			Input.GetKey (KeyCode.D) && !isRunning) 
		{
			anim.SetBool ("Walk", true);
		} else {
			anim.SetBool ("Walk", false);
		}

		if ((Input.GetKey (KeyCode.W) && Input.GetKey (KeyCode.LeftShift))) 
		{
			isRunning = true;
		} else {
			isRunning = false;
		}
		
		if (isRunning == true) 
		{
			anim.SetBool ("Run", true);
		} 
		else 
		{
			anim.SetBool ("Run", false);
		}
	}

	private IEnumerator Shoot()
    {
		ShootServerRpc();
		yield break;
	}

	[ServerRpc]
	private void ShootServerRpc()
    {
		NetworkObject bullet = Instantiate(
			Prefabs.bulletPrefab,
			Spawnpoints.bulletSpawnPoint.transform.position,
			Spawnpoints.bulletSpawnPoint.transform.rotation);
		bullet.GetComponent<Rigidbody>().velocity =
			bullet.transform.forward * bulletForce;
		bullet.Spawn();

		ShootClientRpc();
	}

	[ClientRpc]
	private void ShootClientRpc()
    {
		Instantiate(Prefabs.casingPrefab,
			Spawnpoints.casingSpawnPoint.transform.position,
			Spawnpoints.casingSpawnPoint.transform.rotation);
		shootAudioSource.clip = SoundClips.shootSound;
		shootAudioSource.Play();
        if (enableMuzzleflash)
        {
			muzzleParticles.Emit(1);
			StartCoroutine(MuzzleFlashLight());
		}
        if (enableSparks)
        {
			sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
		}
	}

	private IEnumerator GrenadeSpawnDelay () {
		yield return new WaitForSeconds (grenadeSpawnDelay);
		ThrowGrenadeServerRpc();
	}

	[ServerRpc]
	private void ThrowGrenadeServerRpc()
    {
		NetworkObject grenade = Instantiate(Prefabs.grenadePrefab,
			Spawnpoints.grenadeSpawnPoint.transform.position,
			Spawnpoints.grenadeSpawnPoint.transform.rotation);

		grenade.Spawn();
	}

	private IEnumerator AutoReload () {
		yield return new WaitForSeconds (autoReloadDelay);

		if (outOfAmmo == true) 
		{
			anim.Play ("Reload Out Of Ammo", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
			mainAudioSource.Play ();

			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = false;
				StartCoroutine (ShowBulletInMag ());
			}
		} 
		currentAmmo = ammo - 1;
		outOfAmmo = false;
	}

	private void Reload () {
		if(currentAmmo == ammo) { return; }
		
		if (outOfAmmo == true) 
		{
			anim.Play ("Reload Out Of Ammo", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
			mainAudioSource.Play ();

			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = false;
				StartCoroutine (ShowBulletInMag ());
			}
			currentAmmo = ammo - 1;
		} 
		else 
		{
			anim.Play ("Reload Ammo Left", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundAmmoLeft;
			mainAudioSource.Play ();

			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = true;
			}

			currentAmmo = ammo;
		}
		outOfAmmo = false;
	}

	private IEnumerator ShowBulletInMag () {
		yield return new WaitForSeconds (showBulletInMagDelay);
		bulletInMagRenderer.GetComponent<SkinnedMeshRenderer> ().enabled = true;
	}

	private IEnumerator MuzzleFlashLight () {
		muzzleflashLight.enabled = true;
		yield return new WaitForSeconds (lightDuration);
		muzzleflashLight.enabled = false;
	}

	private void AnimationCheck () {
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Out Of Ammo") || 
			anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Ammo Left")) 
		{
			isReloading = true;
		} 
		else 
		{
			isReloading = false;
		}

		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Inspect")) 
		{
			isInspecting = true;
		} 
		else 
		{
			isInspecting = false;
		}
	}
}