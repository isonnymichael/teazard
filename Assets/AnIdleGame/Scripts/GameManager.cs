using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Thirdweb;
using Newtonsoft.Json;

//script to control the game
public class GameManager : MonoBehaviour {

	//Singleton
	private static GameManager instance;

	public static GameManager Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.Find("GameManager").GetComponent<GameManager> ();

			return instance;
		}
	}

	public GameObject[] monsterPrefabs; //a list of monster prefabs
	public Enemy enemyPrefab; //the enemy prefab
	public Transform Enemies; //the parent object of enemy
	public int timeLimit; //the time limit of each round
	public Player player; //reference to the Player script
	public float nextRoundDelay; //seconds delay before starting next round

	public Timer timer;
	
	[HideInInspector] public List<Enemy> enemies; //a list of enemy scripts
	[HideInInspector] public List<int> aliveEnemies; //a list of alive enmeies
	[HideInInspector] public bool isBattling;
	public ActiveSkill[] activeSkills; //the current using active skill
	public PlayerData playerData; //the selected active skill in active skill selecting window

	private int level; //the monster level
	private int countdown; //the number shows on top-left

	public int Level
	{
		get
		{
			return level;
		}
		set
		{
			level = value;

			HUD.Instance.levelText.text = "Enemy Level: " + level.ToString (); //update the Level text on bottom-right
		}
	}

	void Awake ()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			if (this != instance)
				Destroy (this.gameObject);
		}

		activeSkills = GameObject.Find ("HUD/AbilityWindow/Abilities").GetComponentsInChildren<ActiveSkill> (); //get the list of ActiveSkill scripts
		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			PlayerDataStatic.playerData.money = Convert.ToInt32(Web3Auth.Instance.displayValueToken.Split('.')[0]);
			playerData = PlayerDataStatic.playerData;
		}else{
			playerData.Reset();
		}
		
	}

	void Start ()
	{
		enemies = new List<Enemy> ();
		for (int i = 0; i<3; i++) //create 3*3 enemy prefabs
		{
			for (int j=0; j<3; j++)
			{
				Enemy enemy = (Enemy)Instantiate (enemyPrefab);

				enemies.Add(enemy);

				enemy.transform.SetParent (Enemies);

				enemy.transform.localPosition = new Vector2 (i * 50 + 100, j * 35 - 45);
				enemy.awalPosisi = enemy.transform.localPosition;

				enemy.transform.name = "Monster (" + i + ", " + j + ") ";

				enemy.enemyID = i * 3 + j;
			}
		}

		Level = playerData.level; //load the level

		StartBattle (); //when everything is ready, start the fight
		UnlockSkillCheckStart();
	}

	private void StartBattle ()
	{
		HUD.Instance.panelStatusBattle.SetActive(false);

		SpawnEnemies ();

		isBattling = true;

		StartCoroutine ("StartCountDown"); //start the time limit count down

		player.StartAttack ();
	}

	public void StopBattle (bool currentLevelCompleted = false)
	{
		player.StopAttack ();

		StopCoroutine ("StartCountDown");

		isBattling = false;
		timer.timerRunning = false;

		if (currentLevelCompleted) //if the battle is stopped because all enemies have been killed, we increase the monster level by 1 and check if there is any new skills to unlock
		{
			Level++;
			if (Application.platform == RuntimePlatform.WebGLPlayer) {
				FirebaseDatabase.nextLevel(Web3Auth.Instance.addressWallet, Level);
			}
			
			UnlockSkillCheck (); //unlock new skills if reaching a required level

		}

		aliveEnemies.Clear (); //remove everything in the alive enemy list
		
		Invoke ("StartBattle", nextRoundDelay); //start a new battle after seconds delay
	}
	
	private IEnumerator StartCountDown ()
	{
		timer.timerRunning = true;
		timer.timeRemaining = (double) timeLimit;

		countdown = timeLimit;

		while (countdown > 0 && isBattling)
		{
			yield return new WaitForSeconds (1f); //run the loop per second

			countdown--; 
		}

		StopBattle (); //stop the battle if running out of time

		HUD.Instance.panelStatusBattle.SetActive(true);
		HUD.Instance.panelStatusBattle.gameObject.GetComponentInChildren<Text>().text = "Time Out";
		Debug.Log ("time out");
	}

	private void SpawnEnemies ()
	{
		int i = 0;
		int sortingOrder = 10;
		foreach (Enemy enemy in enemies)
		{
			enemy.SpawnMonster (level);
			SpriteRenderer enemyRenderer = enemy.gameObject.GetComponentInChildren<SpriteRenderer> ();
			enemyRenderer.sortingOrder  = sortingOrder--;

			aliveEnemies.Add (i++);
		}
	}

	private void UnlockSkillCheck () //check if there is a new skill can be unlocked
	{
		bool newSkill = false;
		int idx = 0;
		foreach (ActiveSkill activeSkill in activeSkills)
		{
			if (level == activeSkill.unlockLevel) //unlock the skill if we beat the required level
			{
				activeSkill.Unlock ();
				playerData.skills[idx].unlocked = true;
				newSkill = true; //set to true if a new skill unlocked
				idx++;
			}
		}

		if (newSkill) {
			ActivatedSkill.Instance.StartNewSkillAlarm ();
			// code for save unlocked skill
		}
			
	}

	private void UnlockSkillCheckStart ()
	{
		int idx = 0;
		foreach (ActiveSkill activeSkill in activeSkills)
		{
			if (level >= activeSkill.unlockLevel) //unlock the skill if we beat the required level
			{
				activeSkill.Unlock ();
				playerData.skills[idx].unlocked = true;
				idx++;
			}
		}
	}

	public void Save () //if you want to make a button and save the game manually, set the click event to this function
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			playerData.SaveForWeb ();
		}else{
			playerData.Save ();
		}
			
	}

	public void ShowLeaderboard()
	{
		ShowingLeaderboard();
	}

	public void ShowingLeaderboard ()
	{
		GameSFX.Instance.playClickSound();
		HUD.Instance.btnLeaderBoard.GetComponent<Button>().interactable = false;
		string leaderboard = "";
		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			FirebaseDatabase.getLeaderboard();
		}else{
			leaderboard = "{\"0x361eD9654eC32941B838895883C445b97a49b580\":{\"level\":3,\"quitTime\":\"05/01/202312:40:13\",\"tempMoney\":70},\"0xB622cCffE20241565132D628a60880Bc00228Eee\":{\"activatedSkillID\":0,\"level\":3,\"money\":0,\"quitTime\":\"05/01/202314:02:05\",\"skills\":[{\"ID\":0,\"damageLevel\":1,\"speedLevel\":0,\"unlocked\":true},{\"ID\":1,\"damageLevel\":0,\"speedLevel\":0,\"unlocked\":false},{\"ID\":2,\"damageLevel\":0,\"speedLevel\":0,\"unlocked\":false},{\"ID\":3,\"damageLevel\":0,\"speedLevel\":0,\"unlocked\":false},{\"ID\":4,\"damageLevel\":0,\"speedLevel\":0,\"unlocked\":false},{\"ID\":5,\"damageLevel\":0,\"speedLevel\":0,\"unlocked\":false},{\"ID\":6,\"damageLevel\":0,\"speedLevel\":0,\"unlocked\":false},{\"ID\":7,\"damageLevel\":0,\"speedLevel\":0,\"unlocked\":false}],\"tempMoney\":96}}";
			ShowDataLeaderboard(leaderboard);
		}
	}

	public void OnGetLeaderboard(string result)
	{
		ShowDataLeaderboard(result);
	}

	private void ShowDataLeaderboard(string result)
	{
		string leaderboard = result;
		// Deserialize the string into a dictionary with a string key and a dictionary value
		Dictionary<string, Dictionary<string, object>> leaderboardDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(leaderboard);

		int countLeaderboard = 1;
		string txtLeaderboard = "";
		// Print the result
		foreach (KeyValuePair<string, Dictionary<string, object>> entry in leaderboardDict)
		{
			string key = entry.Key;
			Dictionary<string, object> itemDict = entry.Value;
			int level = Convert.ToInt32(itemDict["level"]);
			string quitTime = (string)itemDict["quitTime"];
			int tempMoney = Convert.ToInt32(itemDict["tempMoney"]);
			Debug.Log($"key: {key}, level: {level}, quitTime: {quitTime}, tempMoney: {tempMoney}");
			txtLeaderboard += countLeaderboard +". "+ key+" - Level "+level+"\n";
			countLeaderboard++;
		}

		for (int i = countLeaderboard; i <= 10;i++){
			txtLeaderboard += i +". 0x00000000000000000000000000000000000000000 - Level 0 \n";
		}

		txtLeaderboard = txtLeaderboard.Substring(0, txtLeaderboard.Length - 2);

		HUD.Instance.textLeaderBoard.text = txtLeaderboard;
		HUD.Instance.panelLeaderBoard.SetActive(true);
	}

}
