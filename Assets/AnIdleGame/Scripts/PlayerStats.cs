using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Globalization;
using Thirdweb;

//script used to upgrade skill 
public class PlayerStats : MonoBehaviour {

	//Singleton
	private static PlayerStats instance;
	
	public static PlayerStats Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.Find("Player").GetComponent<PlayerStats> ();
			
			return instance;
		}
	}

	public SkillTextInfo damageText;
	public SkillTextInfo speedText;

	[HideInInspector] public ActiveSkill activeSkill; //reference to the current using active skill
	[HideInInspector] public ActiveSkill selectedActiveSkill; //used when you select a skill in active skill selecting window

	private int money; //money you have
	private int tempMoney; //temp money you have

	public int Money
	{
		get
		{
			return money;
		}
		set
		{
			money = value;

			HUD.Instance.moneyText.text = money.ToString (); //when change the value of money, update the text
		}
	}
	public int TempMoney
	{
		get
		{
			return tempMoney;
		}
		set
		{
			tempMoney = value;

			HUD.Instance.tempMoneyText.text = " (+" + tempMoney.ToString ()+")"; //when change the value of money, update the text
			if(tempMoney <= 0){
				HUD.Instance.panelRedeem.SetActive(false);
			}else{
				HUD.Instance.panelRedeem.SetActive(true);
			}		
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
	}

	void Start ()
	{
		foreach (ActiveSkill activeSkill in GameManager.Instance.activeSkills) //load the activeSkill which has the saved ID
		{
			if (activeSkill.ID == GameManager.Instance.playerData.activatedSkillID)
			{
				this.activeSkill = selectedActiveSkill = activeSkill;

				break;
			}
		}

		UpdateSkillText (); //call the function to update the text with corresponding damage and speed level to the actived skill
		
		Money = GameManager.Instance.playerData.money; //load the amount of money
		TempMoney = GameManager.Instance.playerData.tempMoney; //load the amount of money

		DateTime quitTime = DateTime.ParseExact(GameManager.Instance.playerData.quitTime, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
		Debug.Log(DateTime.Now);
		Debug.Log(quitTime);

		TimeSpan timeSinceLastPlay = DateTime.Now - quitTime; //calculate the time difference between last play and now

		if (timeSinceLastPlay > TimeSpan.Zero) //if the difference is not negative
			TempMoney += (int)(timeSinceLastPlay.TotalHours * Mathf.Pow (GameManager.Instance.playerData.level, 2f)); //gain money according to the time difference
	}

	public async void UpgradeDamage () //function get called when the player click the damage upgrade button
	{
		
		if (Money >= activeSkill.DamageUpgradeCost) //if the player has enough money
		{
			GameSFX.Instance.playClickSound();

			HUD.Instance.loaderDamageUp.SetActive(true);
			HUD.Instance.btnTempToken.GetComponent<Button>().interactable = false;
			HUD.Instance.btnDamageUp.GetComponent<Button>().interactable = false;
			HUD.Instance.btnSpeedUp.GetComponent<Button>().interactable = false;

			if (Application.platform == RuntimePlatform.WebGLPlayer) {
				try
				{
					var data = await Web3Auth.Instance.contractGame.Read<object>("upgradeStats", new object[] { 0, activeSkill.ID, activeSkill.DamageUpgradeCost });
					
					if(data == null){
						HUD.Instance.loaderDamageUp.SetActive(false);
						HUD.Instance.btnTempToken.GetComponent<Button>().interactable = true;
						HUD.Instance.btnDamageUp.GetComponent<Button>().interactable = true;
						HUD.Instance.btnSpeedUp.GetComponent<Button>().interactable = true;

						return;
					}
				}
				catch (Exception ex)
				{
					Debug.Log(ex);
					
					HUD.Instance.loaderDamageUp.SetActive(false);
					HUD.Instance.btnTempToken.GetComponent<Button>().interactable = true;
					HUD.Instance.btnDamageUp.GetComponent<Button>().interactable = true;
					HUD.Instance.btnSpeedUp.GetComponent<Button>().interactable = true;

					return;
				}
			}
			
			Money -= activeSkill.DamageUpgradeCost;
			
			activeSkill.DamageLevel += 1;
			if (Application.platform == RuntimePlatform.WebGLPlayer) {
				FirebaseDatabase.updgradeDamage(Web3Auth.Instance.addressWallet, activeSkill.ID, activeSkill.DamageLevel);
			}
			
			GameManager.Instance.playerData.skills[activeSkill.ID].damageLevel = activeSkill.DamageLevel;

			damageText.UpdateSkillText (activeSkill.DamageLevel, activeSkill.DamageUpgradeCost); //update the text to show the new damage level and upgrade cost
		
			HUD.Instance.loaderDamageUp.SetActive(false);
			HUD.Instance.btnTempToken.GetComponent<Button>().interactable = true;
			HUD.Instance.btnDamageUp.GetComponent<Button>().interactable = true;
			HUD.Instance.btnSpeedUp.GetComponent<Button>().interactable = true;

			GameSFX.Instance.playRedeemSound();
		}
		else
		{
			Debug.Log ("need more money");
			GameSFX.Instance.playDeclineSound();

			GameObject popupInstance = (GameObject) Instantiate (HUD.Instance.prefabWarningUp);
			popupInstance.transform.SetParent (HUD.Instance.hudPanel.transform, false);
			popupInstance.transform.position = HUD.Instance.btnDamageUp.transform.position;
		}
	}

	public async void UpgradeSpeed () //function to upgrade speed, similar to UpgradeDamage()
	{

		if (Money >= activeSkill.SpeedUpgradeCost)
		{
			GameSFX.Instance.playClickSound();

			HUD.Instance.loaderSpeedUp.SetActive(true);
			HUD.Instance.btnTempToken.GetComponent<Button>().interactable = false;
			HUD.Instance.btnDamageUp.GetComponent<Button>().interactable = false;
			HUD.Instance.btnSpeedUp.GetComponent<Button>().interactable = false;

			if (Application.platform == RuntimePlatform.WebGLPlayer) {

				try
				{
					var data = await Web3Auth.Instance.contractGame.Read<object>("upgradeStats", new object[] { 1, activeSkill.ID, activeSkill.SpeedUpgradeCost });
					if(data == null){

						HUD.Instance.loaderSpeedUp.SetActive(false);
						HUD.Instance.btnTempToken.GetComponent<Button>().interactable = true;
						HUD.Instance.btnDamageUp.GetComponent<Button>().interactable = true;
						HUD.Instance.btnSpeedUp.GetComponent<Button>().interactable = true;
						
						return;
					}
				}
				catch (Exception ex)
				{
					Debug.Log(ex);

					HUD.Instance.loaderSpeedUp.SetActive(false);
					HUD.Instance.btnTempToken.GetComponent<Button>().interactable = true;
					HUD.Instance.btnDamageUp.GetComponent<Button>().interactable = true;
					HUD.Instance.btnSpeedUp.GetComponent<Button>().interactable = true;

					return;
				}

			}
			
			Money -= activeSkill.SpeedUpgradeCost;
			
			activeSkill.SpeedLevel += 1;
			if (Application.platform == RuntimePlatform.WebGLPlayer) {
				FirebaseDatabase.updgradeSpeed(Web3Auth.Instance.addressWallet, activeSkill.ID, activeSkill.SpeedLevel);
			}
			GameManager.Instance.playerData.skills[activeSkill.ID].speedLevel = activeSkill.SpeedLevel;

			speedText.UpdateSkillText (activeSkill.SpeedLevel, activeSkill.SpeedUpgradeCost);

			HUD.Instance.loaderSpeedUp.SetActive(false);
			HUD.Instance.btnTempToken.GetComponent<Button>().interactable = true;
			HUD.Instance.btnDamageUp.GetComponent<Button>().interactable = true;
			HUD.Instance.btnSpeedUp.GetComponent<Button>().interactable = true;

			GameSFX.Instance.playRedeemSound();
		}
		else
		{
			Debug.Log ("need more money");
			GameSFX.Instance.playDeclineSound();

			GameObject popupInstance = (GameObject) Instantiate (HUD.Instance.prefabWarningUp);
			popupInstance.transform.SetParent (HUD.Instance.hudPanel.transform, false);
			popupInstance.transform.position = HUD.Instance.btnSpeedUp.transform.position;
		}
	}

	public void UpdateSkillText () //function to update the text of Lv. and costs for both damage and speed
	{
		damageText.UpdateSkillText (activeSkill.DamageLevel, activeSkill.DamageUpgradeCost);
		
		speedText.UpdateSkillText (activeSkill.SpeedLevel, activeSkill.SpeedUpgradeCost);
	}
}
