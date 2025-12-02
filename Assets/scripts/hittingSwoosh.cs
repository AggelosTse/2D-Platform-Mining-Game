using System.Collections;
using UnityEngine;

public class hittingSwoosh : MonoBehaviour
{
    public GameObject hitanim;
    private Animator anim;
    private SpriteRenderer s;
    private bool isPlaying = false;
    public float animationLength = 0.5f;
    private int sum;

    wallClimb climb;

    void Start()
    {
        climb = GetComponent<wallClimb>();

        anim = hitanim.GetComponent<Animator>();
        s = hitanim.GetComponent<SpriteRenderer>();
        anim.enabled = false;
        s.enabled = false;
        sum = 0;
    }

    // Triggered by TileBreaker
    public void Play(float rotationZ)
    {
        if ((!isPlaying && !climb.isclimbing))
            StartCoroutine(StartHitting(rotationZ));
    }

    IEnumerator StartHitting(float rotationZ)
    {
        if(sum % 2 == 0)
        {
            s.flipX = true;
        }
        else
        {
            s.flipX = false;
        }


            isPlaying = true;

        // Only rotate the animation
        hitanim.transform.localRotation = Quaternion.Euler(0, 0, rotationZ);

        anim.enabled = true;
        s.enabled = true;
        anim.Play("dcdd", -1, 0f); // Replace "dcdd" with your animation name

        yield return new WaitForSeconds(animationLength);

        anim.enabled = false;
        s.enabled = false;
        isPlaying = false;
        sum++;
    }
}
