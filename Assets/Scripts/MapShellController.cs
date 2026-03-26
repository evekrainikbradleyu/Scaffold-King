/*****************************************************************************
// File Name : MapShellController.cs
// Author : Eve "Map Shell Queen" Krainik
// Creation Date : March 21, 2026
//
// Brief Description : Added to the MapService's MapShell to provide access to 
// other scripts. MapController class included.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MapShellController : MonoBehaviour
{

    #region variables

    // publics
    public MapController shellController;

    // privates

    // *crickets*

    // serialized privates
    [SerializeField] private int mapHeight;
    [SerializeField] private float yLayerDelta;
    [SerializeField] private float mapStartHeight;

    #endregion

    #region awake 

    /// <summary>
    /// assigns shellController
    /// </summary>
    private void Awake() // using awake instead of start to get ahead of other
    {                    // scripts using the shellController
        shellController = new MapController(gameObject, new Vector3((float) 
            mapHeight, yLayerDelta, mapStartHeight));
    }

    #endregion

}

[Serializable] // this DIDN'T WORK FOR SOME REASON AND I'M SO MAD but not mad 
               // enough to try and fix it
public class MapController
{

    #region variables

    // all private (use getters + setters)
    private List<GameObject> rows;
    private List<List<GameObject>> spaces;
    private int mapHeight;
    private float yLayerDelta;
    private float mapStartHeight;
    private List<float> mapLayerHeights;

    #endregion

    #region constructor

    /// <summary>
    /// mapcontroller constructor
    /// </summary>
    /// <param name="shell">shell gameobject</param>
    /// <param name="heightData">vector 3 consisting of the mapheight (x), 
    /// difference between ylayer heights (y), and global height that the map 
    /// starts at (z)</param>
    public MapController(GameObject shell, Vector3 heightData)
    {
        Rows = new List<GameObject>();
        Spaces = new List<List<GameObject>>();

        // adds every space for scaffolding to go / player to move
        foreach (Transform child in shell.transform.Find("Floor"))
        {
            Rows.Add(child.gameObject);

            List<GameObject> spacesList = new List<GameObject>();
            foreach (Transform smallerChild in child)
            {
                spacesList.Add(smallerChild.gameObject);
            }

            Spaces.Add(spacesList);
        }

        mapHeight = (int)heightData.x;
        yLayerDelta = heightData.y;
        mapStartHeight = heightData.z;

        MapLayerHeights = new List<float>();

        // sets up all map height values for easy access
        for (int i = 0; i < MapHeight; i++)
        {
            MapLayerHeights.Add(mapStartHeight + (i * YLayerDelta));
        }
    }

    #endregion

    #region functions

    /// <summary>
    /// gets the vector position (grid, not worldspace) of a space. does not 
    /// include height.
    /// </summary>
    /// <param name="space">the space's GameObject</param>
    /// <returns>vector position of space</returns>
    public Vector2 GetVectorFromSpace(GameObject space)
    {
        for (int x = 0; x < Spaces.Count; x++)
        {
            for (int y = 0; y < Spaces[x].Count; y++)
            {
                if     // checks against position values rather than
                       // gameobjects themselves because multiple positions are
                       // in the same column
                    (Spaces[x][y].transform.position.x == space.transform.
                    position.x && Spaces[x][y].transform.position.z == space.
                    transform.position.z)
                {
                    return new Vector2(x, y);
                }
            }
        }

        // returns 0,0 if nothing found.
        Debug.Log("vector not found for space. returning 0,0.");
        return Vector2.zero;
    }

    /// <summary>
    /// gets the space gameobject (on the floor) from the given vector. used 
    /// for positioning.
    /// </summary>
    /// <param name="spacePosition">vector position of space</param>
    /// <returns>space gameobject (use x and z values for positioning)
    /// </returns>
    public GameObject GetSpaceFromVector(Vector2 spacePosition)
    {
        int x = (int)spacePosition.x;
        int y = (int)spacePosition.y;

        if (x < 0 || x >= Spaces.Count) return null;
        if (y < 0 || y >= Spaces[x].Count) return null;

        return Spaces[x][y];
    }

    /// <summary>
    /// gets the position on the board from an object's transform
    /// </summary>
    /// <param name="transform">object's transform</param>
    /// <returns>position of object on board (height not included)</returns>
    public Vector2 GetPosFromTransform(Transform transform)
    {
        Vector2 pos = Vector2.zero;

        List<float> rowPositions = new List<float>();
        List<float> columnPositions = new List<float>();

        // get row and column position in the worldspace
        foreach (var row in rows)
        {
            rowPositions.Add(row.transform.position.z);
        }
        foreach (var col in spaces[0])
        {
            columnPositions.Add(col.transform.position.x);
        }

        // check what position the given transform is in
        for (int i = 0; i < rowPositions.Count; i++)
        {
            pos.x = rowPositions[i] == transform.position.z ? rowPositions[i] :
                0;
        }
        for (int i = 0; i < columnPositions.Count; i++)
        {
            pos.y = columnPositions[i] == transform.position.x ? 
                columnPositions[i] : 0;
        }

        return pos.x == 0 || pos.y == 0 ? Vector2.zero : pos;
    }

    /// <summary>
    /// converts worldspace y value to map height
    /// </summary>
    /// <param name="input">worldspace y value</param>
    /// <returns>map height</returns>
    public int GetHeightFromFloat(float input)
    {
        // returns 0 if the height is the floor (floor space)
        if (input == 0) { return 0; }
        // formula
        float operatorOutput = (input - mapStartHeight) / yLayerDelta + 1;
        // convert to int
        int output = (int)operatorOutput;

        return output;
        
    }

    #endregion

    #region getters + setters

    public List<GameObject> Rows { get => rows; set => rows = value; }
    public List<List<GameObject>> Spaces { get => spaces; set => spaces = 
            value; }
    /// <summary>
    /// preferably use MapLayerHeights.Count
    /// </summary>
    public int MapHeight { get => mapHeight; /*set => mapHeight = value;*/ }
    /// <summary>
    /// preferably use (MapLayerHeights[1]-MapLayerHeights[0])
    /// </summary>
    public float YLayerDelta { get => yLayerDelta; /*set => yLayerDelta = 
            * value;*/ }
    /// <summary>
    /// preferably use MapLayerHeights[0]
    /// </summary>
    public float MapStartHeight { get => mapStartHeight; /*set => 
            mapStartHeight = value;*/ }
    public List<float> MapLayerHeights { get => mapLayerHeights; set => 
            mapLayerHeights = value; } 
    

    #endregion

}

