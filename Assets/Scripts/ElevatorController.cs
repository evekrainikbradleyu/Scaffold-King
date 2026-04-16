/*****************************************************************************
// File Name : ElevatorController.cs
// Author : Eve "Elevatin'" Krainik
// Creation Date : April 14, 2026
//
// Brief Description : Controls elevator scaffolds
*****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    #region variables

    // publics

    public bool elevatorLocked;
    public ScaffoldingController scaffoldingController;
    public PlayerController playerController;
    public Vector3 topPosition;

    // privates

    private int elevatorLevel;
    private bool elevatorMoving;

    // serialized privates

    [SerializeField] private GameObject elevatorLock;
    [SerializeField] private GameObject elevator;
    [SerializeField] private GameObject[] elevatorPoints;

    #endregion

    #region start + update

    /// <summary>
    /// sets variables
    /// </summary>
    private void Start()
    {
        elevatorLocked = true;
        elevatorLevel = 0;
        elevatorMoving = false;
    }

    /// <summary>
    /// sets the player controller if somehow its null (dont ask me why this 
    /// happens)
    /// </summary>
    private void Update()
    {
        if (playerController == null)
        {
            playerController = scaffoldingController.playerController;
        }
    }

    #endregion

    #region coroutines

    /// <summary>
    /// moves elevator
    /// </summary>
    /// <param name="destination">point to move it to</param>
    /// <param name="moveTime">how long it takes to move</param>
    /// <returns>null if going down, else waits for player to move then goes 
    /// down</returns>
    private IEnumerator DoElevator(Transform destination, float moveTime)
    {
        // toggle the solid ground of the top if its not sticking out of the
        // map
        try
        {
            scaffoldingController.map.ToggleSolidGround(topPosition);
        } catch { /**/ }

        // change important variables
        elevatorMoving = true;
        float timeElapsed = 0;
        float elevatorStartY = elevator.transform.position.y;

        // move elevator over given time
        while (timeElapsed < moveTime)
        {
            elevator.transform.position = new Vector3
            (
                elevator.transform.position.x,
                Mathf.Lerp(elevatorStartY, destination.position.y, timeElapsed 
                    / moveTime),
                elevator.transform.position.z
            );

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // make sure elevator ends in the right position
        elevator.transform.position = destination.position;
        elevatorMoving = false;

        // send elevator back down once its brought the player up
        if (destination == elevatorPoints[1].transform)
        {
            yield return StartCoroutine(WaitForPlayerToMove(moveTime));
        }
        // end if was sent to bottom
        else yield return null;
    }

    /// <summary>
    /// makes the elevator go down when the player leaves.
    /// </summary>
    /// <param name="moveTime">time to make the next coroutine take</param>
    /// <returns>starts the elevator moving down when over</returns>
    private IEnumerator WaitForPlayerToMove(float moveTime)
    {
        bool playerMoved = false;
        Vector3 originalPlayerPos = playerController.GetPlayerPosition();

        while (!playerMoved)
        {
            playerMoved = originalPlayerPos != playerController.
                GetPlayerPosition();
            yield return null;
        }

        yield return StartCoroutine(DoElevator(elevatorPoints[0].transform,
                moveTime));
    }

    #endregion

    #region public functions

    /// <summary>
    /// unlocks elevator
    /// </summary>
    public void UnlockElevator()
    {
        elevatorLocked = false;
        Destroy(elevatorLock);
    }

    /// <summary>
    /// starts the elevator moving if the player interacts
    /// </summary>
    /// <param name="destination">index of the point to send elevator to
    /// </param>
    /// <param name="moveTime">time the elevator should take to move</param>
    public void StartElevator(int destination, float moveTime)
    {
        if (elevator.transform.position == elevatorPoints[destination].
            transform.position) { return; }
        if (elevatorMoving) { return; }

        elevatorLevel = destination;
        StartCoroutine(DoElevator(elevatorPoints[destination].transform, 
            moveTime));
    }

    #endregion

    #region monobehaviour functions

    // *crickets*

    #endregion
}
