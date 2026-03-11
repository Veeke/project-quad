using UnityEngine;

namespace ProjectQuad
{
    public class Vector2IntRangeAttribute : PropertyAttribute
    {
        public readonly int minX;
        public readonly int minY;
        public readonly int maxX;
        public readonly int maxY;

        public Vector2IntRangeAttribute(int minX, int minY, int maxX, int maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }
    }
}
