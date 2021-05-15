using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAiming : MonoBehaviour
{
    public float turnSpeed = 15;
    public float aimDuration = 0.3f;
    public Transform cameraLookAt;

    public Cinemachine.AxisState xAxis;
    public Cinemachine.AxisState yAxis;

    Quaternion originRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        originRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        /*     
        if (Input.GetButton("Aim"))
        {
            // El focus de la cámara se mueve en el eje Y solo, el personaje se mueve con la cámara en el eje X
            cameraLookAt.Rotate(-Input.GetAxis("Mouse Y") * turnSpeed * Time.deltaTime, 0, 0);
            transform.Rotate(0, Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime, 0);

            xAxis.Update(Time.deltaTime);
            yAxis.Update(Time.deltaTime);

            cameraLookAt.eulerAngles = new Vector3(yAxis.Value, xAxis.Value, 0);

            float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, yawCamera, 0f), turnSpeed * Time.deltaTime);
        
        }
        */
        if (Input.GetButton("Aim")) {

            xAxis.Update(Time.deltaTime);
            yAxis.Update(Time.deltaTime);

            var yaw = Quaternion.AngleAxis(xAxis.Value, Vector3.up);
            var pitch = Quaternion.AngleAxis(yAxis.Value, Vector3.left);

            //transform.localRotation = originRotation * yaw * pitch;
            cameraLookAt.localRotation = originRotation * pitch;
            transform.Rotate(0, Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime, 0);
        }
    }
}
