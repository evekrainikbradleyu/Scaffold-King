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
using Unity.VisualScripting;
using System.Collections.Generic;
using System;

public class PlayerController : MonoBehaviour
{

    #region variables

    // publics
    public int playerYLayer;
    public int keyCount;

    // privates
    private MapController mapController;
    private InputAction move;
    private InputAction leftClick;
    private InputAction interact;
    private InputAction scrollWheel;
    private GameObject currentSpace;
    private GameObject ghostScaffold;
    private Vector2 moveOutput;
    private bool onLadder;
    private bool movingPlayer;
    private bool playerOnSolidGround;
    private bool interactOutput;
    private bool onConveyor;
    private bool onRefill;
    private bool onKey;

    // serialized privates
    [InfoBox("move speed = time taken to move between squares; less is faster")
        ][SerializeField] private float moveSpeed;
    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private GameObject playerStartSpace;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ScaffoldingController scaffoldingController;
    [SerializeField] private UIController uIController;
    [SerializeField] private float scrollSensitivity;

    #endregion

    #region start + update
    /// <summary>
    /// sets up input actions and variables, move player to start position
    /// </summary>
    private void Start()
    {
        // input actions
        SetUpInputActions();

        // variables
        mapController = mapShellController.shellController;
        movingPlayer = false;
        moveOutput = Vector2.zero;
        currentSpace = playerStartSpace;
        onLadder = false;
        playerYLayer = 0;
        ghostScaffold = null;
        keyCount = 0;

        // set player to start position
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

        // just a few debugging things below
        //Debug.Log("Move output: " + moveOutput.ToString());
        //Debug.Log("Interact output: " + interactOutput.ToString());
        //Debug.Log("On ladder: " + onLadder.ToString());
        //Debug.Log("Current Scaffolding: " + scaffoldingController.
        //    currentScaffolding.ToString());
        //Debug.Log("Next Scaffolding: " + scaffoldingController.
        //    nextScaffolding.ToString());

        // makes sure the player isnt floating in midair
        UpdatePlayerOnSolidGround();
        // makes the player fall if they are in fact floating in midair
        DoFalling();
        // checks and moves the player if they're on a conveyor
        DoConveyors();
        // starts any moving if inputs received this frame
        StartInputMovements();
        // starts interactions (like climbing ladders) if inputs received this 
        // frame
        StartInteractions();
        // show ghost scaffolds if in placing mode
        DoGhostScaffolds();
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
        if (PlayerPathBlocked(destination)) { yield break; }

        // gets positions to move to and sets timer
        Vector3 destinationPosition = destination.transform.position;
        Vector3 startPosition = transform.position;
        float timeElapsed = 0;
        movingPlayer = true;

        // moves player over time
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

        // sets player position to destination so it's at the exact spot
        transform.position = new Vector3(destinationPosition.x, transform.
            position.y, destinationPosition.z);
        movingPlayer = false;
        currentSpace = destination;

        yield return null;
    }

    /// <summary>
    /// moves the player upwards (or downwards for falling if spaces is 
    /// negative)
    /// </summary>
    /// <param name="spaces">how many spaces to move up</param>
    /// <param name="moveTime">how long it takes to move</param>
    /// <returns></returns>
    private IEnumerator MoveUp(int spaces, float moveTime)
    {
        // sets timer and gets new position
        movingPlayer = true;
        playerYLayer += spaces;
        float timeElapsed = 0;

        // if player would move up past the top, they win the game 
        if (PlayerAtTop())
        {
            WinLevel();
            yield break;
        }

        // moves player vertically over time
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

        // sets player position to destination so it's at the exact spot
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

        // gets a vector2 to add to the current space's vector2 value to make
        // the player move a single space in that direction
        Vector2 fixedMoveOutput = moveOutput.x == 0 ? moveOutput.y > 0 ?
            new Vector2(0, 1) : new Vector2(0, -1) : moveOutput.x > 0 ? new
            Vector2(1, 0) : new Vector2(-1, 0);

        // depending on what direction the player is facing, fixes the movement
        // to make it so that w always goes forward and asd move in their own
        // respective directions as well
        fixedMoveOutput *= cameraController.nearestWall > 2 ? -1 : 1;
        fixedMoveOutput = cameraController.nearestWall % 2 == 0 ? new
            Vector2(-fixedMoveOutput.y, fixedMoveOutput.x) :
            fixedMoveOutput;

        // gets the space from the current space + the fixed move output
        nextSpace = mapController.GetSpaceFromVector(mapController.
            GetVectorFromSpace(currentSpace) + fixedMoveOutput);

        // old debugging stuff
        /*Debug.Log("next space vector:" + (mapController.
        GetVectorFromSpace(currentSpace) + fixedMoveOutput).ToString());*/

        // ends function if no space was found (player is on edge of board)
        if (nextSpace == null) { return; }

        // starts move routine
        StartCoroutine(MovePlayer(nextSpace, moveSpeed));

    }

    /// <summary>
    /// starts interactions. interactions currently implemented include: 
    /// ladders
    /// </summary>
    private void StartInteractions()
    {
        // prevents interaction if the player is moving
        if (movingPlayer) { return; }

        if (interactOutput)
        {
            if (onLadder) // for ladder scaffolds
            {
                if (BlockAbovePlayer()) { return; }

                StartCoroutine(MoveUp(1, moveSpeed));
            }

            if (onRefill)
            {
                onRefill = false;

                scaffoldingController.RefillScaffolding(GetPlayerPosition());
            }
        }

        if (onKey)
        {
            onKey = false;
            keyCount++;
            scaffoldingController.CollectKey(GetPlayerPosition());
        }
    }

    /// <summary>
    /// detects if there is a building block above the player blocking 
    /// the ladder
    /// </summary>
    /// <returns>true if the player is under a block</returns>
    private bool BlockAbovePlayer()
    {
        if (PlayerAtTop()) { return false; }

        if (scaffoldingController.GetScaffoldPlacement(GetPlayerPosition(),
            offset: Vector3.right) == null) {  return false; }

        if (scaffoldingController.GetScaffoldPlacement(GetPlayerPosition(), 
            offset: Vector3.right).CompareTag("BuildingBlock")) 
        { return true; }

        return false;
    }

    /// <summary>
    /// returns true if player is at the top
    /// </summary>
    /// <returns>true if player is at the top</returns>
    private bool PlayerAtTop() 
    {
        return playerYLayer >= mapController.MapHeight - 1;
    }

    /// <summary>
    /// checks to make sure the player isn't standing on nothing
    /// </summary>
    private void UpdatePlayerOnSolidGround()
    {
        // prevents errors if the solid ground map hasnt been properly assigned
        // yet
        if (scaffoldingController.map.SolidGroundMap == null) { return; }

        // prevents errors if the player has won
        if (playerYLayer > mapController.MapHeight - 1) { return; }

        // gets state of the ground beneath the player currently
        playerOnSolidGround = scaffoldingController.map.SolidGroundMap  
            [playerYLayer]         // y layer
            [(int)GetPlayerPosition().y]        // z position / row
            [(int)GetPlayerPosition().z];       // x position / col
    }

    /// <summary>
    /// makes the player fall when theyre not on solid ground
    /// </summary>
    private void DoFalling()
    {
        // no falling necessary if the player is moving or on solid ground
        if (movingPlayer || playerOnSolidGround) { return; }

        StartCoroutine(MoveUp(-1, moveSpeed));
    }    

    /// <summary>
    /// click function. places scaffolding, usually.
    /// </summary>
    private void OnClick()
    {
        // stops if a gui was clicked or player is moving
        if (EventSystem.current.IsPointerOverGameObject()) { return; }
        if (movingPlayer) { return; }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue
            ());
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit))
        {
            // prevent placing ladders on ladders
            if (PreventLadderPlacement(hit)) { return; }

            if (hit.collider.CompareTag("ScaffoldSpot")) // if a spot suitable
                                                         // for placing
            {                                            // scaffolding is hit
                // stop if not in placing mode
                if (!scaffoldingController.placing) { return; }

                // stop if no scaffolding left
                if (scaffoldingController.scaffoldingRemaining <= 0) { return; 
                }

                bool wasPlaced;

                scaffoldingController.PlaceScaffolding(GetPlacePosition(hit), 
                    out wasPlaced);

                // destroys collider to prevent further placement
                if (!wasPlaced) { return; }
                Destroy(hit.collider);
            }
        }
    }

    /// <summary>
    /// removes all input actions; used when the game is won or when OnDestroy
    /// is executed
    /// </summary>
    private void RemoveInputActions()
    {
        move.performed -= MovePerformed;
        move.canceled -= MoveCanceled;
        interact.performed -= InteractPerformed;
        interact.canceled -= InteractCanceled;
        leftClick.performed -= LeftClickPerformed;
        scrollWheel.performed -= ScrollPerformed;
    }

    /// <summary>
    /// enables the UI's win panel and removes input actions from the player
    /// </summary>
    private void WinLevel()
    {
        uIController.EnableWinPanel();
        RemoveInputActions();
    }

    /// <summary>
    /// sets up all the input actions. goes in start.
    /// </summary>
    private void SetUpInputActions()
    {
        move = InputSystem.actions.FindAction("Move");
        interact = InputSystem.actions.FindAction("Interact");
        leftClick = InputSystem.actions.FindAction("Click");
        scrollWheel = InputSystem.actions.FindAction("ScrollWheel");

        move.performed += MovePerformed;
        move.canceled += MoveCanceled;
        interact.performed += InteractPerformed;
        interact.canceled += InteractCanceled;
        leftClick.performed += LeftClickPerformed;
        scrollWheel.performed += ScrollPerformed;
    }

    /// <summary>
    /// manages ghost scaffolds that are shown before placing
    /// </summary>
    private void DoGhostScaffolds()
    {
        // get rid of current ghost scaffold
        Destroy(ghostScaffold);

        // only activate if in placing mode
        if (!scaffoldingController.placing) { return; }
        if (EventSystem.current.IsPointerOverGameObject()) { return; }
        if (movingPlayer) { return; }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue
           ());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) 
        {
            // doesn't show ladders being placed on ladders
            try 
            {
                if (PreventLadderPlacement(hit)) { return; }
            } catch { } // shows an exception when mouse is over non-scaffold
                        // spot collider
            if (hit.collider.CompareTag("ScaffoldSpot"))
            {

                ghostScaffold = scaffoldingController.PlaceGhostScaffolding(
                    GetPlacePosition(hit));

            }
        }
        else
        {

            ghostScaffold = null;

        }
    }

    /// <summary>
    /// finds the map position to place on for the scaffold placing and ghost 
    /// scaffold placing functions
    /// </summary>
    /// <param name="hit">hit from original raycast</param>
    /// <returns>place position for the scaffolding controller</returns>
    private Vector3 GetPlacePosition(RaycastHit hit)
    {
        return new Vector3
        (
        // height
            mapController.GetHeightFromFloat(hit.collider.gameObject.transform.
                position.y),
        // z value / row
            mapController.GetVectorFromSpace(hit.collider.gameObject).x,
        // x value / col
            mapController.GetVectorFromSpace(hit.collider.gameObject).y
        );
    }

    /// <summary>
    /// returns the current position of the player in the scaffold placement
    /// grid
    /// </summary>
    /// <returns>the current position of the player in the scaffold placement
    /// grid</returns>
    private Vector3 GetPlayerPosition()
    {
        return new Vector3(playerYLayer, mapShellController.shellController.
            GetVectorFromSpace(currentSpace).x, mapShellController.
            shellController.GetVectorFromSpace(currentSpace).y);
    }

    /// <summary>
    /// makes conveyors move the player
    /// </summary>
    private void DoConveyors()
    {
        // prevents from running if player isn't on a conveyor or is moving
        if (!onConveyor || movingPlayer) {  return; }

        GameObject nextSpace = null;

        // finds the conveyor scaffold gameobject
        GameObject playerConveyor = scaffoldingController.GetScaffoldPlacement(
            GetPlayerPosition() - Vector3.right);

        // gets the direction the conveyor is going
        float conveyorRotation = scaffoldingController.GetScaffoldPlacement(
            GetPlayerPosition() - Vector3.right).transform.eulerAngles.y;
        Vector2 conveyorDirection = 
            conveyorRotation == 0 ? new Vector2(-1, 0) :
            conveyorRotation == 90 ? new Vector2(0, -1) :
            conveyorRotation == 180 ? new Vector2(1, 0) :
            /*conveyorRotation == 270 ?*/ new Vector2(0, 1) /*: Vector2.zero*/;

        nextSpace = mapController.GetSpaceFromVector(mapController.
            GetVectorFromSpace(currentSpace) + conveyorDirection);

        if (nextSpace == null) { return; }
        // doesn't activate if there are two conveyors facing each other
        if (OnConveyorsFacingEachother(playerConveyor, conveyorDirection)) { 
            return; }

        StartCoroutine(MovePlayer(nextSpace, moveSpeed));
    }

    /// <summary>
    /// detects if the player is on a conveyor directly facing into another 
    /// conveyor facing toward the original conveyor. prevents a softlock.
    /// </summary>
    /// <param name="playerConveyor">player's conveyor</param>
    /// <param name="conveyorDirection">direction it's facing</param>
    /// <returns>true if the player's conveyor is facing another one that's
    /// also facing it</returns>
    private bool OnConveyorsFacingEachother(GameObject playerConveyor, Vector2 
        conveyorDirection)
    {
        GameObject otherSpace = scaffoldingController.map.ScaffoldingPlacements
            [playerYLayer - 1]
            [(int)mapController.GetVectorFromSpace(currentSpace).x +
            (int)conveyorDirection.x]
            [(int)mapController.GetVectorFromSpace(currentSpace).y +
            (int)conveyorDirection.y];

        if (otherSpace == null) { return false; }
        if (!otherSpace.CompareTag("Conveyor")) { return false; }

        float[] rotations = new float[4] { 0, 90, 180, 270 };
        float[] compatibleRotations = new float[4] { 180, 270, 0, 90 };

        for (int i = 0; i < 4; i++)
        {
            if (playerConveyor.transform.eulerAngles.y == rotations[i] && 
                otherSpace.transform.eulerAngles.y == compatibleRotations[i])
            { return true; }
        }

        return false;
    }

    /// <summary>
    /// returns true if ladder placement needs to be prevented (eg ladder 
    /// placement is useless or on another ladder)
    /// </summary>
    /// <param name="hit">raycasthit from place function</param>
    /// <returns>true if ladder placement should be prevented</returns>
    private bool PreventLadderPlacement(RaycastHit hit)
    {
        return (hit.collider.transform.parent.CompareTag("Ladder") || hit.
            collider.transform.parent.CompareTag("Conveyor")) && 
            scaffoldingController.currentScaffolding == 1;
    }

    private bool PlayerPathBlocked(GameObject targetSpace)
    {
        foreach (Vector3[] i in scaffoldingController.map.MovementBlockades)
        {
            if (i[0].x == playerYLayer)
            {
                if ((new Vector2(i[0].y, i[0].z) == mapController.
                    GetVectorFromSpace(currentSpace) && new Vector2(i[1].y, i[1
                    ].z) == mapController.GetVectorFromSpace(targetSpace)) || (
                    new Vector2(i[0].y, i[0].z) == mapController.
                    GetVectorFromSpace(targetSpace) && new Vector2(i[1].y, i[1]
                    .z) == mapController.GetVectorFromSpace(currentSpace)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region other monobehaviour functions

    /// <summary>
    /// detects when the player is on special spaces
    /// </summary>
    /// <param name="other">space collider</param>
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("entered collider");

        switch (other.gameObject.tag)
        {
            case "Ladder":
                onLadder = true;
                break;

            case "Conveyor":
                onConveyor = true;
                break;

            case "Refill":
                onRefill = true;
                break;

            case "ElevatorKey":
                onKey = true;
                break;
        }
    }

    /// <summary>
    /// detects when the player leaves special spaces
    /// </summary>
    /// <param name="other">space collider</param>
    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Ladder":
                onLadder = false;
                break;

            case "Conveyor":
                onConveyor = false;
                break;

            case "Refill":
                onRefill = false;
                break;

            case "ElevatorKey":
                onKey = false;
                break;
        }
    }

    /// <summary>
    /// prevents input actions from getting screwed up if gameObject is 
    /// destroyed
    /// </summary>
    private void OnDestroy()
    {
        RemoveInputActions();
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

        // this fixes things because i fucked some shit up while making this
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
        //Debug.Log("Left Click Detected");
        OnClick();
    }

    /// <summary>
    /// detects mouse scrolling
    /// </summary>
    /// <param name="obj">input ctx</param>
    private void ScrollPerformed(InputAction.CallbackContext obj)
    {
        // won't register if there's no output or if it's lower than the 
        // sensitivity value
        if (obj.ReadValue<Vector2>() == Vector2.zero) { return; }
        if (Mathf.Abs(obj.ReadValue<Vector2>().y) < scrollSensitivity) { return
                ; }

        //Debug.Log(obj.ReadValue<Vector2>().ToString());
        scaffoldingController.UpdatePlaceDirection(obj.ReadValue<Vector2>().y);
    }

    #endregion
}
