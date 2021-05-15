﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Assures the particle systems are simulating once.
/// </summary>

namespace Utilities
{
    public class SimulateParticleSystem : MonoBehaviour
    {

        public ParticleSystem[] systems;

        private void OnEnable()
        {
            for (int i = 0; i < systems.Length; i++)
            {
                if (!systems[i].isPlaying)
                    systems[i].Simulate(0f);
            }

        }
    } 
}
