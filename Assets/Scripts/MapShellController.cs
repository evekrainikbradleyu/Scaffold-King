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

    public MapController shellController;

    [SerializeField] private int mapHeight;
    [SerializeField] private float yLayerDelta;
    [SerializeField] private float mapStartHeight;

    #endregion

    #region awake 

    /// <summary>
    /// assigns shellController
    /// </summary>
    private void Awake()
    {
        shellController = new MapController(gameObject, new Vector3((float) 
            mapHeight, yLayerDelta, mapStartHeight));
    }

    #endregion

}

[Serializable]
public class MapController
{

    #region variables

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

        for (int i = 0; i < MapHeight; i++)
        {
            MapLayerHeights.Add(mapStartHeight + (i * YLayerDelta));
        }
    }

    #endregion

    #region functions

    public Vector2 GetVectorFromSpace(GameObject space)
    {
        for (int x = 0; x < Spaces.Count; x++)
        {
            for (int y = 0; y < Spaces[x].Count; y++)
            {
                if (Spaces[x][y] == space)
                {
                    return new Vector2(x, y);
                }
            }
        }

        return Vector2.zero;
    }

    /*public Vector2 GetVectorFromSpace(GameObject space)
    {

        return new Vector2
        (
            int.Parse(space.transform.parent.gameObject.name.ToCharArray()[3].ToString()),
            Array.IndexOf((new char[] { 'a', 'b', 'c', 'd', 'e', 'f' }), space.
                name.ToCharArray()[0]) + 1
        );
    }*/

    public GameObject GetSpaceFromVector(Vector2 spacePosition)
    {
        int x = (int)spacePosition.x;
        int y = (int)spacePosition.y;

        if (x < 0 || x >= Spaces.Count) return null;
        if (y < 0 || y >= Spaces[x].Count) return null;

        return Spaces[x][y];
    }

    /*public GameObject GetSpaceFromVector(Vector2 spacePosition)
    {

        foreach (List<GameObject> i in Spaces)
        { 
            foreach (GameObject k in i)
            {
                Debug.Log("Row name: " + k.transform.parent.gameObject.name);
                if (int.Parse(k.transform.parent.gameObject.name.ToCharArray()[3].ToString())  
                    == spacePosition.x && k.name.ToCharArray()[0] == (new char
                    [] { 'a', 'b', 'c', 'd', 'e', 'f' })[(int) spacePosition.y 
                    - 1])
                {
                    return k;
                }
            }
        }
        return null;
    }*/

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

