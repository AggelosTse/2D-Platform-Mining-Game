using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyhealth : MonoBehaviour
{
     int health = 3;
    SpriteRenderer sprite;
    private Color originalcolor;
    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalcolor = sprite.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(Damaging());
        }
            
        
    }
    IEnumerator Damaging()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        sprite.color = originalcolor;
    }
}
