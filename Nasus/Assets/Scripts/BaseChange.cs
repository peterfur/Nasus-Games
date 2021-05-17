using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseChange : MonoBehaviour
{
    private bool boss_dead = false;

    public void setBoss(bool state)
    {
        boss_dead = state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" & boss_dead) 
        {
            Director.Load(Director.Scene.water_islands);
        }
    }
}
