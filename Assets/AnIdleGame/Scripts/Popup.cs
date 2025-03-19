using UnityEngine;
using System.Collections;

/*
* This script is responsible for creating a pop-ups in the game world, such as when player interacts with an object or item   
*/   
public class Popup : MonoBehaviour {

	public GameObject popup;

	public void HoverOn () //when cursor enter, show popup
	{
		StartCoroutine ("Move");
		
		popup.SetActive (true);
	}
	
	public void HoverOff () //when cursor leave, hide popup
	{
		popup.SetActive (false);
		
		StopCoroutine ("Move");
	}
	
	protected virtual IEnumerator Move () //make the popup follow the cursor when hovering over
	{
		while (true)
		{
			popup.transform.position = Input.mousePosition;

			yield return null;
		}
	}
}
