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
		
	}
}