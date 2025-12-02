using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timerScript : MonoBehaviour
{
    private float timer;      // internal accumulator
    public static int time;   // whole seconds

    void Update()
    {
        timer += Time.deltaTime;

        int newTime = Mathf.FloorToInt(timer); // convert to int
        if (newTime != time)
        {
            time = newTime;
          
        }
    }
}
