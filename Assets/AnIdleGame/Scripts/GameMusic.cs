using UnityEngine;
using System.Collections;

/*
* This script is for controlling environment setup and generation of a level or map within Unity game development using C# programming language 
*/
public class GameMusic : MonoBehaviour {
	
	private static GameMusic instance;

	public static GameMusic Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.Find("GameMusic").GetComponent<GameMusic> ();
			
			return instance;
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

		DontDestroyOnLoad (this.gameObject);
	}
}
