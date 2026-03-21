using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;

using System.Windows.Forms;

namespace EngineX.GUI
{
    public class Control
    {

        protected Vector2 location;
        public Vector2 Location
        {
            get { return location; }
            set { location = value;
            needsRebuild = true;
        }
        }

        protected System.Drawing.Size size;
        public System.Drawing.Size Size
        {
            get { return size; }
            set { size = value;
            needsRebuild = true;
        }
        }

        protected string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private bool focus;
        public bool InFocus
        {
            get { return focus; }
        }

        private bool mouseEntered;
        protected bool needsRebuild;

        //private GUIManager manager;
        protected Control parent;
        protected List<Control> children;
        protected VectorGraphics.Graphic graphic;

        //public VectorGraphics.Graphic Graphic
        //{
        //    get { return graphic; }
        //}
        private VectorGraphics.Graphic renderGraphic;

        public Control()
        {

            children = new List<Control>();

            needsRebuild = true;
        }

        public void SetParent(Control parent)
        {
            this.parent = parent;
        }

        public void AddControl(Control control)
        {
            children.Add(control);
        }

        public virtual void Update(float elapsedTime)
        { 
            // Make any regular updates
        
        }

        public virtual void Rebuild()
        {

            graphic = VectorGraphics.Graphic.BuildFullRectangle(location, size,
                  System.Drawing.Color.Blue, System.Drawing.Color.Aqua, VectorGraphics.ShadeTypes.ShadeBottomToTop, 1,
                  EngineX.GUI.VectorGraphics.BorderStyle.Raised);
        
        }

        public VectorGraphics.Graphic Render()
        {

            if (needsRebuild == true)
            {
                BuildControl();
            }

            return renderGraphic;
        
        }

        private void BuildControl()
        {
            
            if (graphicUpdating != null)
            {
                graphicUpdating(this, new EventArgs());
            }

            Rebuild();

            // Join childen, this and pass up
            renderGraphic = VectorGraphics.Graphic.Join(graphic, VectorGraphics.Graphic.Join(children.ToArray()));

            if (parent != null)
            {
                parent.BuildControl();
            }
        }

        public void ProccessMouseMove(object sender, MouseEventArgs e)
        {
            foreach (Control control in children)
            {
                if (Intersection.RectangleTest(control.location, control.size, e.Location))
                { 
                    control.ProccessMouseMove(sender, e);
                    control.ProccessMouseEnter(sender, new EventArgs());
                }
                else
                {
                    control.ProccessMouseLeave(sender, new EventArgs());
                }
            }
            
            if (MouseMove != null)
            {
                MouseMove(sender, e);
            }
        }

        public void ProccessMouseClick(object sender, EventArgs e)
        {
            foreach (Control control in children)
            {
                if (control.InFocus)
                {
                    control.ProccessMouseClick(sender, e);
                    continue;
                }
            }

            if (MouseClick != null)
            {
                MouseClick(sender, e);
            }
        }

        public void ProccessMouseUp(object sender, MouseEventArgs e)
        {
            foreach (Control control in children)
            {
                //if (Intersection.RectangleTest(control.location, control.size, e.Location))
                //{
                    control.ProccessMouseUp(sender, e);
                    continue;
                //}
            }

            if (MouseUp != null)
            {
                MouseUp(sender, e);
            }
        }

        public void ProccessMouseDown(object sender, MouseEventArgs e)
        {
            foreach (Control control in children)
            {
                if (Intersection.RectangleTest(control.location, control.size, e.Location))
                {
                    control.ProccessMouseDown(sender, e);
                    continue;
                }
            }

            if (MouseDown != null)
            {
                MouseDown(sender, e);
            }
        }

        public void ProccessMouseEnter(object sender, EventArgs e)
        {
            if (!mouseEntered)
            {
                mouseEntered = true;

                if (MouseEnter != null)
                {
                    MouseEnter(sender, e);
                }
            }
        }

        public void ProccessMouseLeave(object sender, EventArgs e)
        {
            if (mouseEntered)
            {
                mouseEntered = false;

                if (MouseOut != null)
                {
                    MouseOut(sender, e);
                }
            }
        }

        public void ProccessFocusGained(object sender, EventArgs e)
        {
            focus = true;
            if (GotFocus != null)
            {
                GotFocus(sender, e);
            }
        }

        public void ProccessFocusLost(object sender, EventArgs e)
        {
            focus = false;
            if (LostFocus != null)
            {
                LostFocus(sender, e);
            }
        }

        public void ProccessKeyDown(KeyEventArgs e)
        {
            if (KeyDown != null)
            {
                KeyDown(this, e);
            }
        }

        public void ProccessKeyPress(KeyPressEventArgs e)
        {
            if (KeyPress != null)
            {
                KeyPress(this, e);
            }
        }

        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseUp;
        public event EventHandler MouseClick;
        public event MouseEventHandler MouseMove;

        public event KeyEventHandler KeyDown;
        public event KeyPressEventHandler KeyPress;

        public event EventHandler MouseEnter;
        public event EventHandler MouseOut;

        public event EventHandler GotFocus;
        public event EventHandler LostFocus;

        public event EventHandler graphicUpdating;

    }
}
