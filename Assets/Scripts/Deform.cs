using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deform : MonoBehaviour
{
    /* Contains information on how large the height deformation will be
    // Stores the radius of terrain that will be affected
    // Use Manhattan distance to calculate amount of falloff on the height to get craters
    // Update Elevation layer and signal that the Terrain needs to be redrawn
    // A bit of pre-emptive optimizing since larger meshes may take a long time to recalculate (TEST TEST TEST!!)
    */

    LayerTerrain terrain;
    int Radius; //This is how many tiles (in a diamond) the deform will affect
    float HeightChange; //Change this many units from the source square and then use HeightChange divided by falloff to get other tiles' heights
    float HeightFalloff; //Divide the height by the number times its absolute manhattan distance from the source tile

    public void SetDeformSettings(int radius, float heightChange,  float heightFalloff)
    {
        Radius = radius;
        HeightChange = heightChange;
        HeightFalloff = heightFalloff;
    }

    public void DeformTerrain(Vector2 coords, string layer)
    {
        //Fetch the tile from the coords from the map
        //Apply HeightChange to the layer specified by multiplying HeightFalloff by the ManhattanDistance
        //Step through each tile adding the resulting HeightChange to each tile within Radius
    }

}
