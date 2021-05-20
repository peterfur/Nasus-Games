using System;
using System.Collections;
using UnityEngine;
using Utilities.Message;
using UnityEngine.SceneManagement;

namespace Utilities
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IMessageReceiver
    {
        public static PlayerController instance
        {
            get { return s_Instance; }
        }

        protected static PlayerController s_Instance;

        // Variables de movimiento
        protected float m_DesiredForwardSpeed;         // How fast Ellen aims be going along the ground based on input.
        protected float m_ForwardSpeed;                // How fast Ellen is currently going along the ground.
        protected float m_VerticalSpeed;               // How fast Ellen is currently moving up or down.

        protected bool m_isGrounded = true;            // Indicate if Ellen is touching the surface
        protected bool m_PreviouslyGrounded = true;    // Whether or not Ellen was standing on the ground last frame.

        public float maxForwardSpeed = 8f;        // How fast Ellen can run.
        public float gravity = 20f;               // How fast Ellen accelerates downwards when airborne.
        public float jumpForce = 10f;             // How fast Ellen takes off when jumping.
        protected bool m_ReadyToJump;                  // Whether or not the input state and Ellen are correct to allow jumping.
        public float minTurnSpeed = 400f;         // How fast Ellen turns when moving at maximum speed.
        public float maxTurnSpeed = 1200f;        // How fast Ellen turns when stationary.
        public float idleTimeout = 5f;            // How long before Ellen starts considering random idles.

        protected Damageable m_Damageable;             // Reference used to set invulnerablity and health based on respawning.

        // These constants are used to ensure Ellen moves and behaves properly.
        const float k_AirborneTurnSpeedProportion = 5.4f;
        const float k_GroundedRayDistance = 1f;
        const float k_JumpAbortSpeed = 10f;
        const float k_InverseOneEighty = 1f / 180f;
        const float k_StickingGravityProportion = 0.3f;
        const float k_GroundAcceleration = 20f;
        const float k_GroundDeceleration = 25f;

        protected PlayerInput m_Input;           // Reference used to determine how Ellen should move.

        // Attack variables
        public bool canAttack;
        protected bool m_InAttack;                     // Whether Ellen is currently in the middle of a melee attack.
        protected bool m_InCombo;

        // Variables con la configuracion de la camara
        protected Quaternion m_TargetRotation;         // What rotation Ellen is aiming to have based on input.
        protected float m_AngleDiff;                   // Angle in degrees between Ellen's current rotation and her target rotation.

        // Variables de Animacion        
        protected float m_IdleTimer;                   // Used to count up to Ellen considering a random idle.

        // Controller of the Animator State
        protected AnimatorStateInfo m_CurrentStateInfo;    // Information about the base layer of the animator cached.
        protected AnimatorStateInfo m_NextStateInfo;
        protected bool m_IsAnimatorTransitioning;
        protected AnimatorStateInfo m_PreviousCurrentStateInfo;    // Information about the base layer of the animator from last frame.
        protected AnimatorStateInfo m_PreviousNextStateInfo;
        protected bool m_PreviousIsAnimatorTransitioning;

        // References
        [Header("References")] public CharacterController player;
        public Animator playerAnimatorController;
        public CameraSettings cameraSettings;            // Reference used to determine the camera's direction.
        public MeleeWeapon meleeWeapon;                  // Reference used to (de)activate the staff when attacking. 

        // Parameters
        readonly int m_HashAirborneVerticalSpeed = Animator.StringToHash("AirborneVerticalSpeed");
        readonly int m_HashForwardSpeed = Animator.StringToHash("ForwardSpeed");
        readonly int m_HashAngleDeltaRad = Animator.StringToHash("AngleDeltaRad");
        readonly int m_HashGrounded = Animator.StringToHash("Grounded");
        readonly int m_HashStateTime = Animator.StringToHash("StateTime");
        readonly int m_HashTimeoutToIdle = Animator.StringToHash("TimeoutToIdle");
        readonly int m_HashInputDetected = Animator.StringToHash("InputDetected");
        readonly int m_HashMeleeAttack = Animator.StringToHash("MeleeAttack");

        readonly int m_HashHurt = Animator.StringToHash("Hurt");
        readonly int m_HashHurtFromX = Animator.StringToHash("HurtFromX");
        readonly int m_HashHurtFromY = Animator.StringToHash("HurtFromY");
        readonly int m_HashDeath = Animator.StringToHash("Death");

        // States
        readonly int m_HashLocomotion = Animator.StringToHash("Locomotion");
        readonly int m_HashAirborne = Animator.StringToHash("Airborne");
        readonly int m_HashLanding = Animator.StringToHash("Landing");    // Also a parameter.
        readonly int m_HashEllenCombo1 = Animator.StringToHash("EllenCombo1");
        readonly int m_HashEllenCombo2 = Animator.StringToHash("EllenCombo2");
        readonly int m_HashEllenCombo3 = Animator.StringToHash("EllenCombo3");
        readonly int m_HashEllenCombo4 = Animator.StringToHash("EllenCombo4");
        readonly int m_HashEllenDeath = Animator.StringToHash("EllenDeath");

        // Control variable
        readonly int m_HashBlockInput = Animator.StringToHash("BlockInput");

        private int currentSceneIndex;
        
        // FUNCTIONS:
        protected bool IsMoveInput
        {
            get { return !Mathf.Approximately(m_Input.MoveInput.sqrMagnitude, 0f); }
        }

        public void SetCanAttack(bool canAttack)
        {
            this.canAttack = canAttack;
        }

        private void Reset()
        {
            meleeWeapon = GetComponentInChildren<MeleeWeapon>();
            cameraSettings = FindObjectOfType<CameraSettings>();

            if (cameraSettings != null)
            {
                if (cameraSettings.follow == null)
                    cameraSettings.follow = transform;

                if (cameraSettings.lookAt == null)
                    cameraSettings.follow = transform.Find("HeadTarget");
            }
        }

        // Called automatically by Unity after Awake whenever the script is enabled. 
        void OnEnable()
        {
            SceneLinkedSMB<PlayerController>.Initialise(playerAnimatorController, this);

            m_Damageable = GetComponent<Damageable>();
            m_Damageable.onDamageMessageReceivers.Add(this);

            m_Damageable.isInvulnerable = true;

            EquipMeleeWeapon(true);

            //m_Renderers = GetComponentsInChildren<Renderer>();
        }

        // Called automatically by Unity whenever the script is disabled.
        void OnDisable()
        {

        }

        void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }


        // Start is called before the first frame update
        void Awake()
        {
            player = GetComponent<CharacterController>();
            playerAnimatorController = GetComponent<Animator>();
            m_Input = GetComponent<PlayerInput>();

            meleeWeapon.SetOwner(gameObject);

            s_Instance = this;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            CacheAnimatorState();

            UpdateInputBlocking();

            EquipMeleeWeapon(IsWeaponEquiped());

            playerAnimatorController.SetFloat(m_HashStateTime, Mathf.Repeat(playerAnimatorController.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
            playerAnimatorController.ResetTrigger(m_HashMeleeAttack);

            if (m_Input.Attack && canAttack)
                playerAnimatorController.SetTrigger(m_HashMeleeAttack);

            CalculateForwardMovement();
            CalculateVerticalMovement();

            SetTargetRotation();

            if (IsOrientationUpdated() && IsMoveInput)
                UpdateOrientation();

            TimeoutToIdle();

            m_PreviouslyGrounded = m_isGrounded;

            if (m_Input.Pause)
            {
                Time.timeScale = 0;
                SceneManager.LoadScene("MenuPausa", LoadSceneMode.Additive);

            }
        }

        // Called at the start of FixedUpdate to record the current state of the base layer of the animator.
        void CacheAnimatorState()
        {
            m_PreviousCurrentStateInfo = m_CurrentStateInfo;
            m_PreviousNextStateInfo = m_NextStateInfo;
            m_PreviousIsAnimatorTransitioning = m_IsAnimatorTransitioning;

            m_CurrentStateInfo = playerAnimatorController.GetCurrentAnimatorStateInfo(0);
            m_NextStateInfo = playerAnimatorController.GetNextAnimatorStateInfo(0);
            m_IsAnimatorTransitioning = playerAnimatorController.IsInTransition(0);
        }

        // Called after the animator state has been cached to determine whether this script should block user input.
        void UpdateInputBlocking()
        {
            bool inputBlocked = m_CurrentStateInfo.tagHash == m_HashBlockInput && !m_IsAnimatorTransitioning;
            inputBlocked |= m_NextStateInfo.tagHash == m_HashBlockInput;
            m_Input.playerControllerInputBlocked = inputBlocked;
        }

        // Called after the animator state has been cached to determine whether or not the staff should be active or not.
        bool IsWeaponEquiped()
        {
            bool equipped = m_NextStateInfo.shortNameHash == m_HashEllenCombo1 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo1;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo2 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo2;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo3 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo3;
            equipped |= m_NextStateInfo.shortNameHash == m_HashEllenCombo4 || m_CurrentStateInfo.shortNameHash == m_HashEllenCombo4;

            return equipped;
        }

        // Called each physics step with a parameter based on the return value of IsWeaponEquiped.
        void EquipMeleeWeapon(bool equip)
        {
            meleeWeapon.gameObject.SetActive(equip);
            m_InAttack = false;
            m_InCombo = equip;

            if (!equip)
                playerAnimatorController.ResetTrigger(m_HashMeleeAttack);
        }


        ///////////////////////////////////////////////////////////////////////
        /// INPUT
        ///////////////////////////////////////////////////////////////////////

        // Called each physics step.
        void CalculateForwardMovement()
        {
            // Cache the move input and cap it's magnitude at 1.
            Vector2 moveInput = m_Input.MoveInput;
            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();

            // Calculate the speed intended by input.
            m_DesiredForwardSpeed = moveInput.magnitude * maxForwardSpeed;

            // Determine change to speed based on whether there is currently any move input.
            float acceleration = IsMoveInput ? k_GroundAcceleration : k_GroundDeceleration;

            // Adjust the forward speed towards the desired speed.
            m_ForwardSpeed = Mathf.MoveTowards(m_ForwardSpeed, m_DesiredForwardSpeed, acceleration * Time.deltaTime);

            // Set the animator parameter to control what animation is being played.
            playerAnimatorController.SetFloat(m_HashForwardSpeed, m_ForwardSpeed);
        }

        void CalculateVerticalMovement()
        {
            // If jump is not currently held and Ellen is on the ground then she is ready to jump.
            if (!m_Input.JumpInput && m_isGrounded)
                m_ReadyToJump = true;

            if (m_isGrounded)
            {
                // When grounded we apply a slight negative vertical speed to make Ellen "stick" to the ground.
                m_VerticalSpeed = -gravity * k_StickingGravityProportion;

                // If jump is held, Ellen is ready to jump and not currently in the middle of a melee combo...
                if (m_Input.JumpInput && m_ReadyToJump && !m_InCombo)
                {
                    // ... then override the previously set vertical speed and make sure she cannot jump again.
                    m_VerticalSpeed = jumpForce;
                    m_isGrounded = false;
                    m_ReadyToJump = false;
                }
            }
            else
            {
                // If Ellen is airborne, the jump button is not held and Ellen is currently moving upwards...
                if (!m_Input.JumpInput && m_VerticalSpeed > 0.0f)
                {
                    // ... decrease Ellen's vertical speed.
                    // This is what causes holding jump to jump higher that tapping jump.
                    m_VerticalSpeed -= k_JumpAbortSpeed * Time.deltaTime;
                }

                // If a jump is approximately peaking, make it absolute.
                if (Mathf.Approximately(m_VerticalSpeed, 0f))
                {
                    m_VerticalSpeed = 0f;
                }

                // If Ellen is airborne, apply gravity.
                m_VerticalSpeed -= gravity * Time.deltaTime;
            }
        }
        // Called each physics step to set the rotation Ellen is aiming to have.
        void SetTargetRotation()
        {
            // Create three variables, move input local to the player, flattened forward direction of the camera and a local target rotation.
            Vector2 moveInput = m_Input.MoveInput;
            Vector3 localMovementDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            Vector3 forward = Quaternion.Euler(0f, cameraSettings.Current.m_XAxis.Value, 0f) * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            Quaternion targetRotation;

            // If the local movement direction is the opposite of forward then the target rotation should be towards the camera.
            if (Mathf.Approximately(Vector3.Dot(localMovementDirection, Vector3.forward), -1.0f))
            {
                targetRotation = Quaternion.LookRotation(-forward);
            }
            else
            {
                // Otherwise the rotation should be the offset of the input from the camera's forward.
                Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
                targetRotation = Quaternion.LookRotation(cameraToInputOffset * forward);
            }

            // The desired forward direction of Ellen.
            Vector3 resultingForward = targetRotation * Vector3.forward;

            // Find the difference between the current rotation of the player and the desired rotation of the player in radians.
            float angleCurrent = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(resultingForward.x, resultingForward.z) * Mathf.Rad2Deg;

            m_AngleDiff = Mathf.DeltaAngle(angleCurrent, targetAngle);
            m_TargetRotation = targetRotation;
        }

        ///////////////////////////////////////////////////////////////////////

        // Called each physics step to help determine whether Ellen can turn under player input.
        bool IsOrientationUpdated()
        {
            bool updateOrientationForLocomotion = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashLocomotion || m_NextStateInfo.shortNameHash == m_HashLocomotion;
            bool updateOrientationForAirborne = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashAirborne || m_NextStateInfo.shortNameHash == m_HashAirborne;
            bool updateOrientationForLanding = !m_IsAnimatorTransitioning && m_CurrentStateInfo.shortNameHash == m_HashLanding || m_NextStateInfo.shortNameHash == m_HashLanding;

            return updateOrientationForLocomotion || updateOrientationForAirborne || updateOrientationForLanding || m_InCombo && !m_InAttack;
        }

        // Called each physics step after SetTargetRotation if there is move input and Ellen is in the correct animator state according to IsOrientationUpdated.
        void UpdateOrientation()
        {
            playerAnimatorController.SetFloat(m_HashAngleDeltaRad, m_AngleDiff * Mathf.Deg2Rad);

            Vector3 localInput = new Vector3(m_Input.MoveInput.x, 0f, m_Input.MoveInput.y);
            float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, m_ForwardSpeed / m_DesiredForwardSpeed);
            float actualTurnSpeed = m_isGrounded ? groundedTurnSpeed : Vector3.Angle(transform.forward, localInput) * k_InverseOneEighty * k_AirborneTurnSpeedProportion * groundedTurnSpeed;
            m_TargetRotation = Quaternion.RotateTowards(transform.rotation, m_TargetRotation, actualTurnSpeed * Time.deltaTime);

            transform.rotation = m_TargetRotation;
        }

        // Called each physics step to count up to the point where Ellen considers a random idle.
        void TimeoutToIdle()
        {
            bool inputDetected = IsMoveInput || m_Input.Attack || m_Input.JumpInput;
            if (m_isGrounded && !inputDetected)
            {
                m_IdleTimer += Time.deltaTime;

                if (m_IdleTimer >= idleTimeout)
                {
                    m_IdleTimer = 0f;
                    playerAnimatorController.SetTrigger(m_HashTimeoutToIdle);
                }
            }
            else
            {
                m_IdleTimer = 0f;
                playerAnimatorController.ResetTrigger(m_HashTimeoutToIdle);
            }

            playerAnimatorController.SetBool(m_HashInputDetected, inputDetected);
        }

        // Called each physics step (so long as the Animator component is set to Animate Physics) after FixedUpdate to override root motion.
        void OnAnimatorMove()
        {
            Vector3 movement;

            // If Ellen is on the ground...
            if (m_isGrounded)
            {
                // ... raycast into the ground...
                RaycastHit hit;
                Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
                if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    // ... and get the movement of the root motion rotated to lie along the plane of the ground.
                    movement = Vector3.ProjectOnPlane(playerAnimatorController.deltaPosition, hit.normal);

                    // Also store the current walking surface so the correct audio is played.
                    //Renderer groundRenderer = hit.collider.GetComponentInChildren<Renderer>();
                    //m_CurrentWalkingSurface = groundRenderer ? groundRenderer.sharedMaterial : null;
                }
                else
                {
                    // If no ground is hit just get the movement as the root motion.
                    // Theoretically this should rarely happen as when grounded the ray should always hit.
                    movement = playerAnimatorController.deltaPosition;
                    //m_CurrentWalkingSurface = null;
                }
            }
            else
            {
                // If not grounded the movement is just in the forward direction.
                movement = m_ForwardSpeed * transform.forward * Time.deltaTime;
            }

            // Rotate the transform of the character controller by the animation's root rotation.
            player.transform.rotation *= playerAnimatorController.deltaRotation;

            // Add to the movement with the calculated vertical speed.
            movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

            // Move the character controller.
            player.Move(movement);

            // After the movement store whether or not the character controller is grounded.
            m_isGrounded = player.isGrounded;

            // If Ellen is not on the ground then send the vertical speed to the animator.
            // This is so the vertical speed is kept when landing so the correct landing animation is played.
            if (!m_isGrounded)
                playerAnimatorController.SetFloat(m_HashAirborneVerticalSpeed, m_VerticalSpeed);

            // Send whether or not Ellen is on the ground to the animator.
            playerAnimatorController.SetBool(m_HashGrounded, m_isGrounded);
        }

        // This is called by an animation event when Ellen swings her staff.
        public void MeleeAttackStart(int throwing = 0)
        {
            meleeWeapon.BeginAttack(throwing != 0);
            m_InAttack = true;
        }

        // This is called by an animation event when Ellen finishes swinging her staff.
        public void MeleeAttackEnd()
        {
            meleeWeapon.EndAttack();
            m_InAttack = false;
        }

        /// <summary>
        /// El objeto recibe un mensaje que puede ser DAMAGE o DIE, dependiendo de cual sea, se le llama a una funcion o a otra.
        /// </summary>

        public void OnReceiveMessage(MessageType type, object sender, object data)
        {
            switch (type)
            {
                case MessageType.DAMAGED:
                    {
                        Damageable.DamageMessage damageData = (Damageable.DamageMessage)data;
                        Damaged(damageData);
                    }
                    break;
                case MessageType.DEAD:
                    {
                        Die();
                    }
                    break;
            }
        }


        // Funcion llamada por OnReceiveMessage
        void Damaged(Damageable.DamageMessage damageMessage)
        {
            playerAnimatorController.SetTrigger(m_HashHurt);

            // Find the direction of the damage.
            Vector3 forward = damageMessage.damageSource - transform.position;
            forward.y = 0f;

            Vector3 localHurt = transform.InverseTransformDirection(forward);

            // Set the HurtFromX and HurtFromY parameters of the animator based on the direction of the damage.
            playerAnimatorController.SetFloat(m_HashHurtFromX, localHurt.x);
            playerAnimatorController.SetFloat(m_HashHurtFromY, localHurt.z);

            // Shake the camera.
            CameraShake.Shake(CameraShake.k_PlayerHitShakeAmount, CameraShake.k_PlayerHitShakeTime);
        }

        // Funcion llamada por OnReceiveMessage
        void Die()
        {
            playerAnimatorController.SetTrigger(m_HashDeath);

            // La velocidad del personaje será 0
            m_ForwardSpeed = 0f;
            m_VerticalSpeed = 0f;

            // El personaje se queda invulnerable
            m_Damageable.isInvulnerable = true;
            Debug.Log("DieFunctionFinal");
            // Llamamos a una funcion llamada DieRoutine que una vez termine la animacion de muerte, hace lo que deberia para la escena de GameOver
            DieRoutine();

            DeathMenu();

            Debug.Log("DieFunctionFinalFIIIIIIINAL");
        }

        protected IEnumerator DieRoutine()
        {
            Debug.Log("DieRoutine");

            // Wait for the animator to be transitioning from the EllenDeath state.
            while (m_CurrentStateInfo.shortNameHash != m_HashEllenDeath || !m_IsAnimatorTransitioning)
            {
                yield return null;
            }

            // Wait for the screen to fade out.
            yield return StartCoroutine(ScreenFader.FadeSceneOut());
            while (ScreenFader.IsFading)
            {
                yield return null;
            }

            /**
             * TODO: Llamamos a la funcion del director que cambia a la pantalla de GameOver
             **/
        }

        void DeathMenu()
        {
            
            Debug.Log("Abre el menu de muerte");

            SceneManager.LoadScene("MenuDeath");
        }
    }
}