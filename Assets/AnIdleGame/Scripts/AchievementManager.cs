using UnityEngine;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine.UI;
using TMPro;

public class AchievementManager : MonoBehaviour
{
    public Prefab_NFT[] nftPrefab;
    public Button[] mintButtons;
    public GameObject[] nftLoaders;

    private bool isFirstLoad = true;

    private void OnEnable()
    {
        if (Web3Auth.Instance == null)
        {
            Debug.LogError("Web3Auth instance not found!");
            return;
        }

        // Show loaders on first load
        if (isFirstLoad)
        {
            SetLoadersActive(true);
        }

        // Initialize all mint buttons
        for (int i = 0; i < mintButtons.Length; i++)
        {
            if (mintButtons[i] != null)
            {
                int requiredLevel = (i + 1) * 10; // Level 10, 20, 30, etc.
                var buttonText = mintButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
                
                if (buttonText == null)
                {
                    Debug.LogError($"TextMeshProUGUI component not found on button {i}");
                    continue;
                }

                if (GameManager.Instance.Level >= requiredLevel)
                {
                    mintButtons[i].interactable = false; // Will be enabled later if NFT not owned
                    buttonText.text = "Mint NFT";
                }
                else
                {
                    mintButtons[i].interactable = false;
                    buttonText.text = $"Unlock at level {requiredLevel}";
                }
            }
        }

        LoadAllNFTs();
    }

    private void SetLoadersActive(bool active)
    {
        foreach (GameObject loader in nftLoaders)
        {
            if (loader != null)
            {
                loader.SetActive(active);
            }
        }
    }

    private async void LoadAllNFTs()
    {
        if (nftPrefab == null || nftPrefab.Length == 0)
        {
            Debug.LogError("NFT Prefab array not set!");
            SetLoadersActive(false);
            return;
        }

        try
        {
            for (int i = 0; i < nftPrefab.Length; i++)
            {
                string tokenId = i.ToString();
                await GetNFTMedia(tokenId, nftPrefab[i]);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"NFT loading failed: {e.Message}");
        }
        finally
        {
            // Hide loaders after loading completes
            SetLoadersActive(false);
            // isFirstLoad = false;
        }
    }

    public async Task GetNFTMedia(string tokenId, Prefab_NFT targetPrefab)
    {
        if (Web3Auth.Instance?.contractNFT == null)
        {
            Debug.LogError("NFT contract not available!");
            return;
        }

        if (targetPrefab == null)
        {
            Debug.LogError("Target Prefab is null!");
            return;
        }

        try
        {
            string stringBalance = await Web3Auth.Instance.contractNFT.Read<string>("balanceOf", Web3Auth.Instance.addressWallet, tokenId);
            float floatBalance = float.Parse(stringBalance);

            Image nftImage = targetPrefab.transform.Find("Image_NFT")?.GetComponent<Image>();
            if (nftImage == null)
            {
                Debug.LogError("Image_NFT component not found!");
                return;
            }

            int index = int.Parse(tokenId);

            if (floatBalance > 0)
            {
                // User owns this NFT
                NFT nft = await Web3Auth.Instance.contractNFT.ERC1155.Get(tokenId);
                
                if (!nft.Equals(default(NFT)))
                {
                    targetPrefab.LoadNFT(nft);
                    nftImage.color = Color.white;
                    
                    if (mintButtons != null && index < mintButtons.Length && mintButtons[index] != null)
                    {
                        mintButtons[index].gameObject.SetActive(false);
                    }
                    
                    Debug.Log($"Successfully loaded NFT {tokenId} (Owned)");
                }
            }
            else
            {
                NFT nft = await Web3Auth.Instance.contractNFT.ERC1155.Get(tokenId);
                targetPrefab.LoadNFT(nft);
                
                // User doesn't own this NFT
                nftImage.color = Color.gray;
                
                if (mintButtons != null && index < mintButtons.Length && mintButtons[index] != null)
                {
                    int requiredLevel = (index + 1) * 10;
                    var buttonText = mintButtons[index].GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    
                    if (GameManager.Instance.Level >= requiredLevel)
                    {
                        mintButtons[index].gameObject.SetActive(true);
                        mintButtons[index].interactable = true;
                        if (buttonText != null)
                            buttonText.text = "Mint NFT";
                    }
                    else
                    {
                        mintButtons[index].gameObject.SetActive(true);
                        mintButtons[index].interactable = false;
                        if (buttonText != null)
                            buttonText.text = $"Unlock at level {requiredLevel}";
                    }
                }
                
                Debug.Log($"NFT {tokenId} not owned by player");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load NFT {tokenId}: {e.Message}");
        }
        finally
        {
            nftLoaders[int.Parse(tokenId)].SetActive(false);
        }
    }

     // Add this method to handle NFT claims
    public async void ClaimNFT(string tokenId)
    {
        if (Web3Auth.Instance?.contractNFT == null)
        {
            Debug.LogError("Contract not available!");
            return;
        }

        int index;
        if (!int.TryParse(tokenId, out index))
        {
            Debug.LogError("Invalid token ID format!");
            return;
        }

        nftLoaders[index].SetActive(true);

        // Get references safely
        Button mintButton = (mintButtons != null && index >= 0 && index < mintButtons.Length) 
            ? mintButtons[index] 
            : null;

        Prefab_NFT targetNftPrefab = (nftPrefab != null && index >= 0 && index < nftPrefab.Length) 
            ? nftPrefab[index] 
            : null;

        try
        {
            // Disable all buttons during claim process
            SetAllButtonsInteractable(false);

            // Execute the claim
            var result = await Web3Auth.Instance.contractNFT.ERC1155.Claim(
                tokenId, 
                1
            );

            // If successful, update UI
            if (mintButton != null)
            {
                mintButton.gameObject.SetActive(false);
            }

            if (targetNftPrefab != null)
            {
                Image nftImage = targetNftPrefab.transform.Find("Image_NFT")?.GetComponent<Image>();
                if (nftImage != null)
                {
                    nftImage.color = Color.white;
                }
            }

            // Refresh NFT data
            await GetNFTMedia(tokenId, targetNftPrefab);

            RefreshAllButtons();
            nftLoaders[index].SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Claim failed: {e.Message}");
            SetLoadersActive(false);
            RefreshAllButtons();
        }
        finally
        {
            // Re-enable buttons that should be active
            RefreshAllButtons();
            nftLoaders[index].SetActive(false);
        }
    }

    private void SetAllButtonsInteractable(bool state)
    {
        foreach (Button button in mintButtons)
        {
            if (button != null && button.gameObject.activeSelf)
            {
                button.interactable = state;
            }
        }
    }

    private void RefreshAllButtons()
    {
        for (int i = 0; i < mintButtons.Length; i++)
        {
            if (mintButtons[i] != null)
            {
                int requiredLevel = (i + 1) * 10;
                bool levelMet = GameManager.Instance.Level >= requiredLevel;
                
                // Only enable if level requirement is met
                mintButtons[i].interactable = levelMet;
            }
        }
    }

    private void OnDisable()
    {
        Debug.Log("Achievement Manager disabled");
    }
}