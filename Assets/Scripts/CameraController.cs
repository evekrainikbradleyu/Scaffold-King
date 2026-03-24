/*****************************************************************************
// File Name : CameraController.cs
// Author : Eve "Goddess of Cameras" Krainik
// Creation Date : March 10, 2026
//
// Brief Description : Makes the camera circle around the position of the 
CameraTrack game object when right click is held down.
*****************************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{

    #region variables

    public int nearestWall;

    private InputAction rightClick;
    private Vector3 cameraOffset;
    private bool rightMouseButtonDown;

    [SerializeField] private GameObject cameraTrack;
    [SerializeField] private float cameraRotateSpeed;
    [SerializeField] private GameObject mapWalls;

    #endregion

    #region start and update

    /// <summary>
    /// sets up variables and input actions
    /// </summary>
    private void Start()
    {
        cameraOffset = transform.position - cameraTrack.transform.position;
        rightMouseButtonDown = false;

        rightClick = InputSystem.actions.FindAction("Right Click");

        rightClick.started += RightClickStarted;
        rightClick.canceled += RightClickCanceled;

        transform.RotateAround(cameraTrack.transform.position, Vector3.up, 45);
    }

    /// <summary>
    /// rotates the camera around cameraTrack if the right mouse button is held
    /// down. also makes walls transparent depending on where the camera is
    /// </summary>
    void Update()
    {

        if (rightMouseButtonDown)
        {
            transform.RotateAround(cameraTrack.transform.position, Vector3.up, 
                cameraRotateSpeed * Input.GetAxis("Mouse X"));
        }

        #region wall shenanigans

        // make map walls transparent when the camera is behind them
        mapWalls.transform.Find("Wall1").gameObject.SetActive(transform.
            position.z <= 0 ? false : true);
        mapWalls.transform.Find("Wall3").gameObject.SetActive(transform.
            position.z > 0 ? false : true);
        mapWalls.transform.Find("Wall2").gameObject.SetActive(transform.
            position.x >= 0 ? false : true);
        mapWalls.transform.Find("Wall4").gameObject.SetActive(transform.
            position.x < 0 ? false : true);

        // set nearestwall
        nearestWall =
            transform.rotation.eulerAngles.y > 315 || transform.rotation.
                eulerAngles.y <= 45 ?
            1 :
            transform.rotation.eulerAngles.y > 45 && transform.rotation.
                eulerAngles.y <= 135 ?
            2 :
            transform.rotation.eulerAngles.y > 135 && transform.rotation.
                eulerAngles.y <= 225 ?
            3 : 4;
        // fuck yeah terniary operator biyatch

        #endregion

    }

    #endregion

    #region input functions

    /// <summary>
    /// locks cursor when right mouse button is held and changes 
    /// rightMouseButtonDown to true
    /// </summary>
    /// <param name="obj"></param>
    private void RightClickStarted(InputAction.CallbackContext obj)
    {
        Cursor.lockState = CursorLockMode.Locked;
        rightMouseButtonDown = true;
    }

    /// <summary>
    /// unlocks cursor and reverts rightMouseButtonDown
    /// </summary>
    /// <param name="obj"></param>
    private void RightClickCanceled(InputAction.CallbackContext obj)
    {
        Cursor.lockState = CursorLockMode.None;
        rightMouseButtonDown = false;
    }

    #endregion

}
