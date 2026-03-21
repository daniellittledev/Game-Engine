using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;
using SharpDX.Direct3D9;
using System.Drawing;

namespace EngineX.GUI.Controls
{
    public class Form : Control
    {
        private bool isMouseDown;
        private Point grabPos;

        private int marginTop, marginBottom, marginSide;
        //private Vector2 oldLocation;

        private bool mouseOverClosing;

        public Form()
        {
            // events
            base.MouseMove += new System.Windows.Forms.MouseEventHandler(OnMouseMove);
            base.MouseDown += new System.Windows.Forms.MouseEventHandler(OnMouseDown);
            base.MouseUp += new System.Windows.Forms.MouseEventHandler(OnMouseUp);

            marginTop = 20;
            marginBottom = 5;
            marginSide = 5;
        }

        public override void Rebuild()
        {
            //base.Rebuild();

            this.graphic = VectorGraphics.Graphic.Join(

                // Main Form Part
                VectorGraphics.Graphic.BuildFullRectangle(location, this.size, Color.Blue, Color.LightBlue,
                EngineX.GUI.VectorGraphics.ShadeTypes.ShadeBottomToTop, 2, EngineX.GUI.VectorGraphics.BorderStyle.Raised,
                EngineX.GUI.VectorGraphics.Border.All),

                // Client Area Part
                VectorGraphics.Graphic.BuildFullRectangle(new Vector2(location.X + marginSide, location.Y + marginTop),
                new Size(size.Width - marginSide - marginSide, size.Height - marginTop - marginBottom), Color.Blue, Color.LightBlue,
                EngineX.GUI.VectorGraphics.ShadeTypes.ShadeBottomToTop, 2, EngineX.GUI.VectorGraphics.BorderStyle.Flat,
                EngineX.GUI.VectorGraphics.Border.All)//,

                //// Close Button
                //VectorGraphics.Graphic.BuildFullRectangle(this.Location, this.size, Color.Blue, Color.DarkBlue,
                //EngineX.GUI.VectorGraphics.ShadeTypes.ShadeBottomToTop, 2, EngineX.GUI.VectorGraphics.BorderStyle.Raised, 
                //EngineX.GUI.VectorGraphics.Border.All),
                );
        }

        private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isMouseDown)
            {
                if (Intersection.RectangleTest(location, new Size(size.Width, marginTop), e.Location))
                {
                    this.Location += new Vector2(e.X - grabPos.X, e.Y - grabPos.Y);
                    grabPos = e.Location;
                }
            }
        }


        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isMouseDown == false)
            {
                grabPos = e.Location;
            }
            isMouseDown = true;
        }


        private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isMouseDown = false;
        }
    }
}
