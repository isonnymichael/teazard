using UnityEngine;
using System.Collections;
using Thirdweb;

/*
* This script handles the connection to Web3 wallets and interacting with smart contracts on Ethereum network.   
*/
public class Web3Auth : MonoBehaviour {
	
	private static Web3Auth instance;

	public ThirdwebSDK sdk;
	public string addressWallet;
	public string valueToken;
	public string displayValueToken;
	public string symbolToken;
	public int chainId = 10218;

	public Contract contractToken;
	public Contract contractGame;
	public Contract contractNFT;

	public static Web3Auth Instance
	{
		get
		{
			if (Application.platform == RuntimePlatform.WebGLPlayer) {
				if (instance == null)
					instance = GameObject.Find("Web3").GetComponent<Web3Auth> ();
			}
			
			return instance;
		}
	}

	void Awake ()
	{
		if (instance == null)
		{
			var options = new ThirdwebSDK.Options
            {
                storage = new ThirdwebSDK.StorageOptions
                {
                    ipfsGatewayUrl = "https://ipfs.io/ipfs/"
                }
            };

			sdk = new ThirdwebSDK("https://tea-sepolia.g.alchemy.com/public", options);
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
