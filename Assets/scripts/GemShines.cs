using System.Collections;
using UnityEngine;

public class GemShines : MonoBehaviour
{
    Animator a;
    float timer;
    bool isShining; // ✅ prevents multiple coroutines at once

    void Start()
    {
        a = GetComponent<Animator>();
        timer = 0f;
        isShining = false;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 4f && !isShining)
        {
            StartCoroutine(ShineAnim());
        }
    }

    IEnumerator ShineAnim()
    {
        isShining = true;
        a.SetBool("shines", true);

        float clipLength = a.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);
      

        a.SetBool("shines", false);
        timer = 0f;
        isShining = false;
    }
}
