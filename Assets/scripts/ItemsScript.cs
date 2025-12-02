using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsScript : MonoBehaviour
{    

    public bool boughtHand;
    public bool boughtHealth;
    public bool boughtRope;

    public int ropesPurchased = 0; // how many ropes we bought this session

  

    void Start()
    {
        boughtHealth = false;
        boughtHand = false;
        boughtRope = false;
       
       
    }

    public void HandPressing()
    {
          if (coins.coinCount >= 10)
          {
              boughtHand = true;
            coins.SpendCoins(10);
          }
          else
          {
              Debug.Log("Not enough coins for hand.");
          }
      }

      public void BombPressing()
      {
          if (coins.coinCount >= 20)
          {
              Debug.Log("Got a bomb");
            coins.SpendCoins(20);
        }
          else
          {
              Debug.Log("Not enough coins for bomb.");
          }
      }

      public void RopePressing()
      {
          if (coins.coinCount >= 20 && ropesPurchased < 3)
          {
              // Add ropes to inventory
              ropesPurchased += 1;
              boughtRope = true;
            coins.SpendCoins(20);

            Debug.Log("Bought a rope. Total ropes: " + ropesPurchased);
          }
          else
          {
              Debug.Log("Not enough coins for rope.");
          }
      }

      public void HealthPressing()
      {
          if (coins.coinCount >= 20)
          {
              boughtHealth = true;
            coins.SpendCoins(20);
        }
          else
          {
              Debug.Log("Not enough coins for health.");
          }
      } 
    
}