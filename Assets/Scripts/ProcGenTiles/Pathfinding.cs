using System.Collections.Generic;
using UnityEngine;

namespace ProcGenTiles
{
	public static class Pathfinding
	{
		static Dictionary<int, List<Tile>> regions = new Dictionary<int, List<Tile>>();

		public static Dictionary<int, List<Tile>> GetRegions() { return regions; }

		public static Dictionary<int, List<Tile>> GetIsolatedWaterRegions()
		{
			int largest = 0;
			int largestIndex = 0;
			//Find the largest water region to exclude, along with any marked as land
			foreach (var region in regions)
			{
				if (region.Value.Count > largest)
				{
					largestIndex = region.Key;
					largest = region.Value.Count;
				}
			}

			//Now create a new dictionary only containing the smaller water regions
			Dictionary<int, List<Tile>> isolatedWater = new Dictionary<int, List<Tile>>();

			foreach(var region in regions)
			{ //One more loop to build the dictionary
				if (region.Key != largestIndex && region.Value[0].ValuesHere[LayersEnum.Land] == 0)
				{ //This isn't the largest region, and it is marked as water, so we add it to the new dictionary
					isolatedWater.Add(region.Key, region.Value);
				}
			}
			return isolatedWater; //Dictionary is built! :DD
		}

		public static void MarkRegions(Map map)
		{
			int regionNumber = 0;

			for (int x = 0; x < map.Width; x++)
			{
				for (int y = 0; y < map.Height; y++)
				{
					if (!map.GetTile(x, y).ValuesHere.ContainsKey(LayersEnum.Region))
					{ //No region has been assigned to this tile, so it's time to floodfill
						FloodFill(map, x, y, regionNumber);
						regionNumber++; //Increment the region after the Floodfill has bubbled all the way back up the call stack
					}
				}
			}
		}

        static void FloodFill(Map map, int x, int y, int regionNumber)
        {
            if (!map.IsValidTilePosition(x, y))
                return; //Not a valid tile, so exit function

            Tile tile = map.GetTile(x, y); //Fetch the valid tile from the map for comparing later, unless it's a new region

            if (tile.ValuesHere.ContainsKey(LayersEnum.Region))
                return; //This tile has already been marked, so we skip it by returning

            if (!regions.ContainsKey(regionNumber))
            { //This is the first time we're marking this region, so set up the dictionary
                List<Tile> tiles = new List<Tile>() { tile }; //Make a new list containing the first tile we've grabbed for this region
                regions.Add(regionNumber, tiles); //Add the list with the first tile
                tile.ValuesHere.Add(LayersEnum.Region, regionNumber); //Mark this tile with the region
            }
            else
            { //The region exists, so we must compare land codes to determine if the tile belongs to the region
                Tile compare = regions[regionNumber][0]; //Nab the first tile from the region list to compare our found tile with

                if (FloatExtensions.Approximately(tile.ValuesHere[LayersEnum.Land], compare.ValuesHere[LayersEnum.Land]))
                { //Values match within float imprecision tolerance so this tile is in the region
                    regions[regionNumber].Add(tile);
                    tile.ValuesHere.Add(LayersEnum.Region, regionNumber); //Mark the region
                }
                else
                {
                    return; //Tile does not belong to the region, so exit the function
                }
            }

			//Debug.Log($"Marked tile at {x},{y} with region {regionNumber}");

            // Recursively flood fill neighboring tiles
            FloodFill(map, x + 1, y, regionNumber); //Right
            FloodFill(map, x - 1, y, regionNumber); //Left
            FloodFill(map, x, y + 1, regionNumber); //Up
            FloodFill(map, x, y - 1, regionNumber); //Down
        }
    }
}