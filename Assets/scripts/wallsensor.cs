using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wallsensor : MonoBehaviour
{
    private wallClimb player;

    private void Awake()
    {
        player = GetComponentInParent<wallClimb>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        player.SetWallTouch(true);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        player.SetWallTouch(false);
    }
}
