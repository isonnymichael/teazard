using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
* This script is for controlling a popup that appears when an enemy or player gets hit by something 
*/
public class DamagePopup : MonoBehaviour {

	public void Show (int damage)
	{
		GetComponent<Text> ().text = damage.ToString (); //set the text to the number of the damage you made
	}
}
