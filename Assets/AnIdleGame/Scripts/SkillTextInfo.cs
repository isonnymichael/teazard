using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
* This script is responsible for managing player's stats and skills in the game world   
*/
public class SkillTextInfo : Popup {

	public Text skillLevelText;
	public Text upgradeCostText;

	public void UpdateSkillText (int skillLevel, int upgradeCost)
	{
		skillLevelText.text = "Lv. " + skillLevel.ToString ();

		upgradeCostText.text = "Cost: " + upgradeCost.ToString ();
	}
}
