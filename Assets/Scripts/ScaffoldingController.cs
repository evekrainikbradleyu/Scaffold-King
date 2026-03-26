using System;
using UnityEngine;

public class ScaffoldingController : MonoBehaviour
{
    public ScaffoldMap map;

    private MapController mapController;

    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private GameObject[] scaffoldTypes;

    private void Start()
    {
        mapController = mapShellController.shellController;

        map = new ScaffoldMap(mapController);

        PlaceScaffolding(0, new Vector3(0, 5, 5));
        PlaceScaffolding(0, new Vector3(1, 5, 5));
        PlaceScaffolding(0, new Vector3(2, 5, 5));
    }
    
    /// <summary>
    /// places a piece of scaffolding.
    /// </summary>
    /// <param name="scaffoldType">the type of scaffolding to place; available
    /// types currently include: 0 - normal, 1- ladder</param>
    /// <param name="position">the position on the map to place the scaffolding
    /// ; x = height, y = row, z = col</param>
    public void PlaceScaffolding(int scaffoldType, Vector3 position)
    {
        map.ScaffoldingPlacements[(int)position.x][(int)position.y][(int)
            position.z] = Instantiate(scaffoldTypes[scaffoldType]);
        map.ScaffoldingPlacements[(int)position.x][(int)position.y][(int)
            position.z].transform.position = new Vector3
            (
                mapController.GetSpaceFromVector(new Vector2(position.y,
                    position.z)).transform.position.x,
                mapController.MapLayerHeights[(int)position.x],
                mapController.GetSpaceFromVector(new Vector2(position.y,
                    position.z)).transform.position.z
            );

        Destroy(mapController.GetSpaceFromVector(new Vector2(position.y, 
            position.z)).GetComponent<BoxCollider>());

        // checks to see if scaffold type is 1x1, update when new types added
        if (Array.Exists<int>(new int[] { 0, 1 }, i => i == scaffoldType))
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
    }
}

public class ScaffoldMap
{
    private bool[][][] solidGroundMap;
    private GameObject[][][] scaffoldingPlacements;

    public ScaffoldMap(MapController mapController)
    {
        solidGroundMap = new bool[mapController.MapHeight][][];
        scaffoldingPlacements = new GameObject[mapController.MapHeight][][];
        for (int i = 0;  i < solidGroundMap.Length; i++)
        {
            solidGroundMap[i] = new bool[mapController.Rows.Count][];
            scaffoldingPlacements[i] = new GameObject[mapController.Rows.Count][];
            for (int j = 0; j < solidGroundMap[i].Length; j++)
            {
                solidGroundMap[i][j] = new bool[mapController.Spaces[0].Count];
                scaffoldingPlacements[i][j] = new GameObject[mapController.Spaces[0].Count];
                for (int k = 0; k <  solidGroundMap[i][j].Length; k++)
                {
                    solidGroundMap[i][j][k] = i == 0; // makes floor solid and everything else fall-through
                    scaffoldingPlacements[i][j][k] = null;
                }
            }
        }
    }

    public void ToggleSolidGround(Vector3 position)
    {
        SolidGroundMap[(int)position.x][(int)position.y][(int)position.z] = 
            !SolidGroundMap[(int)position.x][(int)position.y][(int)position.z];
    }

    /// <summary>
    /// map of whether or not each space's ground is solid. first value = 
    /// height, second value = row, third value = column
    /// </summary>
    public bool[][][] SolidGroundMap { get => solidGroundMap; set => 
            solidGroundMap = value; }
    public GameObject[][][] ScaffoldingPlacements { get => scaffoldingPlacements; set => scaffoldingPlacements = value; }
}