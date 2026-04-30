/*****************************************************************************
// File Name : ScaffoldingController.cs
// Author : Eve "I <3 Scaffolding" Krainik
// Creation Date : March 25, 2026
//
// Brief Description : Controls all scaffolding in the game!! Referenced by 
// most scripts that seek a mere fraction of its power.
*****************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine.PlayerLoop;
using Unity.VisualScripting;

public class ScaffoldingController : MonoBehaviour
{

    #region variables

    // publics
    public ScaffoldMap map;
    public int currentScaffolding;
    public int nextScaffolding;
    public int scaffoldingRemaining; // assign in inspector
    public bool placing;
    public GameObject[] scaffoldTypes;
    public PlayerController playerController;
    public int level;
    

    // privates
    private MapController mapController;
    private int placeDirection;
    private int levelSection;
    private bool encounteredElevator;

    // serialized privates
    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private Vector2[] scaffoldRarities;
    [SerializeField] private GameObject[] ghostScaffolds;
    [SerializeField] private GameObject scaffoldFiller;
    [SerializeField] private GameObject scaffoldFillerWithSpot;
    [SerializeField] private int startingScaffoldingCount;
    [SerializeField] private int scaffoldingPerRefill;
    

    #endregion

    #region start

    /// <summary>
    /// makes the map and sets up the current and next scaffold. also sets 
    /// variables.
    /// </summary>
    private void Start()
    {
        mapController = mapShellController.shellController;

        map = new ScaffoldMap(mapController);

        scaffoldingRemaining = startingScaffoldingCount;

        currentScaffolding = 1;
        
        placing = false;

        placeDirection = 0;

        levelSection = 0;
        ChangeRarities();

        SetUpLevel();

        nextScaffolding = GetNextScaffolding();

        encounteredElevator = level != 2;
    }

    #endregion

    #region public functions

    /// <summary>
    /// places a piece of scaffolding.
    /// </summary>
    /// <param name="position">the position on the map to place the 
    /// scaffolding; x = height, y = row, z = col</param>
    /// <param name="wasPlaced">returns true if placement was successful
    /// </param>
    /// <param name="forceScaffolding">force a certain type of scaffolding 
    /// instead of the current one</param>
    public void PlaceScaffolding(Vector3 position, out bool wasPlaced, int 
        forceScaffolding = -1)
    {
        int scaffold = forceScaffolding > -1 ? forceScaffolding : currentScaffolding;

        wasPlaced = false;

        // stop if theres already scaffolding there
        if (CheckForScaffolding(position, scaffold)) { return; }
        // stops if scaffolds are overlapping
        if (DetectOverlappingScaffolds(position, scaffold)) { return; }
        // stops if scaffold is unplaceable
        if (!CanPlaceScaffold(position, scaffold)) { return; }

        // instantiate scaffolding
        SetScaffoldPlacement(position, Instantiate(scaffoldTypes[
            scaffold]));
        // set position of scaffolding
        GetScaffoldPlacement(position).transform.position = SetPlacePosition(
            position);
        // set rotation based on scroll
        GetScaffoldPlacement(position).transform.eulerAngles = new Vector3(0, 
            placeDirection * 90, 0);

        // checks to see if scaffold type is 1x1, update when new types added
        if (ScaffoldIs1x1(scaffold))
        {
            if (position.x < mapController.MapHeight - 1)
            {
                map.ToggleSolidGround(position + Vector3.right);
            }

            // prevent movement between spaces if its a blocked scaffold or 
            // building block

            Vector3[] dirs;
            
            switch (scaffold)
            {
                case 5:
                    bool blockadesBetweenRows = placeDirection % 2 == 0;
                    map.AddBlockade(position, (blockadesBetweenRows ? Vector3.up :
                        Vector3.forward));
                    map.AddBlockade(position, (blockadesBetweenRows ? Vector3.down
                        : Vector3.back));
                    break;
                case 6:
                    dirs = new Vector3[] { Vector3.up, Vector3.forward, Vector3.
                            down, Vector3.back};
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        map.AddBlockade(position, dirs[i]);
                    }
                    dirs = null;
                    break;
                case 7:

                    Transform scaffoldTransform = GetScaffoldPlacement(position
                        ).transform;

                    Vector3 dirA = scaffoldTransform.forward;
                    Vector3 dirB = -scaffoldTransform.right;

                    map.AddBlockade(position, new Vector3(0, Mathf.Round(dirA.z
                        ), Mathf.Round(dirA.x)));
                    map.AddBlockade(position, new Vector3(0, Mathf.Round(dirB.z
                        ), Mathf.Round(dirB.x)));
                    break;
            }
        }
        else if (ScaffoldIsTall(scaffold))
        {
            // make sure top isn't null
            SetScaffoldPlacement(position, Instantiate(scaffoldFiller), offset:
                Vector3.right); // Vector3.right == (1,0,0)

            if (scaffold == 10)
            {
                // assign elevator controller's scripts
                GetScaffoldPlacement(position).GetComponent<ElevatorController>
                    ().scaffoldingController = this;
                GetScaffoldPlacement(position).GetComponent<ElevatorController>
                    ().playerController = playerController;

                GetScaffoldPlacement(position).GetComponent<ElevatorController>
                    ().topPosition = position + Vector3.right * 2;
            }

            if (position.x < mapController.MapHeight - 2 && scaffold != 10)
            {   // if scaffold is elevator, doesnt toggle solid ground
                //Debug.Log("toggling solid ground");
                map.ToggleSolidGround(position + Vector3.right * 2);
            }
        }
        else if (ScaffoldIsLong(scaffold))
        {
            // gets the offset as well as the one to use for the 3d grid
            Vector2 directionOffset = GetLongPlaceOffset();
            Vector3 directionOffset3D = new Vector3(0, directionOffset.x,
                directionOffset.y);

            // stops if scaffold is being placed out of bounds; may be
            // irrelevant but i don't feel like making sure it isn't right now
            if (!IsWithinBounds(position, directionOffset3D)) { return; }

            // set scaffold spot position and such since there are two spots to
            // place
            SetScaffoldPlacement(position, Instantiate(scaffoldFillerWithSpot),
                offset: directionOffset3D);
            GetScaffoldPlacement(position, offset: directionOffset3D).transform
                .position = SetPlacePosition(position + directionOffset3D);

            // set solid grounds

            if (position.x < mapController.MapHeight - 1)
            {
                map.ToggleSolidGround(position + Vector3.right);
                map.ToggleSolidGround(new Vector3
                    (
                        position.x + 1,
                        position.y + directionOffset.x,
                        position.z + directionOffset.y
                    ));
            }


        }

        // dont cycle if was force placed
        if (forceScaffolding == scaffold) { return; }

        // remove one scaffolding, get new current + next
        scaffoldingRemaining--;
        CycleScaffolding();

        // toggles place mode to false for next scaffolding
        wasPlaced = true;
        placing = false;
    }

    /// <summary>
    /// overload that uses no out var; used for manual placement in scripts
    /// </summary>
    /// <param name="position">position</param>
    /// <param name="forceScaffolding">scaffolding to force</param>
    public void PlaceScaffolding(Vector3 position, int forceScaffolding = -1)
    {
        PlaceScaffolding(position, out var _, forceScaffolding);
    }

    /// <summary>
    /// overload for placing terrain
    /// </summary>
    /// <param name="x">xpos</param>
    /// <param name="y">ypos</param>
    /// <param name="z">zpos</param>
    /// <param name="forceScaffolding">scaffolding type</param>
    public void PlaceScaffolding(int x, int y, int z, int forceScaffolding)
    {
        PlaceScaffolding(new Vector3(x,y,z), forceScaffolding);
    }

    /// <summary>
    /// used to place ghost scaffolding exclusively; very similar to normal
    /// place function.
    /// </summary>
    /// <param name="position">position to place on in the grid</param>
    /// <returns>ghost scaffold for placement in PlayerController</returns>
    public GameObject PlaceGhostScaffolding(Vector3 position)
    {
        // stop if theres already scaffolding there
        if (CheckForScaffolding(position, currentScaffolding)) { return null; }

        if (DetectOverlappingScaffolds(position, currentScaffolding)) { return 
                null; }

        if (!CanPlaceScaffold(position, currentScaffolding)) { return null; }

        // instantiate
        GameObject ghostScaffold = Instantiate(ghostScaffolds[
            currentScaffolding]);

        // set position
        ghostScaffold.transform.position = SetPlacePosition(position);

        // set rotation based on scroll
        ghostScaffold.transform.eulerAngles = new Vector3(0, placeDirection * 
            90, 0);

        return ghostScaffold;
    }

    /// <summary>
    /// returns the scaffold at a given position + optional offset
    /// </summary>
    /// <param name="position">the position to return for</param>
    /// <param name="offset">optional offset; used for larger scaffolds</param>
    /// <returns>the scaffold at the position + offset</returns>
    public ref GameObject GetScaffoldPlacement(Vector3 position, Vector3 
        offset = default) // default is ok here since Vector3s default to 0,0,0
    {
        return ref map.ScaffoldingPlacements[(int)position.x + (int)offset.x][(
            int)position.y + (int)offset.y][(int)position.z + (int)offset.z];
    }

    /// <summary>
    /// updates the rotation for scaffolds being placed; called in 
    /// PlayerController
    /// </summary>
    /// <param name="scrollInput">the mouse's scroll wheel input from 
    /// PlayerController</param>
    public void UpdatePlaceDirection(float scrollInput)
    {
        placeDirection += scrollInput > 0 ? 1 : -1;
        placeDirection = placeDirection < 0 ? 3 : placeDirection > 3 ? 0 : 
            placeDirection;
    }

    /// <summary>
    /// function to call in PlayerController when the player collects from a 
    /// refill scaffold.
    /// </summary>
    /// <param name="refillSpace">reference to the space of the player</param>
    public void RefillScaffolding(Vector3 refillSpace)
    {
        scaffoldingRemaining += scaffoldingPerRefill;

        Destroy(GetScaffoldPlacement(refillSpace).GetComponent("BoxCollider"));
        Destroy(GetScaffoldPlacement(refillSpace).transform.Find("boxes").
            gameObject);

        ChangeRarities();
    }



    /// <summary>
    /// function to call in PlayerController when the player collects a key
    /// </summary>
    /// <param name="collectSpace">reference to the space of the player</param>
    public void CollectKey(Vector3 collectSpace)
    {
        Destroy(GetScaffoldPlacement(collectSpace, Vector3.left).GetComponent(
            "BoxCollider"));
        Destroy(GetScaffoldPlacement(collectSpace, Vector3.left).transform.Find
            ("key").gameObject);
    }

    /// <summary>
    /// returns true if the player has seen the elevator yet; used for tutorial
    /// </summary>
    /// <returns>true if the player has seen the elevator yet</returns>
    public bool PlayerHasEncounteredElevator()
    {
        return encounteredElevator;
    }

    #endregion

    #region private functions

    /// <summary>
    /// checks to see if scaffold is placeable in the given position
    /// </summary>
    /// <param name="position">the given position</param>
    /// <param name="scaffold">the type of scaffold</param>
    /// <returns>true if there will be no issues placing the scaffolding
    /// </returns>
    private bool CanPlaceScaffold(Vector3 position, int scaffold)
    {
        // if out of bounds cannot place
        if (!IsWithinBounds(position, Vector3.zero)) { return false; }

        // if the longer part is out of bounds cannot place
        if (ScaffoldIsLong(scaffold))
        {
            Vector2 dir = GetLongPlaceOffset();
            Vector3 offset = new Vector3(0, dir.x, dir.y);

            if (!IsWithinBounds(position, offset: offset)) { return false; }
        }

        // if the upper part is out of bounds cannot place
        if (ScaffoldIsTall(scaffold))
        {
            if (!IsWithinBounds(position, offset: Vector3.right)) { return false; }
        }

        // else
        return true;
    }

    /// <summary>
    /// checks to make sure a given position is within bounds
    /// </summary>
    /// <param name="position">the given position</param>
    /// <param name="offset">offset; used for long and tall scaffolding</param>
    /// <returns>true if the position is within bounds</returns>
    private bool IsWithinBounds(Vector3 position, Vector3 offset = default)
    {
        // get positions first because its easier to read
        int x = (int)position.x + (int)offset.x;
        int y = (int)position.y + (int)offset.y;
        int z = (int)position.z + (int)offset.z;

        // makes sure it isn't out of bounds
        return x >= 0 && x < mapController.MapHeight &&
               y >= 0 && y < mapController.Rows.Count &&
               z >= 0 && z < mapController.Spaces[0].Count;
    }

    /// <summary>
    /// gets the offset vector for the placement of long scaffolding
    /// </summary>
    /// <returns>the offset vector for the placement of long scaffolding
    /// </returns>
    private Vector2 GetLongPlaceOffset()
    {
        return placeDirection == 0 ? Vector2.down :
                placeDirection == 1 ? Vector2.right : placeDirection == 2 ?
                Vector2.up : Vector2.left;
    }

    /// <summary>
    /// sees if any scaffolds are overlapping
    /// </summary>
    /// <param name="position">position to check from</param>
    /// <param name="scaffold">the type of scaffold</param>
    /// <returns>whether any scaffolds in the next scaffold placement are 
    /// overlapping</returns>
    private bool DetectOverlappingScaffolds(Vector3 position, int scaffold)
    {
        // cannot overlap; if it does then there's an obvious issue somewhere
        if (ScaffoldIs1x1(scaffold)) { return false; }

        // makes sure not overlapping with something above it
        if (ScaffoldIsTall(scaffold))
        {
            return GetScaffoldPlacement(position, offset: Vector3.right) != 
                null; // vector3.right is just 1,0,0 so its easier
        }

        // makes sure not overlapping with something to the side of it
        if (ScaffoldIsLong(scaffold))
        {
            Vector2 dir = GetLongPlaceOffset();
            Vector3 offset = new Vector3(0, dir.x, dir.y);
            
            // stops if it sticks out of bounds
            if (!IsWithinBounds(position, offset)) return true;

            // makes sure theres no scaffold in the spot that sticks out
            return GetScaffoldPlacement(position, offset) != null;
        }

        // hopefully it doesn't get to this point
        Debug.Log("scaffold type not implemented in DetectOverlappingScaffolds"
            );
        return false;
    }

    /// <summary>
    /// directly assigns scaffold to a given position + optional offset
    /// </summary>
    /// <param name="position">the position to set</param>
    /// <param name="newObject">the object (scaffold) to set it to</param>
    /// <param name="offset">optional offset; used to place filler objects at 
    /// parts of larger scaffolds so they don't return null</param>
    private void SetScaffoldPlacement(Vector3 position, GameObject newObject, 
        Vector3 offset = default) // same deal as GetScaffoldPlacement
    {
        map.ScaffoldingPlacements[(int)position.x + (int)offset.x][(int)
            position.y + (int)offset.y][(int)position.z + (int)offset.z] = 
            newObject;

        // makes sure to get rid of lower scaffold spots when placing a spot 
        // filler
        if (newObject.CompareTag("SpotFiller"))
        {
            //Debug.Log(position.ToString());

            // if on the floor gets floor spot instead of scaffold spot
            if (position.x + offset.x == 0)
            {
                Destroy(mapShellController.shellController.Spaces[(int)position
                    .y + (int)offset.y][(int)position.z + (int)offset.z].
                    transform.GetComponent<BoxCollider>());
            }
            else
            {
                var lowerScaffold = GetScaffoldPlacement(position, offset: 
                    Vector3.left);

                if (lowerScaffold != null)
                {
                    var spot = lowerScaffold.transform.Find("ScaffoldSpot");

                    if (spot != null)
                    {
                        var collider = spot.GetComponent<BoxCollider>();

                        if (collider != null)
                        {
                            Destroy(collider);
                        }
                    }
                }

            }
        }
    }

    /// <summary>
    /// cycles through scaffolding after placing
    /// </summary>
    private void CycleScaffolding()
    {
        currentScaffolding = nextScaffolding;
        nextScaffolding = GetNextScaffolding();
    }

    /// <summary>
    /// returns the vector3 position to set the transform position of
    /// scaffolding and ghost scaffolding to
    /// </summary>
    /// <param name="position">position on the scaffolding grid</param>
    /// <returns>the vector3 position to set the transform position of
    /// scaffolding and ghost scaffolding to</returns>
    private Vector3 SetPlacePosition(Vector3 position)
    {
        return new Vector3
        (
            mapController.GetSpaceFromVector(new Vector2(position.y, position.z
                )).transform.position.x, // gets space x position
            mapController.MapLayerHeights[(int)position.x], // gets height
            mapController.GetSpaceFromVector(new Vector2(position.y, position.z
                )).transform.position.z // gets space z position
        );
    }

    /// <summary>
    /// returns true if theres already scaffolding in the selected spot
    /// </summary>
    /// <param name="position">the selected position on the scaffold grid
    /// </param>
    /// <param name="scaffold">type of scaffold</param>
    /// <returns>whether or not theres already scaffolding in the selected spot
    /// </returns>
    private bool CheckForScaffolding(Vector3 position, int scaffold)
    {
        return ScaffoldIs1x1(scaffold) ? GetScaffoldPlacement(position) != null :
            ScaffoldIsTall(scaffold) ? GetScaffoldPlacement(position) != null && 
            GetScaffoldPlacement(position, offset: Vector3.right) != null : 
            false;
    }

    /// <summary>
    /// checks if current scaffold is basic 1x1x1; 1x1x1 scaffolds currently 
    /// include: 
    /// 0. normal scaffolds 
    /// 1. ladder scaffolds 
    /// 3. conveyor scaffolds
    /// 5. two way scaffolds 
    /// 6. building blocks
    /// 7. corner scaffolds
    /// 8. refill scaffolds
    /// 9. key scaffolds
    /// </summary>
    /// <param name="scaffold">type of scaffold</param>
    /// <returns>true if the scaffold is 1x1x1</returns>
    private bool ScaffoldIs1x1(int scaffold)
    {
        return Array.Exists<int>(new int[] { 0, 1, 3, 5, 6, 7, 8, 9 }, 
            i => i == scaffold);
    }

    /// <summary>
    /// checks if current scaffold is tall (2 spaces tall). currently includes:
    /// 2. tall scaffolds
    /// 10. elevator scaffolds
    /// </summary>
    /// <param name="scaffold">type of scaffold</param>
    /// <returns>true if scaffold is tall</returns>
    private bool ScaffoldIsTall(int scaffold)
    {
        return Array.Exists<int>(new int[] { 2, 10 }, i => i == 
            scaffold);
    }

    /// <summary>
    /// checks if current scaffold is long (2 spaces long). currently includes:
    /// 4. long scaffolds
    /// </summary>
    /// <param name="scaffold">type of scaffold</param>
    /// <returns></returns>
    private bool ScaffoldIsLong(int scaffold)
    {
        return Array.Exists<int>(new int[] { 4 }, i => i == scaffold)
            ;
    }

    /// <summary>
    /// gets a new scaffolding for the next pull
    /// </summary>
    /// <returns>a random scaffolding based off of rarities</returns>
    private int GetNextScaffolding()
    {
        List<int> scaffoldRanges = new List<int>();

        foreach (Vector2 i in scaffoldRarities)
        {
            for (int j = 0; j < (int)i.y;  j++)
            {
                scaffoldRanges.Add((int)i.x);
            }
        }

        int selected = scaffoldRanges[UnityEngine.Random.Range(0, 
            scaffoldRanges.Count)];

        // increase rarities of less used scaffolds 
        for (int i = 0; i < scaffoldRarities.Length; i++)
        {
            switch (selected)
            {
                case 1:
                    AdjustRarities(scaffoldRarities[i].x != 1, scaffoldRarities
                        [i].x == 1, 1, i);
                    break;
                case 9:
                    AdjustRarities(scaffoldRarities[i].x == 10, 
                        scaffoldRarities[i].x == 9, 2, i);
                    break;
                case 10:
                    AdjustRarities(scaffoldRarities[i].x == 9, scaffoldRarities
                        [i].x == 10, 2, i);
                    break;
                default:
                    AdjustRarities(scaffoldRarities[i].x == 1, false, 1, i);
                    break;
            }
        }
         
        return selected;
    }

    /// <summary>
    /// used when picking the next scaffolding to adjust the rarities of 
    /// different scaffolds. makes things less monotonous and less likely for 
    /// the player to run into issues.
    /// </summary>
    /// <param name="increaseCondition">the condition that the current 
    /// iteration is increased under</param>
    /// <param name="decreaseCondition">the condition that the current 
    /// iteration is decreased under (minimum remains as 1 always)</param>
    /// <param name="changeAmount">how much to increase and decrease by</param>
    /// <param name="i">the iteration variable in GetNextScaffolding()</param>
    private void AdjustRarities(bool increaseCondition, bool decreaseCondition,
        int changeAmount, int i)
    {
        if (increaseCondition && scaffoldRarities[i].y > 0)
        {
            scaffoldRarities[i] = new Vector2(scaffoldRarities[i].x, 
                scaffoldRarities[i].y + changeAmount);
        }
        else if (decreaseCondition)
        {
            scaffoldRarities[i] = new Vector2(scaffoldRarities[i].x, Mathf.Max(
                1, scaffoldRarities[i].y - changeAmount));
        }
    }

    /// <summary>
    /// sets up the terrain and refills depending on the level
    /// </summary>
    private void SetUpLevel()
    {
        switch (level)
        {
            case 0:

                // most scaffolding (level is 8 tall)

                PlaceScaffolding(0, 4, 0, 0);
                PlaceScaffolding(0, 5, 5, 2);
                PlaceScaffolding(0, 5, 4, 2);
                PlaceScaffolding(0, 5, 3, 2);
                PlaceScaffolding(0, 5, 2, 2);
                PlaceScaffolding(0, 5, 1, 2);
                PlaceScaffolding(0, 5, 0, 2);
                PlaceScaffolding(0, 4, 5, 2);
                PlaceScaffolding(0, 3, 5, 2);
                PlaceScaffolding(0, 2, 5, 2);
                PlaceScaffolding(0, 4, 4, 2);
                PlaceScaffolding(0, 3, 4, 2);
                PlaceScaffolding(0, 2, 4, 2);
                PlaceScaffolding(2, 5, 5, 0);
                PlaceScaffolding(2, 4, 5, 0);
                PlaceScaffolding(2, 3, 5, 0);
                PlaceScaffolding(2, 2, 5, 0);
                PlaceScaffolding(2, 5, 4, 0);
                PlaceScaffolding(2, 4, 4, 0);
                PlaceScaffolding(2, 3, 4, 0);
                PlaceScaffolding(2, 2, 4, 0);
                PlaceScaffolding(3, 3, 5, 8);

                break;
            case 1:

                // first refill
                PlaceScaffolding(9, 2, 5, 6);
                PlaceScaffolding(9, 1, 5, 6);
                PlaceScaffolding(9, 0, 5, 6);
                PlaceScaffolding(9, 1, 4, 6);
                PlaceScaffolding(9, 0, 4, 6);
                PlaceScaffolding(9, 1, 3, 6);
                PlaceScaffolding(9, 0, 3, 6);
                PlaceScaffolding(9, 0, 2, 6);
                PlaceScaffolding(10, 0, 4, 8);

                // second refill

                PlaceScaffolding(19, 5, 3, 6);
                PlaceScaffolding(19, 5, 2, 6);
                PlaceScaffolding(19, 5, 1, 6);
                PlaceScaffolding(19, 5, 0, 6);
                PlaceScaffolding(19, 4, 2, 6);
                PlaceScaffolding(19, 4, 1, 6);
                PlaceScaffolding(19, 4, 0, 6);
                PlaceScaffolding(19, 3, 0, 6);
                PlaceScaffolding(20, 5, 1, 8);

                // two corner scaffolds next to refill to teach player what 
                // they are

                placeDirection = 2;
                PlaceScaffolding(20, 5, 2, 7);
                placeDirection++;
                PlaceScaffolding(20, 5, 0, 7);
                placeDirection = 0;

                // other random bits of terrain

                PlaceScaffolding(5, 1, 0, 6);
                PlaceScaffolding(5, 0, 1, 6);
                PlaceScaffolding(5, 0, 0, 6);
                PlaceScaffolding(6, 0, 0, 6);
                PlaceScaffolding(15, 5, 5, 6);
                PlaceScaffolding(15, 4, 5, 6);
                PlaceScaffolding(16, 5, 5, 6);
                PlaceScaffolding(23, 0, 4, 6);
                PlaceScaffolding(23, 0, 3, 6);

                break;
            case 2:

                // ground spikes
                PlaceScaffolding(0, 4, 4, 6);
                PlaceScaffolding(1, 4, 4, 6);
                PlaceScaffolding(2, 4, 4, 6);
                PlaceScaffolding(3, 4, 4, 6);
                PlaceScaffolding(4, 4, 4, 6);
                PlaceScaffolding(0, 4, 3, 6);
                PlaceScaffolding(0, 3, 4, 6);

                PlaceScaffolding(0, 1, 3, 6);
                PlaceScaffolding(1, 1, 3, 6);
                PlaceScaffolding(2, 1, 3, 6);
                PlaceScaffolding(3, 1, 3, 6);
                PlaceScaffolding(0, 1, 2, 6);

                PlaceScaffolding(0, 3, 1, 6);
                PlaceScaffolding(1, 3, 1, 6);
                PlaceScaffolding(2, 3, 1, 6);
                PlaceScaffolding(0, 3, 2, 6);

                // first refill

                PlaceScaffolding(8, 5, 2, 6);
                PlaceScaffolding(8, 5, 1, 6);
                PlaceScaffolding(8, 5, 0, 6);
                PlaceScaffolding(9, 5, 4, 6);
                PlaceScaffolding(9, 5, 3, 6);
                PlaceScaffolding(9, 5, 2, 6);
                PlaceScaffolding(9, 5, 1, 6);
                PlaceScaffolding(9, 5, 0, 6);
                PlaceScaffolding(9, 4, 3, 6);
                PlaceScaffolding(9, 4, 2, 6);
                PlaceScaffolding(9, 4, 1, 6);
                PlaceScaffolding(9, 4, 0, 6);
                PlaceScaffolding(10, 5, 2, 8);

                // elevator + key to teach player how to use them

                PlaceScaffolding(10, 5, 3, 10);
                PlaceScaffolding(10, 5, 1, 9);
                PlaceScaffolding(10, 5, 0, 1);

                // terrain between refills 1 and 2

                PlaceScaffolding(14, 1, 2, 6);
                PlaceScaffolding(14, 1, 1, 6);
                PlaceScaffolding(14, 1, 0, 6);
                PlaceScaffolding(14, 2, 2, 6);

                PlaceScaffolding(15, 2, 3, 6);
                PlaceScaffolding(15, 2, 2, 6);

                PlaceScaffolding(16, 3, 5, 6);
                PlaceScaffolding(16, 3, 4, 6);
                PlaceScaffolding(16, 3, 3, 6);

                // second refill

                PlaceScaffolding(19, 1, 5, 6);
                PlaceScaffolding(19, 0, 5, 6);
                PlaceScaffolding(19, 0, 4, 6);
                PlaceScaffolding(20, 0, 5, 8);

                break;
        }
    }

    /// <summary>
    /// change scaffold rarities upon refill depending on the level.
    /// </summary>
    private void ChangeRarities()
    {
        levelSection++;

        switch (level)
        {
            case 0:
                switch (levelSection)
                {
                    case 1:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 0),
                            new Vector2(1, 1),
                            new Vector2(2, 0),
                            new Vector2(3, 0),
                            new Vector2(4, 0),
                            new Vector2(5, 0),
                            new Vector2(6, 0),
                            new Vector2(7, 0),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 0)

                        };

                        break;

                    case 2:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 8),
                            new Vector2(1, 10),
                            new Vector2(2, 2),
                            new Vector2(3, 0),
                            new Vector2(4, 2),
                            new Vector2(5, 0),
                            new Vector2(6, 0),
                            new Vector2(7, 0),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 0)

                        };

                        break;
                }
                break;
            case 1:
                switch (levelSection)
                {
                    case 1:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 8),
                            new Vector2(1, 10),
                            new Vector2(2, 2),
                            new Vector2(3, 0),
                            new Vector2(4, 2),
                            new Vector2(5, 0),
                            new Vector2(6, 0),
                            new Vector2(7, 0),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 0)

                        };

                        break;

                    case 2:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 3),
                            new Vector2(1, 8),
                            new Vector2(2, 1),
                            new Vector2(3, 7),
                            new Vector2(4, 5),
                            new Vector2(5, 0),
                            new Vector2(6, 0),
                            new Vector2(7, 0),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 0)

                        };

                        break;

                    case 3:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 1),
                            new Vector2(1, 8),
                            new Vector2(2, 1),
                            new Vector2(3, 0),
                            new Vector2(4, 5),
                            new Vector2(5, 4),
                            new Vector2(6, 0),
                            new Vector2(7, 5),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 0)

                        };

                        break;
                }
                break;
            case 2:
                switch (levelSection)
                {
                    case 1:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 0),
                            new Vector2(1, 8),
                            new Vector2(2, 1),
                            new Vector2(3, 2),
                            new Vector2(4, 4),
                            new Vector2(5, 2),
                            new Vector2(6, 0),
                            new Vector2(7, 2),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 0)

                        };

                        break;

                    case 2:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 6),
                            new Vector2(1, 8),
                            new Vector2(2, 2),
                            new Vector2(3, 0),
                            new Vector2(4, 4),
                            new Vector2(5, 0),
                            new Vector2(6, 0),
                            new Vector2(7, 0),
                            new Vector2(8, 0),
                            new Vector2(9, 4),
                            new Vector2(10, 3)

                        };

                        // open elevator tutorial panel since it's the first
                        // time seeing them
                        encounteredElevator = true;

                        break;

                    case 3:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 8),
                            new Vector2(1, 10),
                            new Vector2(2, 4),
                            new Vector2(3, 0),
                            new Vector2(4, 10),
                            new Vector2(5, 4),
                            new Vector2(6, 0),
                            new Vector2(7, 4),
                            new Vector2(8, 0),
                            new Vector2(9, 14),
                            new Vector2(10, 10)

                        };

                        break;
                }

                break;
            case 3:
                switch (levelSection)
                {
                    case 1:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 0),
                            new Vector2(1, 0),
                            new Vector2(2, 0),
                            new Vector2(3, 0),
                            new Vector2(4, 0),
                            new Vector2(5, 0),
                            new Vector2(6, 0),
                            new Vector2(7, 0),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 0)

                        };

                        break;

                    case 2:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 0),
                            new Vector2(1, 0),
                            new Vector2(2, 0),
                            new Vector2(3, 0),
                            new Vector2(4, 0),
                            new Vector2(5, 0),
                            new Vector2(6, 0),
                            new Vector2(7, 0),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 0)

                        };

                        break;

                    case 3:

                        scaffoldRarities = new Vector2[]
                        {
                            new Vector2(0, 0),
                            new Vector2(1, 0),
                            new Vector2(2, 0),
                            new Vector2(3, 0),
                            new Vector2(4, 0),
                            new Vector2(5, 0),
                            new Vector2(6, 0),
                            new Vector2(7, 0),
                            new Vector2(8, 0),
                            new Vector2(9, 0),
                            new Vector2(10, 5)

                        };

                        break;
                }
                break;
        }
    }

    #endregion

}

public class ScaffoldMap
{
    
    #region variables

    private bool[][][] solidGroundMap;
    private GameObject[][][] scaffoldingPlacements;
    private List<Vector3[]> movementBlockades;

    #endregion

    #region constructor

    public ScaffoldMap(MapController mapController)
    {
        // set height 
        solidGroundMap = new bool[mapController.MapHeight][][];
        scaffoldingPlacements = new GameObject[mapController.MapHeight][][];
        for (int i = 0;  i < solidGroundMap.Length; i++)
        {
            // set rows 
            solidGroundMap[i] = new bool[mapController.Rows.Count][];
            scaffoldingPlacements[i] = new GameObject[mapController.Rows.Count][];
            for (int j = 0; j < solidGroundMap[i].Length; j++)
            {
                // set cols
                solidGroundMap[i][j] = new bool[mapController.Spaces[0].Count];
                scaffoldingPlacements[i][j] = new GameObject[mapController.Spaces[0].Count];
                for (int k = 0; k <  solidGroundMap[i][j].Length; k++)
                {
                    // make floor solid and everything else fall-through
                    solidGroundMap[i][j][k] = i == 0; 
                    // make each scaffolding gameobject null
                    scaffoldingPlacements[i][j][k] = null;
                }
            }
        }

        movementBlockades = new List<Vector3[]>();
    }

    #endregion

    #region miscellaneous functions

    /// <summary>
    /// sets given position to solid ground
    /// </summary>
    /// <param name="position">position in grid</param>
    public void ToggleSolidGround(Vector3 position)
    {
        SolidGroundMap[(int)position.x][(int)position.y][(int)position.z] = 
            !SolidGroundMap[(int)position.x][(int)position.y][(int)position.z];
    }

    /// <summary>
    /// adds a blockade to prevent movement between the given space and the
    /// second space determined by the offset.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="secondPosOffset"></param>
    public void AddBlockade(Vector3 position, Vector3 secondPosOffset)
    {
        movementBlockades.Add(new Vector3[] { position, position + 
            secondPosOffset });
    }

    #endregion

    #region getters and setters

    /// <summary>
    /// map of whether or not each space's ground is solid. first value = 
    /// height, second value = row, third value = column
    /// </summary>
    public bool[][][] SolidGroundMap { get => solidGroundMap; set => 
            solidGroundMap = value; }
    public GameObject[][][] ScaffoldingPlacements { get => 
            scaffoldingPlacements; set => scaffoldingPlacements = value; }
    public List<Vector3[]> MovementBlockades { get => movementBlockades; set => movementBlockades = value; }

    #endregion
}