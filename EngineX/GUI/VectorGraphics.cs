using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;
using System.Drawing;

namespace EngineX.GUI.VectorGraphics
{
    public class Graphic
    {
        // Data to hold rendering information

        int[] indices;
        public int[] Indices
        {
            get { return indices; }
            set { indices = value; }
        }

        int numPrimitives;
        public int NumPrimitives
        {
            get { return numPrimitives; }
            set { numPrimitives = value; }
        }

        CustomVertex.TransformedColored[] vertices;
        public CustomVertex.TransformedColored[] Vertices
        {
            get { return vertices; }
            set { vertices = value; }
        }


        public Graphic()
        {
            this.numPrimitives = 0;
            this.vertices = new CustomVertex.TransformedColored[0];
            this.indices = new int[0];
        }

        public Graphic(int numPrimitives, CustomVertex.TransformedColored[] vertices, int[] indices)
        {
            this.numPrimitives = numPrimitives;
            this.vertices = vertices;
            this.indices = indices;
        }

        public static Graphic Join(params Graphic[] graphicDescriptors)
        {

            int vbTotal = 0;
            int ibTotal = 0;
            int numPrimitives = 0;

            CustomVertex.TransformedColored[] vertices;
            int[] indices;

            for (int i = 0; i < graphicDescriptors.Length; i++)
            {
                Graphic descriptor = graphicDescriptors[i];
                vbTotal += descriptor.Vertices.Length;
                ibTotal += descriptor.Indices.Length;
                numPrimitives += descriptor.numPrimitives;
            }

            vertices = new CustomVertex.TransformedColored[vbTotal];
            indices = new int[ibTotal];

            int vbOffset = 0;
            int ibOffset = 0;

            for (int i = 0; i < graphicDescriptors.Length; i++)
            {
                Graphic descriptor = graphicDescriptors[i];
                Array.Copy(descriptor.Vertices, 0, vertices, vbOffset, descriptor.Vertices.Length);

                for (int j = 0; j < descriptor.Indices.Length; j++)
                    indices[j + ibOffset] = descriptor.Indices[j] + vbOffset;

                vbOffset += descriptor.Vertices.Length;
                ibOffset += descriptor.Indices.Length;
            }

            Graphic result = new Graphic(numPrimitives, vertices, indices);
            return result;
        
        }

        public static Graphic Join(params Control[] graphicControl)
        {

            int vbTotal = 0;
            int ibTotal = 0;
            int numPrimitives = 0;

            CustomVertex.TransformedColored[] vertices;
            int[] indices;

            for (int i = 0; i < graphicControl.Length; i++)
            {
                Graphic descriptor = graphicControl[i].Render();
                vbTotal += descriptor.Vertices.Length;
                ibTotal += descriptor.Indices.Length;
                numPrimitives += descriptor.numPrimitives;
            }

            vertices = new CustomVertex.TransformedColored[vbTotal];
            indices = new int[ibTotal];

            int vbOffset = 0;
            int ibOffset = 0;

            for (int i = 0; i < graphicControl.Length; i++)
            {
                Graphic descriptor = graphicControl[i].Render();
                Array.Copy(descriptor.Vertices, 0, vertices, vbOffset, descriptor.Vertices.Length);

                for (int j = 0; j < descriptor.Indices.Length; j++)
                    indices[j + ibOffset] = descriptor.Indices[j] + vbOffset;

                vbOffset += descriptor.Vertices.Length;
                ibOffset += descriptor.Indices.Length;
            }

            Graphic result = new Graphic(numPrimitives, vertices, indices);
            return result;

        }

        static float darkShadeFactor = 0.5f;
        //static float lightShadeFactor = 1.15f;

        private static Color ScaleColor(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                (int)Math.Min(255, color.R * factor),
                (int)Math.Min(255, color.G * factor),
                (int)Math.Min(255, color.B * factor));
        }

        #region ShadingMode methods

        /// <summary>
        /// Shades a rectangle top to bottom using the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>An array of ints containing the color of each vertex (0: top left 1: top right 2: bottomLeft 3: bottom right</returns>
        public static int[] ShadeTopToBottom(Color color)
        {
            int colorTopLeft = color.ToArgb();
            int colorTopRight = colorTopLeft;
            int colorBottomLeft = Color.FromArgb(color.A, ScaleColor(color, darkShadeFactor)).ToArgb();
            int colorBottomRight = colorBottomLeft;

            return new int[] { colorTopLeft, colorTopRight, colorBottomLeft, colorBottomRight };
        }

        /// <summary>
        /// Shades a rectangle bottom to top using the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>An array of ints containing the color of each vertex (0: top left 1: top right 2: bottomLeft 3: bottom right</returns>
        public static int[] ShadeBottomToTop(Color color)
        {
            int colorTopLeft = Color.FromArgb(color.A, ScaleColor(color, darkShadeFactor)).ToArgb();
            int colorTopRight = colorTopLeft;
            int colorBottomLeft = color.ToArgb();
            int colorBottomRight = colorBottomLeft;

            return new int[] { colorTopLeft, colorTopRight, colorBottomLeft, colorBottomRight };
        }

        /// <summary>
        /// Shades a rectangle left to right using the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>An array of ints containing the color of each vertex (0: top left 1: top right 2: bottomLeft 3: bottom right</returns>
        public static int[] ShadeLeftToRight(Color color)
        {
            int colorTopLeft = color.ToArgb();
            int colorTopRight = Color.FromArgb(color.A, ScaleColor(color, darkShadeFactor)).ToArgb();
            int colorBottomLeft = colorTopLeft;
            int colorBottomRight = colorTopRight;

            return new int[] { colorTopLeft, colorTopRight, colorBottomLeft, colorBottomRight };
        }

        /// <summary>
        /// Shades a rectangle right to left using the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>An array of ints containing the color of each vertex (0: top left 1: top right 2: bottomLeft 3: bottom right</returns>
        public static int[] ShadeRightToLeft(Color color)
        {
            int colorTopLeft = Color.FromArgb(color.A, ScaleColor(color, darkShadeFactor)).ToArgb();
            int colorTopRight = color.ToArgb();
            int colorBottomLeft = colorTopLeft;
            int colorBottomRight = colorTopRight;

            return new int[] { colorTopLeft, colorTopRight, colorBottomLeft, colorBottomRight };
        }

        /// <summary>
        /// It assigns the same color for each of the four vertex.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>An array of ints containing the color of each vertex (0: top left 1: top right 2: bottomLeft 3: bottom right</returns>
        public static int[] ShadeNone(Color color)
        {
            int colorToArgb = color.ToArgb();
            return new int[] { colorToArgb, colorToArgb, colorToArgb, colorToArgb };
        }

        #endregion

        #region Circle

        /// <summary>
        /// Builds a circle with an outline.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="outlineRadius">The outline radius.</param>
        /// <param name="slices">The slices.</param>
        /// <param name="borderSize">Size of the border.</param>
        /// <param name="color">The color.</param>
        /// <param name="borderColor">Color of the border.</param>
        /// <returns>A full Circular ShapeDescriptor object.</returns>
        public static Graphic BuildFullCircle(Vector2 center, float radius, float outlineRadius,
                                                     int slices, int borderSize, Color color, Color borderColor)
        {
            return Graphic.Join(
                BuildCircle(center, radius, slices, color),
                BuildCircularOutline(center, outlineRadius, slices, borderSize, borderColor));
        }

        /// <summary>
        /// Builds a circle without an outline.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="slices">The slices.</param>
        /// <param name="color">The color.</param>
        /// <returns>A Circular ShapeDescriptor object.</returns>
        public static Graphic BuildCircle(Vector2 center, float radius, int slices, Color color)
        {
            CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[slices + 2];
            int[] indices = new int[slices * 3];
            int col1;
            float x, y;
            x = center.X;
            y = center.Y;
            col1 = color.ToArgb();

            float deltaRad = MathUtil.DegreesToRadians(360) / slices;
            float delta = 0;

            vertices[0] = new CustomVertex.TransformedColored(x, y, 0, 1, col1);

            for (int i = 1; i < slices + 2; i++)
            {
                vertices[i] = new CustomVertex.TransformedColored(
                    (float)Math.Cos(delta) * radius + x,
                    (float)Math.Sin(delta) * radius + y,
                    0, 1, col1);
                delta += deltaRad;
            }

            indices[0] = 0;
            indices[1] = 1;

            for (int i = 0; i < slices; i++)
            {
                indices[3 * i] = 0;
                indices[(3 * i) + 1] = i + 1;
                indices[(3 * i) + 2] = i + 2;
            }
            return new Graphic(slices, vertices, indices);
        }

        /// <summary>
        /// Builds a circular outline.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="slices">The slices.</param>
        /// <param name="width">The width.</param>
        /// <param name="color">The color.</param>
        /// <returns>A Circular outline ShapeDescriptor object.</returns>
        public static Graphic BuildCircularOutline(Vector2 center, float radius, int slices, int width,
                                                          Color color)
        {
            Vector2[] points = new Vector2[slices];
            float deltaRad = MathUtil.DegreesToRadians(360) / slices;
            float delta = 0;

            for (int i = 0; i < slices; i++)
            {
                points[i] = new Vector2(
                    (float)Math.Cos(delta) * radius + center.X,
                    (float)Math.Sin(delta) * radius + center.Y);

                delta += deltaRad;
            }

            return BuildPolyLine(width, color, true, points);
        }

        #endregion

        #region Ellipse

        public static Graphic BuildEllipse(
            Vector2 center, int radius1, int radius2, int slices, Color color)
        {
            return
                BuildEllipse(center, radius1, radius2, MathUtil.DegreesToRadians(0), MathUtil.DegreesToRadians(360), slices,
                            color);
        }

        public static Graphic BuildEllipse(Vector2 center, int radius1, int radius2, float radFrom, float radTo,
                                                  int slices, Color color)
        {
            CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[slices + 2];
            int[] indices = new int[slices * 3];
            int col1;
            float x, y;
            x = center.X;
            y = center.Y;
            col1 = color.ToArgb();

            float deltaRad = radTo / slices;
            float delta = radFrom;

            vertices[0] = new CustomVertex.TransformedColored(x, y, 0, 1, col1);

            for (int i = 1; i < slices + 2; i++)
            {
                vertices[i] = new CustomVertex.TransformedColored(
                    (float)Math.Cos(delta) * radius1 + x,
                    (float)Math.Sin(delta) * radius2 + y,
                    0, 1, col1);
                delta -= deltaRad;
            }

            indices[0] = 0;
            indices[1] = 1;

            for (int i = 0; i < slices; i++)
            {
                indices[3 * i] = 0;
                indices[(3 * i) + 1] = i + 2;
                indices[(3 * i) + 2] = i + 1;
            }
            return new Graphic(slices, vertices, indices);
        }

        #endregion

        #region Lines

        public static Graphic BuildLine(float width, Color color, Vector2 v1, Vector2 v2)
        {
            CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[4];
            int col1 = color.ToArgb();

            Vector2 vDir = (v1 - v2);
            vDir = new Vector2(-vDir.Y, vDir.X);
            vDir = Vector2.Normalize(vDir);
            width /= 2;

            Vector2 vTopLeft = v1 + (-width * vDir);
            Vector2 vTopRight = v1 + (width * vDir);
            Vector2 vBottomLeft = v2 + (-width * vDir);
            Vector2 vBottomRight = v2 + (width * vDir);
            vertices[0] = new CustomVertex.TransformedColored(vTopLeft.X, vTopLeft.Y, 0, 1, col1);
            vertices[1] = new CustomVertex.TransformedColored(vBottomLeft.X, vBottomLeft.Y, 0, 1, col1);
            vertices[2] = new CustomVertex.TransformedColored(vBottomRight.X, vBottomRight.Y, 0, 1, col1);
            vertices[3] = new CustomVertex.TransformedColored(vTopRight.X, vTopRight.Y, 0, 1, col1);

            int[] indices = new int[6];

            indices[0] = 0;
            indices[1] = 2;
            indices[2] = 1;
            indices[3] = 2;
            indices[4] = 0;
            indices[5] = 3;

            return new Graphic(2, vertices, indices);
        }

        public static Graphic BuildPolyLine(int width, Color color, bool closed, params Vector2[] points)
        {
            CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[4];
            Graphic[] segments;

            int col1 = color.ToArgb();

            if (closed)
            {
                segments = new Graphic[points.Length];
                for (int i = 0; i < points.Length - 1; i++)
                    segments[i] = BuildLine(width, color, points[i], points[i + 1]);
                segments[points.Length - 1] = BuildLine(width, color, points[points.Length - 1], points[0]);
            }
            else
            {
                segments = new Graphic[points.Length - 1];
                for (int i = 0; i < points.Length - 1; i++)
                    segments[i] = BuildLine(width, color, points[i], points[i + 1]);
            }
            return Graphic.Join(segments);
        }

        #endregion

        #region Rectangle

        public static Graphic BuildRectangle(Vector2 topLeft, Size size, Color color)
        {
            return BuildRectangle(topLeft, size, color, ShadeTypes.None);
        }

        public static Graphic BuildFullRectangle(Vector2 position, Size size, Color innerAreaColor,
                                                        Color borderColor, ShadeTypes shading, int borderSize,
                                                        BorderStyle borderStyle)
        {
            return BuildFullRectangle(position, size, innerAreaColor, borderColor, shading,
                                     borderSize, borderStyle, Border.All);
        }

        public static Graphic BuildFullRectangle(Vector2 position, Size size, Color innerAreaColor,
                                                        Color borderColor, ShadeTypes shading, int borderSize,
                                                        BorderStyle borderStyle, Border borders)
        {
            Graphic rectangle;
            Graphic border;

            if (innerAreaColor != Color.Empty)
            {
                rectangle = BuildRectangle(new Vector2(position.X + borderSize, position.Y + borderSize),
                                          new Size(size.Width - borderSize * 2, size.Height - borderSize * 2),
                                          innerAreaColor,
                                          shading);
            }
            else
                rectangle = null;

            if (borderStyle != BorderStyle.None)
            {
                border = BuildRectangularOutline(position, size, borderSize, borderColor, borderStyle, borders);
            }
            else
                border = null;

            if (rectangle == null && border == null)
            {
                throw new InvalidOperationException();
            }
            else if (rectangle != null && border == null)
                return rectangle;
            else if (rectangle == null)
                return border;
            else
                return Graphic.Join(rectangle, border);
        }

        public static Graphic BuildRectangle(Vector2 topLeft, Size size, Color color, ShadeTypes shading)
        {
            CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[4];

            int width = size.Width;
            int height = size.Height;

            int[] colors;

            switch (shading)
            {
                case ShadeTypes.ShadeTopToBottom:
                    colors = ShadeBottomToTop(color);
                    break;
                case ShadeTypes.ShadeBottomToTop:
                    colors = ShadeBottomToTop(color);
                    break;
                case ShadeTypes.ShadeLeftToRight:
                    colors = ShadeLeftToRight(color);
                    break;
                case ShadeTypes.ShadeRightToLeft:
                    colors = ShadeRightToLeft(color);
                    break;
                case ShadeTypes.None:
                    colors = ShadeNone(color);
                    break;
                default:
                    colors = ShadeNone(color);
                    break;
            }  

            int colorTopLeft = colors[0];
            int colorTopRight = colors[1];
            int colorBottomleft = colors[2];
            int colorBottomRight = colors[3];

            vertices[0] = new CustomVertex.TransformedColored(topLeft.X, topLeft.Y, 0, 1, colorTopLeft);
            vertices[1] = new CustomVertex.TransformedColored(topLeft.X, topLeft.Y + size.Height, 0, 1, colorBottomleft);
            vertices[2] =
                new CustomVertex.TransformedColored(topLeft.X + size.Width, topLeft.Y + size.Height, 0, 1,
                                                    colorBottomRight);
            vertices[3] = new CustomVertex.TransformedColored(topLeft.X + size.Width, topLeft.Y, 0, 1, colorTopRight);

            int[] indices = new int[6];

            indices[0] = 0;
            indices[1] = 3;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 1;

            return new Graphic(2, vertices, indices);
        }

        public static Graphic BuildRectangularOutline(Vector2 position,
                       Size size, int borderSize, Color borderColor, BorderStyle style, Border borders)
        {
            switch (style)
            {
                case BorderStyle.Raised:
                    return BuildRectangularOutline(position, size,
                                                  Color.FromArgb(255, ScaleColor(borderColor, 1f)),
                                                  Color.FromArgb(255, ScaleColor(borderColor, 0.5f)),
                                                  borderSize, borders);

                case BorderStyle.Sunken:
                    return BuildRectangularOutline(position, size,
                                                  Color.FromArgb(255, ScaleColor(borderColor, 0.5f)),
                                                  Color.FromArgb(255, ScaleColor(borderColor, 1f)),
                                                  borderSize, borders);

                case BorderStyle.Flat:
                default:
                    return BuildRectangularOutline(position, size,
                                                  borderColor, borderColor,
                                                  borderSize, borders);
            }
        }

        public static Graphic BuildRectangularOutline(Vector2 position,
                       Size size, Color borderTopAndLeft, Color borderBottomAndRight, int borderSize, Border borders)
        {
            Graphic vBorderTop = new Graphic();
            Graphic vBorderSideL = new Graphic();
            Graphic vBorderSideR = new Graphic();
            Graphic vBorderBottom = new Graphic();

            Vector2 innerPositionTopLeft = new Vector2(
                position.X + borderSize, position.Y + borderSize);

            Vector2 borderPositionTopRight = new Vector2(
                position.X + size.Width - borderSize, position.Y);

            Vector2 borderPositionBottomLeft = new Vector2(
                position.X, position.Y + size.Height - borderSize);

            Size borderTop = new Size(size.Width, borderSize);
            Size borderSide = new Size(borderSize, size.Height);

            if ((borders & Border.Top) != 0)
                vBorderTop = BuildRectangle(
                    position, borderTop, borderTopAndLeft);
            if ((borders & Border.Left) != 0)
                vBorderSideL = BuildRectangle(
                    position, borderSide, borderTopAndLeft);
            if ((borders & Border.Right) != 0)
                vBorderSideR = BuildRectangle(
                    borderPositionTopRight, borderSide, borderBottomAndRight);
            if ((borders & Border.Bottom) != 0)
                vBorderBottom = BuildRectangle(
                    borderPositionBottomLeft, borderTop, borderBottomAndRight);

            return Graphic.Join(vBorderTop, vBorderSideL, vBorderSideR, vBorderBottom);
        }

        #endregion

        #region Triangles

        public static Graphic BuildEquilateralTriangle(Vector2 leftVertex, float sideLength, Color color,
                                                              bool isShaded, bool isTriangleUpside)
        {
            CustomVertex.TransformedColored[] vertices = new CustomVertex.TransformedColored[3];
            Color shaded;
            float heightOffset = (float)(sideLength / 2 * Math.Sqrt(3));

            int col1 = color.ToArgb();
            int col2;

            if (isShaded)
            {
                shaded = Color.FromArgb(color.A, ScaleColor(color, 0.5f));
                col2 = shaded.ToArgb();
            }
            else
                col2 = col1;

            vertices[0] = new CustomVertex.TransformedColored(leftVertex.X, leftVertex.Y, 0, 1, col2);
            vertices[1] = new CustomVertex.TransformedColored(leftVertex.X + sideLength, leftVertex.Y, 0, 1, col2);

            int[] indices = new int[3];

            if (isTriangleUpside)
            {
                heightOffset *= -1;
                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;
            }
            else
            {
                indices[0] = 2;
                indices[1] = 0;
                indices[2] = 1;
            }

            vertices[2] =
                new CustomVertex.TransformedColored(leftVertex.X + sideLength / 2, leftVertex.Y + heightOffset, 0, 1, col1);


            return new Graphic(1, vertices, indices);
        }

        #endregion

    }

    public enum ShadeTypes
    {
        ShadeTopToBottom,
        ShadeBottomToTop,
        ShadeLeftToRight,
        ShadeRightToLeft,
        None
    }


    public enum BorderStyle
    {
        NotSet,
        None,
        Flat,
        Raised,
        Sunken,
    }

    [Flags]
    public enum Border : int
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8,
        All = Top | Bottom | Left | Right
    }

}
