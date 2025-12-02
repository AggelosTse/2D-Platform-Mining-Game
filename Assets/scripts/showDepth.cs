using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class showDepth : MonoBehaviour
{




    public Transform player;
    private int dept;

    static int mindepth;
    // Start is called before the first frame update
    void Start()
    {
        mindepth = 1000;
        dept = 0;



    }

    // Update is called once per frame
    void Update()
    {
        dept = Mathf.FloorToInt(player.position.y);
        if (dept < mindepth)
        {
            mindepth = dept;
        }



    }
    public static int GetMinDepth()
    {
        return mindepth;
    }
}
