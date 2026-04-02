/*****************************************************************************
// File Name : CameraController.cs
// Author : Eve "Goddess of Cameras" Krainik
// Creation Date : March 10, 2026
//
// Brief Description : Makes the camera circle around the position of the 
CameraTrack game object when right click is held down.
*****************************************************************************/

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{

    #region variables

    // publics
    public int nearestWall;

    // privates
    private InputAction rightClick;
    private Vector3 cameraOffset;
    private bool rightMouseButtonDown;
    private float yOffset;
    private Vector2 mouseDelta;
    private float mouseXDelta;
    private float mouseYDelta;

    // serialized privates
    [SerializeField] private GameObject cameraTrack;
    [SerializeField] private float cameraRotateSpeed;
    [SerializeField] private GameObject mapWalls;
    [SerializeField] private GameObject player;
    [SerializeField] private float smoothingTime;
    [SerializeField] private Vector2 verticalConstraints;

    #endregion

    #region start and update

    /// <summary>
    /// sets up variables and input actions
    /// </summary>
    private void Start()
    {
        cameraOffset = transform.position - cameraTrack.transform.position;
        rightMouseButtonDown = false;
        yOffset = transform.position.y-player.transform.position.y;

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
        // sets new mouse delta values
        GetMouseDelta();

        // rotates camera with cursor while holding right click
        if (rightMouseButtonDown)
        {
            transform.RotateAround(cameraTrack.transform.position, Vector3.up, 
                cameraRotateSpeed * mouseXDelta);
        }

        // check summary lol
        UpdateCameraY();

        #region wall shenanigans

        // make map walls transparent when the camera is behind them
        mapWalls.transform.Find("Wall1").gameObject.SetActive(!(transform.
            position.z <= 0));
        mapWalls.transform.Find("Wall3").gameObject.SetActive(!(transform.
            position.z > 0));
        mapWalls.transform.Find("Wall2").gameObject.SetActive(!(transform.
            position.x >= 0));
        mapWalls.transform.Find("Wall4").gameObject.SetActive(!(transform.
            position.x < 0));

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
        // fuck yeah ternary operator biyatch

        #endregion

    }

    #endregion

    #region miscellaneous functions

    /// <summary>
    /// makes camera follow player up and down
    /// </summary>
    private void UpdateCameraY()
    {
        float yVel = 0;

        float newY = Mathf.SmoothDamp(transform.position.y, player.transform.
            position.y + yOffset, ref yVel, smoothingTime);

        transform.position = new Vector3(transform.position.x, newY, transform.
            position.z);

        cameraTrack.transform.position = new Vector3(cameraTrack.transform.
            position.x, 1 + player.transform.position.y / 2, cameraTrack.
            transform.position.z);

        transform.LookAt(cameraTrack.transform);
    }

    private void GetMouseDelta()
    {
        mouseDelta = Mouse.current.delta.ReadValue();
        mouseXDelta = mouseDelta.x;
        mouseYDelta = mouseDelta.y;
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
