using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    // Variables de movimiento
    private float horizontalMove;
    private float verticalMove;

    private Vector3 playerVelocity;

    public CharacterController player;
    public float playerSpeed;

    public float gravity = 9.8f;
    public float fallVelocity;
    public float jumpForce;

    // Variables de Cámara
    public Camera mainCamera;
    public CinemachineFreeLook moveCamera;
    public CinemachineVirtualCamera aimCamera;
    
    private Vector3 camForward;
    private Vector3 camRight;

    private Vector3 playerDirection;

    // Variables deslizamiento de pendiente
    public bool isOnSlope = false;  // Para saber si se desliza o no dependiendo de la pendiente sobre la que está el personaje
    private Vector3 hitNormal;
    public float slideVelocity;
    public float slopeForceDown;

    // Variables de Disparo
    public float range = 100f;

    // Variables de Animacion
    public Animator playerAnimatorController;
    public bool aim = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<CharacterController>();
        playerAnimatorController = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        States();

        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        playerVelocity = new Vector3(horizontalMove, 0f, verticalMove);
        playerVelocity = Vector3.ClampMagnitude(playerVelocity, 1);

        playerAnimatorController.SetFloat("PlayerWalkVelocity", playerVelocity.magnitude * playerSpeed);

        camDirection();
        playerDirection = playerVelocity.x * camRight + playerVelocity.z * camForward;
        playerDirection = playerDirection * playerSpeed;

        LookAt();
        //player.transform.LookAt(player.transform.position + playerDirection);

        SetGravity();
        PlayerSkills();

        player.Move(playerDirection * Time.deltaTime);
    }

    // Control de estados
    void States()
    {
        if (Input.GetButton("Aim"))
        {
            aim = true;
            moveCamera.Priority = 9;
            aimCamera.Priority = 10;

        } 
        else
        {
            aim = false;

            moveCamera.Priority = 10;
            aimCamera.Priority = 9;
        }
        playerAnimatorController.SetBool("PlayerAim", aim);
    }

    // Funcion para las habilidades del jugador
    public void PlayerSkills()
    {
        if(player.isGrounded && Input.GetButtonDown("Jump"))
        {
            fallVelocity = jumpForce;
            playerDirection.y = fallVelocity;
            playerAnimatorController.SetTrigger("PlayerJump");
        }

        if (Input.GetButtonDown("Fire1") && aim)
        {
            //Debug.Log("Dispara");
            Shoot();
        }
    }

    // Control de la camara según el boton de apuntar (si apunta o no)
    void LookAt()
    {
        if (!aim)
        {
            player.transform.LookAt(player.transform.position + playerDirection);
        }
        else
        {
            player.transform.LookAt(player.transform.position + camForward);
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
            playerAnimatorController.SetFloat("PlayerVerticalVelocity", player.velocity.y);
        }
        playerAnimatorController.SetBool("IsGrounded", player.isGrounded);
        SlideDown();
    }


    public void SlideDown()
    {
        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= player.slopeLimit;

        if (isOnSlope)
        {
            playerDirection.x += (1f - hitNormal.y) * hitNormal.x * slideVelocity;
            playerDirection.z += (1f - hitNormal.y) * hitNormal.z * slideVelocity;

            playerDirection.y -= slopeForceDown;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
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

    //////////////////////////////////////////////////////////////////
    /// Habilities:
    //////////////////////////////////////////////////////////////////

    void Shoot()
    {
        RaycastHit hit;

        if (Physics.Raycast(mainCamera.transform.position, camForward, out hit, range))  // range es opcional
        {
            Debug.Log(hit.transform.name);
            playerAnimatorController.SetTrigger("PlayerShoot");
        }
    }

    private void OnAnimatorMove()
    {

    }
}
