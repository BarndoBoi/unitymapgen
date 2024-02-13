using Unity.VisualScripting;

namespace ProcGenTiles
{
	public class Map
	{
		public Tile[,] Tiles { get; set; }
		public int Width, Height;

		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			Tiles = new Tile[width, height];
			for (int x = 0; x < width; x++){
				for (int y = 0; y < height; y++){
					Tiles[x,y] = new Tile();
				}
			}
		}
		
		public bool IsValidTilePosition(int x, int y)
		{
			return x >= 0 && x < Tiles.GetLength(0) && y >= 0 && y < Tiles.GetLength(1);
		}
		
		public Tile GetTile((int x, int y) coords)
		{
			if (IsValidTilePosition(coords.x, coords.y))
			{
				return Tiles[coords.x, coords.y];
			}
			return null; //If it isn't a valid tile return null
		}
		
		public Tile GetTile(int x, int y)
		{
			return GetTile((x, y));
		}

		public float[,] FetchFloatValues(string layer)
		{
			float[,] array = new float[Width, Height];
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					Tile tile = GetTile(i, j);
					if (!tile.ValuesHere.ContainsKey(layer))
					{
						throw new System.ArgumentException("No such layer is present on the tile to fetch!"); //This should crash the function and alert the editor.
					}
					else
					{
						array[i, j] = tile.ValuesHere[layer];
					}
				}
			}
			return array;
		}

		public float[,] FetchFloatValuesSlice(string layer, int minY, int maxY, int minX, int maxX)
		{
			float[,] array = new float[maxY - minY, maxX - minX];
			for (int i = 0; i < maxY - minY; i++)
			{
				for (int j = 0; j < maxX - minX; j++)
				{
					Tile tile = GetTile(j + minX, i + minY); //Offset the lookup by the minimum coords
					if (!tile.ValuesHere.ContainsKey(layer))
						throw new System.ArgumentException("No such layer is present on the tile to fetch!"); //Crash the function and alert the editor.
					else
						array[i, j] = tile.ValuesHere[layer];
				}
			}

			return array;
		}
	}
}