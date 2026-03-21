using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace EngineX
{

    namespace Structures
    {

        /// <summary>
        /// Screen Resolutions
        /// </summary>
        public enum Resolutions
        {
            R640x480 = 0,
            R800x600 = 1,
            R1024x768 = 2,
            R1280x1024 = 3
        }

        /// <summary>
        /// Structre Size 2D Vector
        /// </summary>
        public struct Size
        {
            public float X;
            public float Y;

            /// <summary>
            /// New Size
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            public Size(float x, float y)
            {
                X = x;
                Y = y;
            }

            /// <summary>
            /// Release Variables
            /// </summary>
            public void Release()
            {
                X = 0;
                Y = 0;
            }

            /// <summary>
            /// Multiply Sizes
            /// </summary>
            /// <param name="One"></param>
            /// <param name="Two"></param>
            /// <returns></returns>
            static public Size operator *(Size One, Size Two)
            {
                return new Size(One.X * Two.X, One.Y * Two.Y);
            }

            /// <summary>
            /// Add Sizes
            /// </summary>
            /// <param name="One"></param>
            /// <param name="Two"></param>
            /// <returns></returns>
            static public Size operator +(Size One, Size Two)
            {
                return new Size(One.X + Two.X, One.Y + Two.Y);
            }

            /// <summary>
            /// Subtract Sizes
            /// </summary>
            /// <param name="One"></param>
            /// <param name="Two"></param>
            /// <returns></returns>
            static public Size operator -(Size One, Size Two)
            {
                return new Size(One.X - Two.X, One.Y - Two.Y);
            }

            /// <summary>
            /// Devide Sizes
            /// </summary>
            /// <param name="One"></param>
            /// <param name="Two"></param>
            /// <returns></returns>
            static public Size operator /(Size One, Size Two)
            {
                return new Size(One.X / Two.X, One.Y / Two.Y);
            }

        }


    }

}
