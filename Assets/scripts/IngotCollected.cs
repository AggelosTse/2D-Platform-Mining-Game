using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngotCollected : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public Sprite whiteIngot;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(CollectAnim());
        }
    }
    IEnumerator CollectAnim()
    {
        spriteRenderer.sprite = whiteIngot;
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }
}
