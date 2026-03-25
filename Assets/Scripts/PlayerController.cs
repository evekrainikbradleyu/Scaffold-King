/*****************************************************************************
// File Name : PlayerController.cs
// Author : Eve "Oh Fuck Yeah" Krainik
// Creation Date : March 21, 2026
//
// Brief Description : Allows for control of the player character (movement).
*****************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{

    #region variables

    // publics

    // privates
    private InputAction move;
    private Vector2 moveOutput;
    private MapController mapController;
    private bool movingPlayer;
    private GameObject currentSpace;
    private bool onLadder;
    private InputAction interact;
    private int playerYLayer;
    private bool interactOutput;
    private bool playerOnSolidGround;
    private InputAction leftClick;

    // serialized privates
    [InfoBox("move speed = time taken to move between squares; less is faster")
        ][SerializeField] private float moveSpeed;
    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private GameObject playerStartSpace;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ScaffoldingController scaffoldingController;

    #endregion

    #region start + update
    /// <summary>
    /// sets up input actions and variables, move player to start position
    /// </summary>
    private void Start()
    {
        move = InputSystem.actions.FindAction("Move");
        interact = InputSystem.actions.FindAction("Interact");
        leftClick = InputSystem.actions.FindAction("Left Click");

        move.performed += MovePerformed;
        move.canceled += MoveCanceled;
        interact.performed += InteractPerformed;
        interact.canceled += InteractCanceled;
        leftClick.performed += LeftClickPerformed;

        mapController = mapShellController.shellController;
        movingPlayer = false;
        moveOutput = Vector2.zero;
        currentSpace = playerStartSpace;
        onLadder = false;
        playerYLayer = 0;

        transform.position = new Vector3
        (
            playerStartSpace.transform.position.x, 
            mapController.MapStartHeight, 
            playerStartSpace.transform.position.z
        );
    }



    /// <summary>
    /// checks for input and starts movement routines + interactions based upon 
    /// that input
    /// </summary>
    private void Update()
    {
        //Debug.Log("Move output: " + moveOutput.ToString());
        //Debug.Log("Interact output: " + interactOutput.ToString());
        //Debug.Log("On ladder: " + onLadder.ToString());

        UpdatePlayerOnSolidGround();
        DoFalling();
        StartInputMovements();
        StartInteractions();
    }

    #endregion

    #region coroutines

    /// <summary>
    /// moves the player to the given space over the given amount of time
    /// </summary>
    /// <param name="destination">space to move to</param>
    /// <param name="moveTime">how long it takes to move</param>
    /// <returns></returns>
    private IEnumerator MovePlayer(GameObject destination, float moveTime)
    {
        Vector3 destinationPosition = destination.transform.position;
        Vector3 startPosition = transform.position;
        float timeElapsed = 0;
        movingPlayer = true;

        while (timeElapsed < moveTime)
        {
            transform.position = new Vector3
            (
                Mathf.Lerp(startPosition.x, destinationPosition.x, timeElapsed/
                    moveTime),
                transform.position.y,
                Mathf.Lerp(startPosition.z, destinationPosition.z, timeElapsed/ 
                    moveTime)
            );

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(destinationPosition.x, transform.
            position.y, destinationPosition.z);
        movingPlayer = false;
        currentSpace = destination;

        yield return null;
    }

    /// <summary>
    /// moves the player upwards
    /// </summary>
    /// <param name="spaces">how many spaces to move up</param>
    /// <param name="moveTime">how long it takes to move</param>
    /// <returns></returns>
    private IEnumerator MoveUp(int spaces, float moveTime)
    {
        movingPlayer = true;
        playerYLayer += spaces;
        float timeElapsed = 0;

        while (timeElapsed < moveTime)
        {
            transform.position = new Vector3
            (
                transform.position.x,
                Mathf.Lerp(mapController.MapLayerHeights[playerYLayer - spaces]
                    , mapController.MapLayerHeights[playerYLayer], timeElapsed 
                    / moveTime),
                transform.position.z
            );

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, mapController.
            MapLayerHeights[playerYLayer], transform.position.z);
        movingPlayer = false;

        yield return null;
    }

    #endregion

    #region miscellaneous functions

    /// <summary>
    /// checks to make sure move output is there and the player isnt already 
    /// moving, then moves the player to the next space based on the move 
    /// output.
    /// </summary>
    private void StartInputMovements()
    {
        // stops function if player is moving or there isnt move input
        if (moveOutput == Vector2.zero || movingPlayer) { return; }


        GameObject nextSpace = null;

        Vector2 fixedMoveOutput = moveOutput.x == 0 ? moveOutput.y > 0 ?
            new Vector2(0, 1) : new Vector2(0, -1) : moveOutput.x > 0 ? new
            Vector2(1, 0) : new Vector2(-1, 0);

        fixedMoveOutput *= cameraController.nearestWall > 2 ? -1 : 1;
        fixedMoveOutput = cameraController.nearestWall % 2 == 0 ? new
            Vector2(-fixedMoveOutput.y, fixedMoveOutput.x) :
            fixedMoveOutput;

        nextSpace = mapController.GetSpaceFromVector(mapController.
            GetVectorFromSpace(currentSpace) + fixedMoveOutput);

        /*Debug.Log("next space vector:" + (mapController.
        GetVectorFromSpace(currentSpace) + fixedMoveOutput).ToString());*/

        if (nextSpace == null) { return; }

        StartCoroutine(MovePlayer(nextSpace, moveSpeed));

    }

    /// <summary>
    /// starts interactions. interactions currently implemented include: 
    /// ladders
    /// </summary>
    private void StartInteractions()
    {
        if (movingPlayer) { return; }

        if (interactOutput)
        {
            if (onLadder)
            {
                StartCoroutine(MoveUp(1, moveSpeed));
            }
        }
    }

    /// <summary>
    /// checks to make sure the player isn't standing on nothing
    /// </summary>
    private void UpdatePlayerOnSolidGround()
    {
        if (scaffoldingController.map.SolidGroundMap == null) { return; }

        playerOnSolidGround = scaffoldingController.map.SolidGroundMap  
            [playerYLayer]
            [(int)mapShellController.shellController.GetVectorFromSpace(
                currentSpace).x]
            [(int)mapShellController.shellController.GetVectorFromSpace(
                currentSpace).y];
    }

    private void DoFalling()
    {
        if (movingPlayer || playerOnSolidGround) { return; }

        StartCoroutine(MoveUp(-1, moveSpeed));
    }    

    private void OnClick()
    {
        Debug.Log("Clicked");

        if (EventSystem.current.IsPointerOverGameObject()) { return; }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue
            ());
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit))
        {
            if (hit.collider.CompareTag("ScaffoldSpot"))
            {
                scaffoldingController.PlaceScaffolding
                (
                    0,
                    new Vector3
                    (
                    0,
                    mapController.GetVectorFromSpace(hit.collider.gameObject).x,
                    mapController.GetVectorFromSpace(hit.collider.gameObject).y
                    )
                );
            }
        }
    }

    #endregion

    #region other monobehaviour functions

    /// <summary>
    /// detects when the player is on special spaces
    /// </summary>
    /// <param name="other">space collider</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ladder"))
        {
            onLadder = true;
        }
    }

    /// <summary>
    /// detects when the player leaves special spaces
    /// </summary>
    /// <param name="other">space collider</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ladder"))
        {
            onLadder = false;
        }
    }

    #endregion

    #region input functions

    /// <summary>
    /// detects when the player stops move input
    /// </summary>
    /// <param name="obj">input ctx</param>
    private void MoveCanceled(InputAction.CallbackContext obj)
    {
        moveOutput = Vector2.zero;
    }

    /// <summary>
    /// detects when the player performs move input
    /// </summary>
    /// <param name="obj">input ctx</param>
    private void MovePerformed(InputAction.CallbackContext obj)
    {
        moveOutput = obj.ReadValue<Vector2>();
        moveOutput = new Vector2 (moveOutput.y, moveOutput.x);
    }

    /// <summary>
    /// detects when the player releases the space bar
    /// </summary>
    /// <param name="obj">input ctx</param>
    private void InteractCanceled(InputAction.CallbackContext obj)
    {
        interactOutput = false;
    }

    /// <summary>
    /// detects when the player presses the space bar
    /// </summary>
    /// <param name="obj">input ctx</param>
    private void InteractPerformed(InputAction.CallbackContext obj)
    {
        interactOutput = true;
    }

    /// <summary>
    /// detects when the player clicks the left mouse button
    /// </summary>
    /// <param name="obj">input ctx</param>
    private void LeftClickPerformed(InputAction.CallbackContext obj)
    {
        Debug.Log("Left Click Detected");
        OnClick();
    }

    #endregion
}
