using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcGenTiles;
using UnityEngine.InputSystem;

public class Deform : MonoBehaviour
{
    /* Contains information on how large the height deformation will be
    // Stores the radius of terrain that will be affected
    // Use Manhattan distance to calculate amount of falloff on the height to get craters
    // Update Elevation layer and signal that the Terrain needs to be redrawn
    // A bit of pre-emptive optimizing since larger meshes may take a long time to recalculate (TEST TEST TEST!!)
    */

    [SerializeField]
    LayerTerrain terrain;

    Map map;

    [SerializeField]
    int Radius; //This is how many tiles (in a diamond) the deform will affect
    [SerializeField]
    float HeightChange; //Change this many units from the source square and then use HeightChange divided by falloff to get other tiles' heights
    [SerializeField]
    float HeightFalloff; //Divide the height by the number times its absolute manhattan distance from the source tile

    private void Start()
    {
        
    }

    void OnFire()
    { //Fire action was pressed, find the transform of this since it's on the boat and pass in the serialized values to the deform settings
        map = terrain.finalMap;
        DeformTerrain(new Vector2(transform.position.z, transform.position.x), LayersEnum.Elevation);
    }

    public void SetDeformSettings(int radius, float heightChange,  float heightFalloff)
    { //Used for later when we need to read these values off of the projectile instead of setting them manually
        Radius = radius;
        HeightChange = heightChange;
        HeightFalloff = heightFalloff;
    }

    public void DeformTerrain(Vector2 coords, string layer)
    {
        //Fetch the tile from the coords from the map
        //Apply HeightChange to the layer specified by multiplying HeightFalloff by the ManhattanDistance
        //Step through each tile adding the resulting HeightChange to each tile within Radius
        int x = Mathf.RoundToInt(coords.x);
        int y = Mathf.RoundToInt(coords.y);

        //Run through a square of radius checking if the manhattan distance is greater than the radius
        for (int i = Radius * -1; i <= Radius; i++)
        {
            for (int j = Radius * -1; j <= Radius; j++)
            {
                int distance = Helpers.ManhattanDistance(x + i, x, y + j, y);
                if (distance <= Radius && map.IsValidTilePosition(x + i, y + j))
                { //This tile is within the diamond grid we want to deform and on the map
                    Tile tile = map.GetTile(x + i, y + j); //Fetch the tile from the map array
                    float falloff = HeightFalloff * distance;
                    float change = HeightChange / falloff; //Height change is strongest when falloff is 1 and grows the further the distance is
                    tile.ValuesHere[layer] += change; //Adjust the height of the tile by adding the change. Negative numbers decrease height, positive increases
                }
            }
        }
        //Now fetch float[,] array from map to update LayerTerrain
        float[,] heights = map.FetchFloatValues(layer);
        //Now send this to LayerTerrain
        terrain.CreateTerrainFromHeightmap(heights);
    }

}
