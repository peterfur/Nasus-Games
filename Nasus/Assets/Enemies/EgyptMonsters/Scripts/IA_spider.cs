using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IA_spider : MonoBehaviour
{
    public Transform ObjectToFollow = null;
    public float speed = 2;
    bool follow = false;
    private Animator anim;

    private void Awake()
    {
        ObjectToFollow = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
        anim.SetBool("isFinding", follow);

    }

    // Update is called once per frame
    void Update()
    {
        if (ObjectToFollow == null)
            return;

        if (follow)
        {
            transform.position = Vector3.MoveTowards(transform.position, ObjectToFollow.transform.position, speed * Time.deltaTime);
            transform.forward = ObjectToFollow.position - transform.position;
        }
         
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            follow = true;
            anim.SetBool("isFinding", follow);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            follow = false;
            anim.SetBool("isFinding", follow);
        }
    }
}
