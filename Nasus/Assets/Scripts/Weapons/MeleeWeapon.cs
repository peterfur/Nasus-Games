using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class MeleeWeapon : MonoBehaviour
{
    [System.Serializable]
    public class AttackPoint
    {
        public float radius;
        public Vector3 offset;
        public Transform attackRoot;

#if UNITY_EDITOR
        //editor only as it's only used in editor to display the path of the attack that is used by the raycast
        [NonSerialized] public List<Vector3> previousPositions = new List<Vector3>();
#endif

    }

    protected GameObject m_Owner;   // Owner of the weapon
    public int damage = 1;

    public LayerMask targetLayers;  // Layer a las que va a golpear
    public ParticleSystem hitParticlePrefab;

    public AttackPoint[] attackPoints = new AttackPoint[0];
    public TimeEffect[] effects;
    protected Vector3[] m_PreviousPos = null;
    protected Vector3 m_Direction;

    public bool throwingHit
    {
        get { return m_IsThrowingHit; }
        set { m_IsThrowingHit = value; }
    }

    protected bool m_IsThrowingHit = false;
    protected bool m_InAttack = false;

    protected static RaycastHit[] s_RaycastHitCache = new RaycastHit[32];
    protected static Collider[] s_ColliderCache = new Collider[32];

    // Particulas para los efectos al atacar
    
    const int PARTICLE_COUNT = 10;
    protected ParticleSystem[] m_ParticlesPool = new ParticleSystem[PARTICLE_COUNT];
    protected int m_CurrentParticle = 0;

    private void Awake()
    {
        if (hitParticlePrefab != null)
        {
            for (int i = 0; i < PARTICLE_COUNT; ++i)
            {
                m_ParticlesPool[i] = Instantiate(hitParticlePrefab);
                m_ParticlesPool[i].Stop();
            }
        }
    }

    private void OnEnable()
    {

    }

    //whoever own the weapon is responsible for calling that. Allow to avoid "self harm"
    public void SetOwner(GameObject owner)
    {
        m_Owner = owner;
    }

    public void BeginAttack(bool thowingAttack)
    {
        throwingHit = thowingAttack;

        m_InAttack = true;

        m_PreviousPos = new Vector3[attackPoints.Length];

        for (int i = 0; i < attackPoints.Length; ++i)
        {
            Vector3 worldPos = attackPoints[i].attackRoot.position +
                               attackPoints[i].attackRoot.TransformVector(attackPoints[i].offset);
            m_PreviousPos[i] = worldPos;

#if UNITY_EDITOR
            attackPoints[i].previousPositions.Clear();
            attackPoints[i].previousPositions.Add(m_PreviousPos[i]);
#endif
        }
    }

    public void EndAttack()
    {
        m_InAttack = false;


#if UNITY_EDITOR
        for (int i = 0; i < attackPoints.Length; ++i)
        {
            attackPoints[i].previousPositions.Clear();
        }
#endif
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_InAttack)
        {
            for (int i = 0; i < attackPoints.Length; ++i)
            {
                AttackPoint pts = attackPoints[i];

                Vector3 worldPos = pts.attackRoot.position + pts.attackRoot.TransformVector(pts.offset);
                Vector3 attackVector = worldPos - m_PreviousPos[i];

                if (attackVector.magnitude < 0.001f)
                {
                    // A zero vector for the sphere cast don't yield any result, even if a collider overlap the "sphere" created by radius. 
                    // so we set a very tiny microscopic forward cast to be sure it will catch anything overlaping that "stationary" sphere cast
                    attackVector = Vector3.forward * 0.0001f;
                }

                Ray r = new Ray(worldPos, attackVector.normalized);

                int contacts = Physics.SphereCastNonAlloc(r, pts.radius, s_RaycastHitCache, attackVector.magnitude, 0, QueryTriggerInteraction.Ignore);

                for (int k = 0; k < contacts; ++k)
                {
                    Collider col = s_RaycastHitCache[k].collider;

                    //Comprobar el da�o de col
                }

                m_PreviousPos[i] = worldPos;

#if UNITY_EDITOR
                pts.previousPositions.Add(m_PreviousPos[i]);
#endif
            }
        }
    }
}
