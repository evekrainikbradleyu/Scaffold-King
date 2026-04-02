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

    // privates
    private MapController mapController;
    private int placeDirection;

    // serialized privates
    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private Vector2[] scaffoldRarities;
    [SerializeField] private GameObject[] ghostScaffolds;
    [SerializeField] private GameObject scaffoldFiller;

    #endregion

    #region start

    /// <summary>
    /// makes the map and sets up the current and nexxt scaffold. also sets 
    /// variables.
    /// </summary>
    private void Start()
    {
        mapController = mapShellController.shellController;

        map = new ScaffoldMap(mapController);

        currentScaffolding = 1;
        nextScaffolding = GetNextScaffolding();

        placing = false;

        placeDirection = 0;
    }

    #endregion

    #region miscellaneous functions

    /// <summary>
    /// places a piece of scaffolding.
    /// </summary>
    /// <param name="position">the position on the map to place the scaffolding
    /// ; x = height, y = row, z = col</param>
    public void PlaceScaffolding(Vector3 position)
    {

        // stop if theres already scaffolding there
        if (CheckForScaffolding(position)) { return; }

        // instantiate scaffolding
        SetScaffoldPlacement(position, Instantiate(scaffoldTypes[
            currentScaffolding]));
        // set position of scaffolding
        GetScaffoldPlacement(position).transform.position = SetPlacePosition(
            position);
        // set rotation based on scroll
        GetScaffoldPlacement(position).transform.eulerAngles = new Vector3(0, 
            placeDirection * 90, 0);

        // checks to see if scaffold type is 1x1, update when new types added
        if (ScaffoldIs1x1())
        {
            if (position.x < mapController.MapHeight - 1)
            {
                map.ToggleSolidGround(new Vector3
                    (
                        position.x + 1,
                        position.y,
                        position.z
                    ));
            }
        } 
        else if (ScaffoldIsTall())
        {
            // make sure top isn't null
            SetScaffoldPlacement(position, Instantiate(scaffoldFiller), offset: 
                Vector3.right); // Vector3.right == (1,0,0)

            if (position.x < mapController.MapHeight - 2)
            {
                map.ToggleSolidGround(new Vector3
                    (
                        position.x + 2,
                        position.y,
                        position.z
                    ));
            }
        }

        // remove one scaffolding, get new current + next
        scaffoldingRemaining--;
        CycleScaffolding();

        // toggles place mode to false for next scaffolding
        placing = false;
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
        if (CheckForScaffolding(position)) { return null; }

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
    /// <returns>whether or not theres already scaffolding in the selected spot
    /// </returns>
    private bool CheckForScaffolding(Vector3 position)
    {
        return ScaffoldIs1x1() ? GetScaffoldPlacement(position) != null :
            ScaffoldIsTall() ? GetScaffoldPlacement(position) != null && 
            GetScaffoldPlacement(position, offset: Vector3.right) != null : 
            false;
    }

    /// <summary>
    /// checks if current scaffold is basic 1x1x1
    /// </summary>
    /// <returns>true if the scaffold is 1x1x1</returns>
    private bool ScaffoldIs1x1()
    {
        return Array.Exists<int>(new int[] { 0, 1 }, i => i ==
            currentScaffolding);
    }

    /// <summary>
    /// checks if current scaffold is tall (2 spaces tall)
    /// </summary>
    /// <returns>true if scaffold is tall</returns>
    private bool ScaffoldIsTall()
    {
        return Array.Exists<int>(new int[] { 2 }, i => i == currentScaffolding)
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

        return scaffoldRanges[UnityEngine.Random.Range(0, scaffoldRanges.Count)
            ];
            
    }

    /// <summary>
    /// cycles through scaffolding after placing
    /// </summary>
    private void CycleScaffolding()
    {
        currentScaffolding = nextScaffolding;
        nextScaffolding = GetNextScaffolding();
    }

    private ref GameObject GetScaffoldPlacement(Vector3 position, Vector3 
        offset = default) // default is ok here since Vector3s default to 0,0,0
    {
        return ref map.ScaffoldingPlacements[(int)position.x + (int)offset.x][(
            int)position.y + (int)offset.y][(int)position.z + (int)offset.z];
    }

    private void SetScaffoldPlacement(Vector3 position, GameObject newObject, 
        Vector3 offset = default) // same deal as GetScaffoldPlacement
    {
        map.ScaffoldingPlacements[(int)position.x + (int)offset.x][(int)
            position.y + (int)offset.y][(int)position.z + (int)offset.z] = 
            newObject;
    }

    public void UpdatePlaceDirection(float scrollInput)
    {
        placeDirection += scrollInput > 0 ? 1 : -1;
        placeDirection = placeDirection < 0 ? 3 : placeDirection > 3 ? 0 : 
            placeDirection;
    }

    #endregion

}

public class ScaffoldMap
{

    #region variables

    private bool[][][] solidGroundMap;
    private GameObject[][][] scaffoldingPlacements;

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

    #endregion
}