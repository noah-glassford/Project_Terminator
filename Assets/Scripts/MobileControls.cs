using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControls : MonoBehaviour
{
    public Joystick joystickMove;
    public Joystick joystickCamera;
    public gunManager gunManager;
    public Rigidbody rigidBody;
    public Camera playerCam;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        Cursor.lockState = CursorLockMode.None;


        Vector3 movement = Vector3.zero;

        movement += -joystickMove.Direction.x * playerCam.transform.forward;
        movement += joystickMove.Direction.y * playerCam.transform.right;

        movement *= Time.deltaTime;
        movement *= speed;

        movement.y = 0;

        transform.Translate(movement);


        
            

            Vector3 rotation;

            rotation = playerCam.transform.rotation.eulerAngles;

            rotation.x += -joystickCamera.Direction.y;

            rotation.y += joystickCamera.Direction.x;

           // rotation *= Time.deltaTime;

            playerCam.transform.rotation = Quaternion.Euler(rotation);
        
    }

    public void ShootButtonPressed()
    {
        gunManager.currentGun.Fire();
    }

    public void ReloadButtonPressed()
    {
        gunManager.currentGun.DoReload();
    }

}
