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

                // Skip tiles outside the circular radius
                if (distance > Radius || !map.IsValidTilePosition(x, y))
                {
                    continue;
                }

                // This tile is within the circle and on the map
                Tile tile = map.GetTile(x, y); // Fetch the tile from the map array

                // Calculate falloff using smoothstep interpolation
                float falloff = Mathf.SmoothStep(1f, 0f, distance / Radius); // Invert the order to apply change strongest at the center

                // Adjust the Y of the tile by adding the change.
                // Negative numbers decrease Y, positive increases
                tile.ValuesHere[layer] += Change * falloff;
            }
        }

        // Now fetch float[,] array from map to update LayerTerrain
        float[,] heights = map.FetchFloatValuesSlice(layer, sourceY - Radius, sourceY + Radius, sourceX - Radius, sourceX + Radius);
        Debug.Log("Heights has dimensions of " + heights.GetLength(0) + " " + heights.GetLength(1));
        //float[,] heights = map.FetchFloatValues(layer);
        //terrain.CreateTerrainFromHeightmap(heights);

        // Now send this to LayerTerrain
        terrain.UpdateTerrainRegion(sourceX - Radius, sourceY - Radius, heights); //Pass the modified values to the terrain and adjust heights
    }


    /*public void DeformTerrain(Vector2 coords, string layer)
    {
        //Fetch the tile from the coords from the map
        //Apply Change to the layer specified by multiplying HeightFalloff by the ManhattanDistance
        //Step through each tile adding the resulting Change to each tile within Radius
        int sourceX = Mathf.RoundToInt(coords.sourceX);
        int sourceY = Mathf.RoundToInt(coords.sourceY);

        //Run through a square of radius checking if the manhattan distance is greater than the radius
        for (int y = Radius * -1; y <= Radius; y++)
        {
            for (int x = Radius * -1; x <= Radius; x++)
            {
                int distance = Helpers.ManhattanDistance(sourceX + y, sourceX, sourceY + x, sourceY);
                if (distance <= Radius && map.IsValidTilePosition(sourceX + y, sourceY + x))
                { //This tile is within the diamond grid we want to deform and on the map
                    Tile tile = map.GetTile(sourceX + y, sourceY + x); //Fetch the tile from the map array
                    float falloff = HeightFalloff * distance;
                    float change = Change / falloff; //Height change is strongest when falloff is 1 and grows the further the distance is
                    tile.ValuesHere[layer] += change; //Adjust the Y of the tile by adding the change. Negative numbers decrease Y, positive increases
                }
            }
        }
        //Now fetch float[,] array from map to update LayerTerrain
        float[,] heights = map.FetchFloatValues(layer);
        //Now send this to LayerTerrain
        terrain.CreateTerrainFromHeightmap(heights);
    }*/

}
