using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseChange : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Hola");
        }
    }

    private void Update()
    {
        
    }

    /* void OnCollisionEnter(Collision other)
     {
         Debug.Log("Hola");

         if (other.gameObject.CompareTag("Player"))
         {
             Debug.Log("Hola");
             Loader.Load(Loader.Scene.SampleScene1);
         }
     }*/
}
