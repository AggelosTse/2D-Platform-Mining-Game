using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class coinTextAnim : MonoBehaviour
{
    public Text te;

    public float scaleUpFactor = 1.3f; // how big it scales
    public float animationSpeed = 0.2f; // speed of pop effect
    

    Vector3 originalScale;
    

    void Start()
    {
      
        originalScale = transform.localScale;

        te.color = Color.white;
   

    
        
    }

    public void StartTextAnim()
    {
        StartCoroutine(PlayPopEffect());
    }


    IEnumerator PlayPopEffect()
    {
     

        // Scale up
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / animationSpeed;
            if(coins.SpendMoney)
            {
                te.color = Color.red;
            }
            else if(coins.earnMoney)
            {
                te.color = Color.green;
            }
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleUpFactor, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Scale back down
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / animationSpeed;
            te.color = Color.white;
            transform.localScale = Vector3.Lerp(originalScale * scaleUpFactor, originalScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Restore color
        
    }
}
