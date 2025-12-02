using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class coins : MonoBehaviour
{
    public int beforetaxCoins = 0;
    public int aftertaxCoins = 0;

    public static int coinCount;
    public static int maxcoins;
 

    [SerializeField] private Text tex; // visible in Inspector
    private static coins instance;     // reference to the single "coins" object

    private coinTextAnim cAnim;

    public static bool earnMoney;
    public static bool SpendMoney;

    void Awake()
    {
        instance = this; // store reference so static methods can access it
    }

    void Start()
    {
        maxcoins = 0;
        coinCount = 0;
        beforetaxCoins = 0;
        cAnim = tex.GetComponent<coinTextAnim>();

        earnMoney = false;
        SpendMoney = false;

        UpdateCoinText();
    }

    public void AddCoins(int amount)
    {
        coinCount += amount;
        beforetaxCoins += amount;

        maxcoins += amount;
        earnMoney = true;
        // ✅ Access cAnim through the instance
        if (instance != null && instance.cAnim != null)
        {
            instance.cAnim.StartTextAnim();
        }

        UpdateCoinText();
        earnMoney = false;
    }

    public static void SpendCoins(int amount)
    {
        coinCount -= amount;
        SpendMoney = true;
        if (instance != null && instance.cAnim != null)
        {
            instance.cAnim.StartTextAnim();
        }
        UpdateCoinText();
        SpendMoney = false;
    }

    public static void UpdateCoinText()
    {
        if (instance != null && instance.tex != null)
        {
            instance.tex.text = " x " + coinCount;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("IronIngot"))
        {
            AddCoins(2);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("GoldIngot"))
        {
            AddCoins(4);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("GreenIngot"))
        {
            AddCoins(6);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("PurpleIngot"))
        {
            AddCoins(15);
            Destroy(collision.gameObject);
        }
    }

    public void bogdanosTax()
    {
        
            float tax = 0.6f;

            aftertaxCoins = Mathf.RoundToInt(beforetaxCoins - beforetaxCoins * tax);
           
            beforetaxCoins = 0;
        // Optionally update coinCount to reflect the taxed value
        SpendCoins(aftertaxCoins);

            UpdateCoinText();
        
    }
}
