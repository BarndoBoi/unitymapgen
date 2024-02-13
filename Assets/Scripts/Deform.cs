using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcGenTiles;
using UnityEngine.InputSystem;

public class Deform : MonoBehaviour
{
    /* Contains information on how large the Y deformation will be
    // Stores the radius of terrain that will be affected
    // Use Manhattan distance to calculate amount of falloff on the Y to get craters
    // Update Elevation layer and signal that the Terrain needs to be redrawn
    // A bit of pre-emptive optimizing since larger meshes may take a long time to recalculate (TEST TEST TEST!!)
    */

    [SerializeField]
    LayerTerrain terrain;

    Map map;

    [SerializeField]
    int Radius; //This is how many tiles (in a circle) the deform will affect
    [SerializeField]
    float Change; //Change the layer's float value by this amount (use negative numbers to subtract from the layer

    void OnFire()
    { //Fire action was pressed, find the transform of this since it's on the boat and pass in the serialized values to the deform settings
        map = terrain.finalMap;
        Debug.Log("Deforming terrain");
        DeformTerrain(new Vector2(transform.position.z, transform.position.x), LayersEnum.Elevation);
    }

    public void SetDeformSettings(int radius, float change)
    { //Used for later when we need to read these values off of the projectile instead of setting them manually
        Radius = radius;
        Change = change;
    }

    public void DeformTerrain(Vector2 coords, string layer)
    {
        int sourceX = Mathf.RoundToInt(coords.x);
        int sourceY = Mathf.RoundToInt(coords.y);

        // Run through a square of radius checking if the coords are inside the circle
        for (int y = sourceY - Radius; y <= sourceY + Radius; y++)
        {
            for (int x = sourceX - Radius; x <= sourceX + Radius; x++)
            {
                // Calculate distance from current tile to center coordinates
                int distanceX = Mathf.Abs(sourceX - x);
                int distanceY = Mathf.Abs(sourceY - y);
                float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

                //Debug.Log($"xIndex is: {xIndex} yIndex is: {yIndex}");

                // Skip tiles outside the circular radius
                if (distance > Radius || !map.IsValidTilePosition(x, y))
                {
                    continue;
                }

                // This tile is within the circle and on the map
                Tile tile = map.GetTile(x, y); // Fetch the tile from the map array

                // Calculate falloff using smoothstep interpolation
                float falloff = Mathf.SmoothStep(1f, 0f, distance / Radius); // Invert the order to apply change strongest at the center
                float start = tile.ValuesHere[layer];


                // Adjust the Y of the tile by adding the change.
                // Negative numbers decrease Y, positive increases
                tile.ValuesHere[layer] += Change * falloff;
                Debug.Log($"Start value: {start} final value: {tile.ValuesHere[layer]} change: {Change * falloff} coords: {x},{y} falloff: {falloff}");
            }
        }

        // 
        //float[,] heights = map.FetchFloatValuesSlice(layer, sourceY - Radius, sourceY + Radius, sourceX - Radius, sourceX + Radius);
        //terrain.UpdateTerrainRegion(sourceX - Radius, sourceY - Radius, heights); //Pass the modified values to the terrain and adjust heights

        //This works for some reason, but trying to update only a small slice of the area doesn't work at all, and switching around the values makes it work even more wonky
        //I'm hella confused yo
        float[,] heights = map.FetchFloatValuesSlice(layer, 0, map.Width, 0, map.Height);
        terrain.UpdateTerrainRegion(0,0,heights);
        //terrain.CreateTerrainFromHeightmap(heights);


    }

}
