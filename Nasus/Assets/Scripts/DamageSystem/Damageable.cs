using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utilities.Message;

namespace Utilities
{
    public partial class Damageable : MonoBehaviour
    {
        public int maxHitPoints;
        [Tooltip("Time that this gameObject is invulnerable for, after receiving damage.")]
        public float invulnerabilityTime;

        [Tooltip("The angle from the which that damageable is hitable. Always in the world XZ plane, with the forward being rotate by hitForwardRoation")]
        [Range(0.0f, 360.0f)]
        public float hitAngle = 360.0f;

        [Tooltip("Allow to rotate the world forward vector of the damageable used to define the hitAngle zone")]
        [Range(0.0f, 360.0f)]
        [FormerlySerializedAs("hitForwardRoation")] //SHAME!
        public float hitForwardRotation = 360.0f;

        public bool isInvulnerable { get; set; }
        public int currentHitPoints { get; private set; }

        public UnityEvent OnDeath, OnReceiveDamage, OnHitWhileInvulnerable, OnBecomeVulnerable, OnResetDamage;

        protected float m_timeSinceLastHit = 0.0f;
        protected Collider m_Collider;

        System.Action schedule;

        [Tooltip("When this gameObject is damaged, these other gameObjects are notified.")]
        //[EnforceType(typeof(Message.IMessageReceiver))]
        public List<MonoBehaviour> onDamageMessageReceivers;

        void Start()
        {
            ResetDamage();
            m_Collider = GetComponent<Collider>();
        }

        void Update()
        {
            if (isInvulnerable)
            {
                m_timeSinceLastHit += Time.deltaTime;
                if (m_timeSinceLastHit > invulnerabilityTime)
                {
                    m_timeSinceLastHit = 0.0f;
                    isInvulnerable = false;
                    OnBecomeVulnerable.Invoke();
                }
            }
        }

        // Reinicia el daño al máximo (se usa al inicio o tras morir)
        public void ResetDamage()
        {
            currentHitPoints = maxHitPoints;
            isInvulnerable = false;
            m_timeSinceLastHit = 0.0f;
            OnResetDamage.Invoke();
        }

        // Funcion que desactiva o activa el collider dependiendo de si esta en modo invulnerable o no
        public void SetColliderState(bool enabled)
        {
            m_Collider.enabled = enabled;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// FUNCION PARA APLICAR EL DAÑO
        ///////////////////////////////////////////////////////////////////////////////////////////////
        
        public void ApplyDamage(DamageMessage data)
        {
            if (currentHitPoints <= 0)
            {
                // Si ya está muerto, se ignora el seguir restando daño
                return;
            }
            if (isInvulnerable)
            {
                // Si es invulnerable, no afecta el golpe
                OnHitWhileInvulnerable.Invoke();
                return;
            }

            // Calculamos el angulo de impacto ya que para que afecte el golpe, el angulo debe tener un rango en concreto
            Vector3 forward = transform.forward;
            forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

            //we project the direction to damager to the plane formed by the direction of damage
            Vector3 positionToDamager = data.damageSource - transform.position;
            positionToDamager -= transform.up * Vector3.Dot(transform.up, positionToDamager);

            if (Vector3.Angle(forward, positionToDamager) > hitAngle * 0.5f)
                return;

            // Tras el daño, se vuelve invulnerable y se resta la cantidad de daño recibida a la vida
            isInvulnerable = true;
            currentHitPoints -= data.amount;

            // Comprobamos el estado por si ya está muerto
            if (currentHitPoints <= 0)
                schedule += OnDeath.Invoke; //This avoid race condition when objects kill each other.
            else
                OnReceiveDamage.Invoke();

            // Informamos a los objetos a los que les afecta
            var messageType = currentHitPoints <= 0 ? MessageType.DEAD : MessageType.DAMAGED;

            for (var i = 0; i < onDamageMessageReceivers.Count; ++i)
            {
                var receiver = onDamageMessageReceivers[i] as IMessageReceiver;
                receiver.OnReceiveMessage(messageType, this, data);
            }
        }

        void LateUpdate()
        {
            if (schedule != null)
            {
                schedule();
                schedule = null;
            }
        }
    }
}