using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseChange : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Loader.Load(Loader.Scene.SampleScene1);
        }
    }
}
