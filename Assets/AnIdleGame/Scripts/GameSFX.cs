using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

//script to control the game
public class GameSFX : MonoBehaviour {

	//Singleton
	private static GameSFX instance;

	public static GameSFX Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.Find("GameSFX").GetComponent<GameSFX> ();

			return instance;
		}
	}
    private AudioSource audioSource;
    public AudioClip clickClip;
    public AudioClip declineClip;
    public AudioClip redeemClip;

    void Awake ()
	{
		audioSource = GetComponent<AudioSource>();
	}

    public void playClickSound()
    {
        audioSource.PlayOneShot(clickClip);
    }
    public void playDeclineSound()
    {
        audioSource.PlayOneShot(declineClip);
    }

    public void playRedeemSound()
    {
        audioSource.PlayOneShot(redeemClip);
    }

}
