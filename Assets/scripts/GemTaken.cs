using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GemTaken : MonoBehaviour

{
    public GameObject death;
  
    public bool GemIsTaken;
    // Start is called before the first frame update
    void Start()
    {
        GemIsTaken = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 deathPos = death.transform.position;
        Vector2 playerPos = transform.position;

        if (Vector2.Distance(deathPos, playerPos) <= 2 && Input.GetKeyDown(KeyCode.E) && GemIsTaken)
        {
            LoadingWinningScreen();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Gem"))
        {
            GemIsTaken = true;
            Destroy(collision.gameObject);
        }
    }

    private void LoadingWinningScreen()
    {
        SceneManager.LoadScene(2);
    }
}
