using System.Drawing;
using System.Dynamic;

namespace ProcGenTiles{
	public class TerrainInfo
	{
		public Color GlyphColor { get; private set; }
		public char DisplayCharacter { get; private set; }
		public string TerrainName { get; set; }
		
		public TerrainInfo(Color glyph, char symbol, string name = "default")
		{
			GlyphColor = glyph;
			DisplayCharacter = symbol;
			TerrainName = name;
		}
		
	}
}