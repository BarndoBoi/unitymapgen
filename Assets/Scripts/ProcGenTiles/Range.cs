namespace ProcGenTiles
{
    public class Range
    {
        public float Minimum { get; set; }
        public float Maximum { get; set; }

        public bool InRange(float value) => value >= Minimum && value < Maximum;

        public Range(float min, float max){
            Minimum = min;
            Maximum = max;
        }
    }
}