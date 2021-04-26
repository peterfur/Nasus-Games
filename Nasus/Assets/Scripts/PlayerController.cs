using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizontalMove;
    private float verticalMove;

    private Vector3 playerVelocity;
    private Vector3 playerDirection;

    public CharacterController player;
    public float gravity = 9.8f;
    public float fallVelocity;
    public float jumpForce;

    public float playerSpeed;

    public Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        playerVelocity = new Vector3(horizontalMove, 0, verticalMove);
        playerVelocity = Vector3.ClampMagnitude(playerVelocity, 1);

        camDirection();
        playerDirection = playerVelocity.x * camRight + playerVelocity.z * camForward;
        playerDirection = playerDirection * playerSpeed;
        player.transform.LookAt(player.transform.position + playerDirection);

        SetGravity();
        PlayerSkills();

        player.Move(playerDirection * Time.deltaTime);
    }

    // Funcion para las habilidades del jugador
    public void PlayerSkills()
    {
        if(player.isGrounded && Input.GetButtonDown("Jump"))
        {
            fallVelocity = jumpForce;
            playerDirection.y = fallVelocity;
        }
    }

    // Funcion de gravedad
    void SetGravity()
    {
        if (player.isGrounded)
        {
            fallVelocity = -gravity * Time.deltaTime;
            playerDirection.y = fallVelocity;
        } 
        else
        {
            fallVelocity -= gravity * Time.deltaTime;
            playerDirection.y = fallVelocity;
        }
    }

    void camDirection()
    {
        camForward = mainCamera.transform.forward;
        camRight = mainCamera.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward = camForward.normalized;
        camRight = camRight.normalized;
    }
}
