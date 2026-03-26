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

    // privates
    private MapController mapController;

    // serialized privates
    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private GameObject[] scaffoldTypes;
    [SerializeField] private Vector2[] scaffoldRarities;

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
    }

    #endregion

    #region miscellaneous functions

    /// <summary>
    /// places a piece of scaffolding.
    /// </summary>
    /// <param name="position">the position on the map to place the scaffolding
    /// ; x = height, y = row, z = col</param>
    public void PlaceScaffolding( Vector3 position)
    {
        // stop if player isn't in placing mode
        if (!placing) { return; }

        // stop if theres already scaffolding there
        if (map.ScaffoldingPlacements[(int)position.x][(int)position.y][(int)
            position.z] != null) { return; }

        // instantiate scaffolding
        map.ScaffoldingPlacements[(int)position.x][(int)position.y][(int)
            position.z] = Instantiate(scaffoldTypes[currentScaffolding]);
        // set position of scaffolding
        map.ScaffoldingPlacements[(int)position.x][(int)position.y][(int)
            position.z].transform.position = new Vector3
            (
                mapController.GetSpaceFromVector(new Vector2(position.y,
                    position.z)).transform.position.x, // gets space x position
                mapController.MapLayerHeights[(int)position.x], // gets height
                mapController.GetSpaceFromVector(new Vector2(position.y,
                    position.z)).transform.position.z // gets space z position
            );

        // checks to see if scaffold type is 1x1, update when new types added
        if (Array.Exists<int>(new int[] { 0, 1 }, i => i == currentScaffolding)
            )
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

        // remove one scaffolding, get new current + next
        scaffoldingRemaining--;
        CycleScaffolding();

        // toggles place mode to false for next scaffolding
        placing = false;
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