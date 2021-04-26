using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 0.125f;
    private Vector3 offset;

    public bool LookAtPlayer = true;
    public bool RotateAroundPlayer = true;
    public float RotationSpeed = 5.0f;

    void Start ()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (RotateAroundPlayer)
        {
            Quaternion camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * RotationSpeed, Vector3.up);

            offset = camTurnAngle * offset;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

        if (LookAtPlayer)
        {
            transform.LookAt(target);
        }
        
    }
}
