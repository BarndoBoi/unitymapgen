using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcGenTiles;
using UnityEngine.InputSystem;


public class Deform : MonoBehaviour
{
    /* Contains information on the deformation for the terrain
    // Stores the radius of terrain that will be affected
    // Use circular distance to make the crater the size of the Radius times 2
    // Update Elevation layer and signal that the Terrain needs to be redrawn
    // Currently has to redraw the entire Terrain mesh in order to properly deform the terrain
    // Change should be negative for craters, and positive for hills
    //
    // Future TODO: Create an object that more precisely controls the curves and area affected e.g. diamond/square patterns, sharper falloffs on craters,
    //   and a way to mark the terrain as dirty in case we need to apply multiple deforms before updating the Terrain mesh
    */

    [SerializeField]
    LayerTerrain terrain; //Does not support using TerrainFromTilemap, so don't try it Lexi

    Map map; //Fetched out of the terrain, but it would be wise to have a delegate this can listen for when the LayerTerrain has finished making the map (or move assignment to DeformTerrain)

    [SerializeField]
    int Radius; //This is how many tiles (in a circle) the deform will affect 
    [SerializeField]
    float Change; //Change the layer's float value by this amount (use negative numbers to subtract from the layer, positive to add)

    [SerializeField]
    GameObject nm_builder_object;

    // for some reason, this is throwing a null reference when trying to access it in the Deform func
    LocalNavMeshBuilder navmesh;

    




    private void Start()
    {   
        map = terrain.finalMap; //Need to grab a reference to the finalMap before trying to deform. This needs to be moved into DeformTerrain with a null check        
        nm_builder_object = GameObject.Find("navmesh_builder");
    }   

    public void SetDeformSettings(int radius, float change)
    { //Used for later when we need to read these values off of the projectile instead of setting them manually
        //Future TODO: Take in an object that defines the Deform's shape and curves (if any)
        Radius = radius;
        Change = change;
    }

    public void DeformTerrain(Vector2 coords, string layer)
    {
        map = terrain.finalMap; // grabbing new updated map

        //Store the impact coordinates so we know how far it is
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
                float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY); //May want to simplify this to avoid the expensive square root operation. Profile it later

                // Skip tiles outside the circular radius
                if (distance > Radius || !map.IsValidTilePosition(x, y))
                {
                    continue;
                }

                // This tile is within the circle and on the map
                Tile tile = map.GetTile(x, y); // Fetch the tile from the map array

                // Calculate falloff using smoothstep interpolation
                //Falloff is highest at the center and then reduces towards the edges of the circle
                float falloff = Mathf.SmoothStep(1f, 0f, distance / Radius); // Invert the order to apply change strongest at the center

                // Adjust the layer's value of the tile by adding the change.
                // Negative numbers decrease change, positive increases
                tile.ValuesHere[layer] += Change * falloff;

                //Debug.Log($"Start value: {start} final value: {tile.ValuesHere[layer]} change: {Change * falloff} coords: {x},{y} falloff: {falloff}"); //Not needed right now
            }
        }

        //This stays for now, but if slices of the terrain can't be grabbed this should instead use map.FetchFloatValues
        //If there is a chance that multiple deforms can happen this needs to instead mark the LayerTerrain as dirty and wait until all operations are complete before updating the Terrain
        float[,] heights = map.FetchFloatValuesSlice(layer, 0, map.Width, 0, map.Height);
        terrain.UpdateTerrainHeightmap(0,0,heights);
        terrain.ApplyTextures(sourceX-Radius,sourceY-Radius, sourceX+Radius, sourceY + Radius, true);

        // this is fucking terrible but works.... figure out why???
        // TODO: only run this if the deform causes new water layer.
        // Don't need to update navmesh if the side of a mountain is hit.
        nm_builder_object.GetComponent<LocalNavMeshBuilder>().UpdateNavMesh(false);
        //navmesh.UpdateNavMesh(false);
        // This errors Null reference for some reason, but shouldn't... 
        //navmesh.UpdateNavMesh(false);

    }
}
