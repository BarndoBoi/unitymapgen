using System.Collections.Generic;
using System;

namespace ProcGenTiles
{
	public class Pathfinding
	{
		//HashSet<(int x, int y)> visited = new HashSet<(int x, int y)>();
		//Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
		readonly Map Map;
		HashSet<Tile> visited = new HashSet<Tile>();
		Queue<Tile> queue = new Queue<Tile>();
		public Dictionary<int, int> regionSizes = new Dictionary<int, int>(); //Holds the region index and the number of tiles marked with it, for size checking

		public Pathfinding(Map map)
		{
			Map = map;
		}
		
		public void MarkAllRegions()
		{
			//Grab first tile and give it a region index
			//Add it to visited and put its neighbors in the frontier as long as they don't have region codes
			//
			Tile start = Map.GetTile(0, 0);
			int region = 0;
			Queue<Tile> frontier = new Queue<Tile>();
			visited.Clear();
			queue.Enqueue(start);
			AddFourNeighbors(0, 0, frontier);
			while (queue.Count > 0)
			{
				Tile tile = queue.Dequeue();
				tile.ValuesHere.Add(LayersEnum.Region, region);
				while (frontier.Count > 0)
				{
					Tile compare = frontier.Dequeue();
					if (compare.ValuesHere.ContainsKey(LayersEnum.Region))
						continue;
					if (compare.ValuesHere[LayersEnum.Land] == tile.ValuesHere[LayersEnum.Land])
					{
						compare.ValuesHere.Add(LayersEnum.Region, region);
						visited.Add(tile);
					}
					else
					{

					}
				}

			}
			//start.ValuesHere.Add(LayersEnum.Region, region);


		}

		/*bool IsValidAndUnvisited(int x, int y)
		{
		}*/

		public void MarkAllRegions()
		{
			//Get a list of all tiles
			List<(int x, int y)> values = new List<(int x, int y)>();
			for (int x = 0; x < Map.Width; x++)
			{
				for (int y = 0; y < Map.Height; y++)
				{
					values.Add((x, y));
				}
			}
			//Move first tile from list to frontier
			Queue<(int x, int y)> frontier = new Queue<(int x, int y)>();
			visited.Clear();
			int region = 0; //Track which region we're marking
			//Add neighbors to frontier if Land values match
			
			while (values.Count > 0)
			{ //Loop for all tiles
				frontier.Enqueue(values[0]);
				visited.Add(values[0]);
				Tile compare = Map.GetTile(values[0]); //For checking the Land value
				if (!regionSizes.ContainsKey(region))
				{ //If there's no region entry for this, we should add it
					regionSizes.Add(region, 0);
				}

				while (frontier.Count > 0)
				{
					(int x, int y) coords = frontier.Dequeue();
					Tile found = Map.GetTile(coords);
					if (found.ValuesHere["Land"] == compare.ValuesHere["Land"])
					{ //The neighbor matches the start value so assign them the same region
						found.ValuesHere.TryAdd("Region", region);
						regionSizes[region]++; //Increment the number of tiles in the region
						values.Remove(coords); //Delete from values if region is marked
						AddFourNeighbors(coords.x, coords.y, frontier);
					}
				}
				region++; //On to the next one if the frontier ran out
				visited.Clear();
			}
			//Mark until frontier is empty, removing values from the list
			//increment region and pop first item from list until list is empty
		}

		public void BFS((int x, int y) start)
		{

			visited.Add(start);
			queue.Enqueue(start);

			while (queue.Count > 0)
			{
				var current = queue.Dequeue();

				// Add neighboring tiles to the queue if not visited for 4 dir pathfinding: diamonds
				AddFourNeighbors(current.x, current.y, queue);

				//Thinking about running a Func<> through the params to determine what to do with the found tiles

			}
		}

		private void AddFourNeighbors(int x, int y, Queue<Tile> q)
		{
			AddNeighborToQueue(x - 1, y, q);
			AddNeighborToQueue(x + 1, y, q);
			AddNeighborToQueue(x, y - 1, q);
			AddNeighborToQueue(x, y + 1, q);
		}
		
		private void AddEightNeighbors(int x, int y, Queue<Tile> q)
		{ //Stubbed just in case
			AddFourNeighbors(x, y, q);
			AddNeighborToQueue(x - 1, y - 1, q);
			AddNeighborToQueue(x + 1, y - 1, q);
			AddNeighborToQueue(x - 1, y + 1, q);
			AddNeighborToQueue(x + 1, y + 1, q);
		}

		private void AddNeighborToQueue(int x, int y, Queue<Tile> q)
		{
            Tile tile = Map.GetTile(x, y);
            if (Map.IsValidTilePosition(x, y) && !visited.Contains(tile))
			{
				q.Enqueue(tile);
			}
		}
	}
}