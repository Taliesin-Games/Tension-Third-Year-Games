using UnityEngine;

namespace Utils
{
    public static class Helpers
    {
        const float DEFAULT_DEBUG_RADIUS = 10.0f;
        static readonly Color DEFAULT_DRAW_COLOUR = Color.red;
        const int   DEFAULT_DEBUG_SEGMENTS = 24;


        public static void DebugDrawCircle(Vector3 center)
        {
            DebugDrawCircle(center, DEFAULT_DEBUG_RADIUS, DEFAULT_DRAW_COLOUR, DEFAULT_DEBUG_SEGMENTS);
        }
        public static void DebugDrawCircle(Vector3 center, float radius)
        {
            DebugDrawCircle(center, radius, DEFAULT_DRAW_COLOUR, DEFAULT_DEBUG_SEGMENTS);

        }
        public static void DebugDrawCircle(Vector3 center, float radius, Color colour)
        {
            DebugDrawCircle(center, radius, colour, DEFAULT_DEBUG_SEGMENTS);
        }
        public static void DebugDrawCircle(Vector3 center, float radius, Color colour, int segments = 24)
        {
            
            Vector3 prev = center + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float ang = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(ang) * radius, 0, Mathf.Sin(ang) * radius);
                Debug.DrawLine(prev, next, colour);
                prev = next;
            }
        }
    }
}