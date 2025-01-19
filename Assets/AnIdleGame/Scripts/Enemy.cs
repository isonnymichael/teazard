﻿using UnityEngine;
using System.Collections;

//script controls the enemy
public class Enemy : MonoBehaviour {
	
	public GameObject damagePopup; //a reference to the damagePopup prefab
	public Transform PopupSpawnPoint; //the parent object
	public int enemyID;
	public AudioClip damageClip;
	public AudioClip deathClip;

	private int maxHealth; //the max health
	private int loot; //how much money the player will get after killing this enemy
	private int health; //the current health
	private HealthBar healthBar; //reference to the healthBar script on this enemy
	private Animator anim;
	private ParticleSystem deathParticle; //particle will play after death
	private ParticleSystem hitParticle; //particle will play after hit
	private GameObject canvas; //the canvas used to show health bar and damage popup text
	private AudioSource audioSource;
	private CameraShake cameraShake;

	private float kecepatan = 5f;
	private float tempKecepatan;
    private float jarak = 300f;
	[HideInInspector] public Vector3 awalPosisi;

	private GameObject masterMonster;

	void Awake ()
	{
		healthBar = GetComponentInChildren<HealthBar> ();

		deathParticle = transform.Find("DeathParticle").GetComponent<ParticleSystem>();
		hitParticle = transform.Find("DamageParticle").GetComponent<ParticleSystem>();

		canvas = GetComponentInChildren<Canvas> ().gameObject;

		audioSource = GetComponent<AudioSource>();
		cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
	}

	void Start ()
	{
		
	}

	private IEnumerator Gerakan()
    {
        float posisiSekarang = transform.position.x;
		float ySekarang = transform.position.y;
        float batasKanan = awalPosisi.x - jarak;

        while (masterMonster != null)
        {
            posisiSekarang -= kecepatan * Time.deltaTime;

            if (posisiSekarang < batasKanan)
            {
                kecepatan = 0;
            }

            transform.position = new Vector3(posisiSekarang, ySekarang, transform.position.z);

            yield return null;
        }
    }

	public void TakeDamage (int damage) //function get called if the player attacks this enemy
	{
		if (!anim) //see if ther is no monster prefab under this enemy object. this should never happen
		{
			Debug.Log ("Need to reset monster");

			return;
		}

		health -= damage;

		if (health <= 0) //if the current health is equal to or smaller than 0, this enmey should die
		{
			StopCoroutine ("Gerakan");

			tempKecepatan = kecepatan;

			health = 0; //change the value to 0, make sure it's not negative. we will used this value to calculate health ratio

			Destroy (anim.gameObject);

			deathParticle.Play (); //play death partivle
			audioSource.PlayOneShot(deathClip);
			cameraShake.shakeDuration = 0.3f;

			canvas.SetActive (false); //hide the canvas

			PlayerStats.Instance.TempMoney += loot; //give the player the reward
			if (Application.platform == RuntimePlatform.WebGLPlayer) {
				FirebaseDatabase.getTempMoney(Web3Auth.Instance.addressWallet, PlayerStats.Instance.TempMoney);
			}
			
			GameManager.Instance.aliveEnemies.Remove (enemyID); //remove this enmey from the enemy list
			masterMonster = null;


			// StartCoroutine ("ResetPos");
		}

		float healthRatio = (float)health / maxHealth; //calculate the health ratio, this value will be used by HealthBar script. 

		GameObject popupInstance = Instantiate (damagePopup); //create the damage popup text

		popupInstance.transform.SetParent (PopupSpawnPoint, false);

		popupInstance.GetComponent<DamagePopup> ().Show (damage); //call the function in DamagePupup script

		Destroy (popupInstance, 1f); //destory the damage popup after 1 second

		healthBar.Change (healthRatio); //call the function in HealthBar script to update the health bar

		StartCoroutine ("DamageAnim");
		
	}

	private IEnumerator ResetPos () //function to make the attack
	{		
		yield return new WaitForSeconds(1f);
		transform.position = new Vector3(awalPosisi.x, transform.position.y, transform.position.z);
	}

	private IEnumerator DamageAnim () //function to make the attack
	{
		tempKecepatan = kecepatan;
		hitParticle.Play ();
		if(!audioSource.isPlaying){
			audioSource.PlayOneShot(damageClip);
		}

		kecepatan = 1f;
		anim.SetTrigger ("Damage"); //trigger the animation when the enemy gets attacked
		AnimatorStateInfo animasiStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		float durasiAnimasi = animasiStateInfo.length;
		
		yield return new WaitForSeconds(durasiAnimasi);
		kecepatan = tempKecepatan;
	}

	public void SpawnMonster (int level) //function used to spawn monster prefab
	{
		int randomX = UnityEngine.Random.Range(20, 50);
		int randomY = UnityEngine.Random.Range(10, 35);
		transform.position = new Vector3(awalPosisi.x + randomX, awalPosisi.y + randomY, transform.position.z);

		if (anim) //if this enmey doesn't get killed in the last round, we destroy the old one
			Destroy (anim.gameObject); //destroy the old monster prefab

		maxHealth = (int)(100 * Mathf.Pow (1.2f, level)); //formula to determine the health of emeny

		loot = level; //formula to determine the amount of the reward

		health = maxHealth; //set current health to full
		
		healthBar.Reset (); //reset the health bar
		
		GameObject monster = (GameObject)Instantiate (GameManager.Instance.monsterPrefabs [Random.Range (0, GameManager.Instance.monsterPrefabs.Length)]); //randomly create a monster prefab
		masterMonster = monster;

		monster.transform.SetParent (transform);
		monster.transform.localPosition = Vector2.zero;
		canvas.SetActive (true); //show canvas again
		anim = monster.GetComponent<Animator> (); //set the reference to the animator on the newly created monster prefab

		kecepatan = UnityEngine.Random.Range(3, 5);
		tempKecepatan = kecepatan;

		StartCoroutine ("Gerakan");
	}
}
