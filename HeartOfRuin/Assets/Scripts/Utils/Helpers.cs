using UnityEngine;

namespace Utils
{
    public static class Helpers
    {
        const float DEFAULT_DEBUG_RADIUS = 10.0f;
        static readonly Color DEFAULT_DRAW_COLOUR = Color.red;
        const int   DEFAULT_DEBUG_SEGMENTS = 24;

        public static void DebugDrawCircle(Vector3 center) { DebugDrawCircle(center, DEFAULT_DEBUG_RADIUS, DEFAULT_DRAW_COLOUR, DEFAULT_DEBUG_SEGMENTS); }
        public static void DebugDrawCircle(Vector3 center, float radius) { DebugDrawCircle(center, radius, DEFAULT_DRAW_COLOUR, DEFAULT_DEBUG_SEGMENTS); }
        public static void DebugDrawCircle(Vector3 center, float radius, Color colour) { DebugDrawCircle(center, radius, colour, DEFAULT_DEBUG_SEGMENTS); }
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
        
        public static void DebugDrawSphere(Vector3 center, float radius, Color colour, int segments = 24, int latitudeDivisionsPerRadi = 10, int longitudeDivisionsPerRadi = 10)
        {
            int latDivisions = (int)Mathf.Ceil(latitudeDivisionsPerRadi * radius);
            int longDivisions = (int)Mathf.Ceil(longitudeDivisionsPerRadi * radius);
            // Horizontal rings (latitude)
            for (int lat = 0; lat <= latDivisions; lat++)
            {
                float v = lat / (float)latDivisions;
                float theta = Mathf.Lerp(-Mathf.PI * 0.5f, Mathf.PI * 0.5f, v);

                float y = Mathf.Sin(theta) * radius;
                float ringRadius = Mathf.Cos(theta) * radius;

                Vector3 prev = center + new Vector3(ringRadius, y, 0);

                for (int i = 1; i <= segments; i++)
                {
                    float ang = (i / (float)segments) * Mathf.PI * 2f;

                    Vector3 next = center + new Vector3(
                        Mathf.Cos(ang) * ringRadius,
                        y,
                        Mathf.Sin(ang) * ringRadius
                    );

                    Debug.DrawLine(prev, next, colour);
                    prev = next;
                }
            }

            // Vertical rings (longitude)
            for (int lon = 0; lon < longDivisions; lon++)
            {
                float phi = (lon / (float)longDivisions) * Mathf.PI * 2f;

                Vector3 prev = center + new Vector3(
                    Mathf.Cos(phi) * radius,
                    0,
                    Mathf.Sin(phi) * radius
                );

                for (int i = 1; i <= segments; i++)
                {
                    float ang = (i / (float)segments) * Mathf.PI * 2f;

                    Vector3 next = center + new Vector3(
                        Mathf.Cos(phi) * Mathf.Cos(ang) * radius,
                        Mathf.Sin(ang) * radius,
                        Mathf.Sin(phi) * Mathf.Cos(ang) * radius
                    );

                    Debug.DrawLine(prev, next, colour);
                    prev = next;
                }
            }
        }
    }
}