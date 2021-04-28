using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    // VARIABLES PUBLICAS:

    public CharacterController player;
    public Transform cam;

    public float speed = 6f;
    public float jumpForce;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float gravity = 9.8f;
    public float fallVelocity;

    public float slideVelocity;
    public float slopeForceDown;

    // VARIABLES PRIVADAS
    private Vector3 direction;

    private bool isOnSlope = false;  // Para saber si se desliza o no dependiendo de la pendiente sobre la que está el personaje
    private Vector3 hitNormal;

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        direction = new Vector3(horizontal, 0f, vertical).normalized;


        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            moveDir = SetGravity(moveDir);
            moveDir = PlayerSkills(moveDir);

            player.Move(moveDir.normalized * speed * Time.deltaTime);
        }
    }

    ////////////////////////////////////////////////////////

    // Funcion de gravedad
    Vector3 SetGravity(Vector3 direction)
    {
        if (player.isGrounded)
        {
            fallVelocity = -gravity * Time.deltaTime;
            direction.y = fallVelocity;
        }
        else
        {
            fallVelocity -= gravity * Time.deltaTime;
            direction.y = fallVelocity;
        }

        return SlideDown(direction);
    }


    public Vector3 SlideDown(Vector3 direction)
    {
        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= player.slopeLimit;

        if (isOnSlope)
        {
            direction.x += (1f - hitNormal.y) * hitNormal.x * slideVelocity;
            direction.z += (1f - hitNormal.y) * hitNormal.z * slideVelocity;

            direction.y -= slopeForceDown;
        }

        return direction;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    // Funcion para las habilidades del jugador
    public Vector3 PlayerSkills(Vector3 direction)
    {
        if (player.isGrounded && Input.GetButtonDown("Jump"))
        {
            fallVelocity = jumpForce;
            direction.y = fallVelocity;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("Dispara");
            //Shoot();
        }

        return direction;
    }
}