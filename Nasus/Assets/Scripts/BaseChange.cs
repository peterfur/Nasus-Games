using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseChange : MonoBehaviour
{
    private  bool boss_dead = true;

    public  void setBoss(bool state)
    {
        boss_dead = state;
    }

    private void OnTriggerEnter(Collider other)
    {
	Debug.Log("trigged");
        //if (other.tag == "Player" & boss_dead) 
        //{
	SceneManager.LoadScene("dungeon");
        //}
    }
}