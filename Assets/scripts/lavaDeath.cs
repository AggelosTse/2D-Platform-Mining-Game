using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lavaDeath : MonoBehaviour
{
    Health h;
    // Start is called before the first frame update
    void Start()
    {
        h = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("lava"))
        {
            h.health = 0;
        }
    }
}
