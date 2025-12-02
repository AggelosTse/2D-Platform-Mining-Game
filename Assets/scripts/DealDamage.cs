using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    public GameObject player;
    public int damage;
    public float damageCooldown = 1.5f; // seconds before enemy can damage again

    private Health h;
    private float lastDamageTime;


    public GameObject camer;
    cameraShake cam;

    void Start()
    {
        cam = camer.GetComponent<cameraShake>();
        h = player.GetComponent<Health>();
        lastDamageTime = -damageCooldown; // so enemy can damage immediately on first touch
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown && h.health > 0)
            {
                cam.StartShake(0.1f, 0.03f);
                h.health -= damage;
                lastDamageTime = Time.time;
            }
        }
    }
}
