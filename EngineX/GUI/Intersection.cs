using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using System.Drawing;

namespace EngineX.GUI
{
        public static class Intersection
        {
            public static bool RectangleTest(Vector2 position, System.Drawing.Size size, Point cursorLocation)
            {
                int xEvent = cursorLocation.X;
                int yEvent = cursorLocation.Y;
                int xPos = (int)position.X;
                int yPos = (int)position.Y;

                if ((xEvent >= xPos && xEvent <= xPos + size.Width) &&
                    (yEvent >= yPos && yEvent <= yPos + size.Height))
                    return true;
                else
                    return false;
            }

            public static bool CircleTest(Vector2 center, float radius, Point cursorLocation)
            {
                float d = (cursorLocation.X - center.X) * (cursorLocation.X - center.X) +
                          (cursorLocation.Y - center.Y) * (cursorLocation.Y - center.Y);
                if (d <= radius * radius)
                    return true;
                else
                    return false;
            }
        }
}
