using System.Drawing;
using System.Collections.Generic;

namespace ProcGenTiles
{
    public class ColorEnumerator
    {
        private List<Color> colors;
        private int currentIndex;

        public ColorEnumerator(List<Color> colors)
        {
            this.colors = colors;
            this.currentIndex = 0;
        }

        public Color Next()
        {
            Color nextColor = colors[currentIndex];

            // Increment the index for the next call
            currentIndex = (currentIndex + 1) % colors.Count;

            return nextColor;
        }

        public Color Next(int index)
        {
            currentIndex = index;
            return Next();
        }
    }
}