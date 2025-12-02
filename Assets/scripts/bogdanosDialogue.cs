using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class bogdanosDialogue : MonoBehaviour
{
    public bool firstDialgueFinishedBlud;
    
    coins c;
    public GameObject player;

    List<string> startingSpeech = new List<string>(3);

    List<string> lowMoneyText = new List<string>(7);
    List<string> MidMoneyText = new List<string>(7);
    List<string> HighMoneyText = new List<string>(7);

    private string starting1 = "Oh Hello!";
    private string starting2 = "In case you didnt hear, you are working for me now!!";
    private string starting3 = "Isnt it cool?? Im so excited!";
    private string starting4 = "Now go down there and get me some money!!";
    private string starting5 = "";

    private string Low1 = "You are cutting it a bit low..Dont mind if i..";
    private string Low2 = "Yoink..";
    private string Low3 = "I expected a bit more effort. Ill be taking my percentage now.";

    int randomIndex;
    public TextMeshProUGUI bogdanosText;

    public bool bogdanosIsTalking;

    // Start is called before the first frame update
    void Start()
    {

        firstDialgueFinishedBlud = false;

        bogdanosIsTalking = false;

        bogdanosText.transform.position = new Vector2(transform.position.x, transform.position.y + 2);
        bogdanosText.text = "";


        startingSpeech.Add(starting1);
        startingSpeech.Add(starting2);
        startingSpeech.Add(starting3);
        startingSpeech.Add(starting4);
        startingSpeech.Add(starting5);

        lowMoneyText.Add(Low1);
        lowMoneyText.Add(Low2);
        lowMoneyText.Add(Low3);


        c = player.GetComponent<coins>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetStartingText()
    {
        StartCoroutine(DisplayStartingSpeech());
    }

    private IEnumerator DisplayStartingSpeech()
    {
        bogdanosIsTalking = true;
        for (int i = 0; i < startingSpeech.Count; i++)
        {
            bogdanosText.text = startingSpeech[i];

            // First wait until mouse button is released (to prevent skipping)
            yield return new WaitUntil(() => !Input.GetKeyDown(KeyCode.E));

            // Then wait for a new click
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
            if (i == startingSpeech.Count - 1)
            {
                firstDialgueFinishedBlud = true;
            }

        }
        bogdanosIsTalking = false;
    }


    public void printBogdanosMoneyText()
    { 
            
            randomIndex = Random.Range(0, lowMoneyText.Count);

            if (c.beforetaxCoins >= 0 && c.beforetaxCoins <= 190)
            {
                bogdanosText.text = lowMoneyText[randomIndex];
            }

        bogdanosIsTalking = true;
    }


    public void HideDialogue()
    {
        bogdanosIsTalking = false;
        bogdanosText.text = "";
    }

}
