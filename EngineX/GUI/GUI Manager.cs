using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using System.Windows.Forms;

namespace EngineX.GUI
{
    public class GUIManager
    {
        private Device device;
        private System.Windows.Forms.Control target;
        //private Control topControl; // Fixed Window, Link Desktop
        private Control focusedControl;
        private List<Control> windows;

        public GUIManager(Device Device, Vector2 size, System.Windows.Forms.Control Target)
        {
            device = Device;
            target = Target;

            //topControl = new Control();
            focusedControl = null;

            windows = new List<Control>(35);

            //Setup Hooks
            target.KeyDown += new KeyEventHandler(KeyDown);
            target.KeyPress += new KeyPressEventHandler(KeyPress);

            target.MouseDown += new MouseEventHandler(MousePress);
            target.MouseUp += new MouseEventHandler(MouseRelease);
            target.MouseMove += new MouseEventHandler(MouseMove);

            target.Click += new EventHandler(MouseClicked);

        }

        public void Update(float elapsedTime)
        {
            foreach (Control control in windows)
            {
                control.Update(elapsedTime);
            }
        }

        public void Render()
        {
            // Commplete Rendering Code
            VectorGraphics.Graphic renderInfo = VectorGraphics.Graphic.Join(windows.ToArray());

            //device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
            //device.TextureState[0].AlphaArgument1 = TextureArgument.Current;


            device.RenderState.Lighting = false;
            device.RenderState.ZBufferWriteEnable = false;

            //device.RenderState.AlphaBlendOperation = BlendOperation.Add;

            device.RenderState.AlphaBlendOperation = BlendOperation.Add;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.InvSourceAlpha;


            device.VertexFormat = CustomVertex.TransformedColored.Format;

            device.SetTexture(0, null);

            device.RenderState.AlphaTestEnable = true;
            device.RenderState.AlphaBlendEnable = true;
            //device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
            //                             0, renderInfo.BaseVertex, renderInfo.VertexCount, renderInfo.BaseIndex,
            //                             renderInfo.PrimitiveCount);
            device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, renderInfo.Vertices.Length, renderInfo.Vertices.Length,
                                             renderInfo.NumPrimitives, renderInfo.Indices, false, renderInfo.Vertices);


            device.RenderState.AlphaBlendEnable = false;
            device.RenderState.AlphaTestEnable = false;

            device.RenderState.ZBufferWriteEnable = true;
                //sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortTexture);
                //for (int i = renderInfo.BaseLabelIndex; i < renderInfo.LabelCount; i++)
                //    spriteList[i].Render();
                //sprite.End();


        }

        public void AddWindow(Control window)
        {
            windows.Add(window);
        }

        private void TranslateLocation(MouseEventArgs e)
        {
            e = new MouseEventArgs(e.Button, e.Clicks, 
            e.X - target.ClientRectangle.X,
            e.Y - target.ClientRectangle.Y,
            e.Delta);        
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            //TranslateLocation(e);

            for (int index = windows.Count - 1; index > -1; index--)
            {
                if (Intersection.RectangleTest(windows[index].Location, windows[index].Size, e.Location))
                {
                    windows[index].ProccessMouseMove(sender, e);
                    break;
                }
            }

        }

        private void MousePress(object sender, MouseEventArgs e)
        {
            //TranslateLocation(e);
            for (int index = windows.Count - 1; index > -1; index--)
            {
                if (Intersection.RectangleTest(windows[index].Location, windows[index].Size, e.Location))
                {
                    // Update Focus
                    if (focusedControl != null)
                        focusedControl.ProccessFocusLost(this, new EventArgs());

                    focusedControl = windows[index];
                    focusedControl.ProccessFocusGained(this, new EventArgs());

                    // Update Z-Order
                    windows.RemoveAt(index);
                    windows.Add(focusedControl);

                    // Set focused window
                    windows[index].ProccessMouseDown(sender, e);
                    break;
                }
            }
        }

        private void MouseRelease(object sender, MouseEventArgs e)
        {
            //TranslateLocation(e);
            for (int index = windows.Count - 1; index > -1; index--)
            {
                //if (Intersection.RectangleTest(windows[index].Location, windows[index].Size, e.Location))
                //{
                    windows[index].ProccessMouseUp(sender, e);
                //}
            }
        }

        private void MouseClicked(object sender, EventArgs e)
        {
            // Which Window

            // Test if in focus (maybe)
            for (int index = windows.Count - 1; index > -1; index--)
            {
                if (windows[index].InFocus)
                {
                    windows[index].ProccessMouseClick(sender, e);
                    break;
                }
            }

        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (focusedControl != null)
                focusedControl.ProccessKeyDown(e);
        }

        private void KeyPress(object sender, KeyPressEventArgs e)
        {
            if (focusedControl != null)
                focusedControl.ProccessKeyPress(e);
        }

    }

}
