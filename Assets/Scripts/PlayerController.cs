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

    // serialized privates
    [InfoBox("move speed = time taken to move between squares; less is faster"
        )][SerializeField] private float moveSpeed;
    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private GameObject playerStartSpace;
    [SerializeField] private CameraController cameraController;

    #endregion

    #region start + update
    /// <summary>
    /// sets up input actions and variables, move player to start position
    /// </summary>
    private void Start()
    {
        move = InputSystem.actions.FindAction("Move");

        move.performed += MovePerformed;
        move.canceled += MoveCanceled;

        mapController = mapShellController.shellController;
        movingPlayer = false;
        moveOutput = Vector2.zero;
        currentSpace = playerStartSpace;

        transform.position = new Vector3
        (
            playerStartSpace.transform.position.x, 
            mapController.MapStartHeight, 
            playerStartSpace.transform.position.z
        );
    }

    private void Update()
    {
        //Debug.Log("Move output: " + moveOutput.ToString());

        if (moveOutput != Vector2.zero && !movingPlayer)
        {
            GameObject nextSpace = null;

            Vector2 fixedMoveOutput = moveOutput.x == 0 ? moveOutput.y > 0 ? 
                new Vector2(0, 1) : new Vector2(0, -1) : moveOutput.x > 0 ? new 
                Vector2(1, 0) : new Vector2(-1, 0);

            fixedMoveOutput *= cameraController.nearestWall > 2 ? -1 : 1;
            fixedMoveOutput = cameraController.nearestWall % 2 == 0 ? new 
                Vector2(-fixedMoveOutput.y, fixedMoveOutput.x) : fixedMoveOutput
                ;

            nextSpace = mapController.GetSpaceFromVector(mapController.
                GetVectorFromSpace(currentSpace) + fixedMoveOutput);

            Debug.Log("next space vector:" + (mapController.GetVectorFromSpace(currentSpace) + fixedMoveOutput).ToString());
            
            if (nextSpace != null)
            {
                StartCoroutine(MovePlayer(nextSpace, moveSpeed));
            }
        }
    }

    #endregion

    #region coroutines

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

    #endregion

    #region input functions

    private void MoveCanceled(InputAction.CallbackContext obj)
    {
        moveOutput = Vector2.zero;
    }

    private void MovePerformed(InputAction.CallbackContext obj)
    {
        moveOutput = obj.ReadValue<Vector2>();
        moveOutput = new Vector2 (moveOutput.y, moveOutput.x);
    }

    #endregion
}
