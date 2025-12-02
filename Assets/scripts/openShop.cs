using UnityEngine;

public class openShop : MonoBehaviour
{
    public GameObject shopPanel;
    public GameObject shopGuy;

    private shopGuyInteractions shopInter;
    private bool isOpened = false;

    void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (shopGuy != null)
            shopInter = shopGuy.GetComponent<shopGuyInteractions>();
    }

    void Update()
    {
        if (shopInter == null || shopPanel == null) return;

        // Only allow opening if the player is in range
        if (shopInter.canOpenShop && Input.GetKeyDown(KeyCode.E))
        {
            isOpened = !isOpened; // toggle state
            shopPanel.SetActive(isOpened);
        }

        // Optional: auto-close if player walks away
        if (!shopInter.canOpenShop && isOpened)
        {
            isOpened = false;
            shopPanel.SetActive(false);
        }
    }
}
