using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class TransformFollow : MonoBehaviour
    {
        public Transform target;

        private void LateUpdate()
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }
}