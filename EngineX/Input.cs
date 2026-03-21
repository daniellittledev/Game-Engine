using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SharpDX.DirectInput;

namespace EngineX
{

    namespace Input
    {

        /// <summary>
        /// Keyboard input manager
        /// </summary>
        public class Keyboard : IDisposable
        {
            private DirectInput _directInput;
            private SharpDX.DirectInput.Keyboard _keyboard;
            private KeyboardState _state;

            /// <summary>
            /// Initialize keyboard input
            /// </summary>
            public Keyboard(Form form)
            {
                _directInput = new DirectInput();
                _keyboard = new SharpDX.DirectInput.Keyboard(_directInput);
                _keyboard.SetCooperativeLevel(form.Handle, CooperativeLevel.Background | CooperativeLevel.NonExclusive);

                try
                {
                    _keyboard.Acquire();
                }
                catch (SharpDX.SharpDXException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            /// <summary>
            /// Retrieve input state
            /// </summary>
            public void Poll()
            {
                try
                {
                    _keyboard.Poll();
                    _state = _keyboard.GetCurrentState();
                }
                catch (SharpDX.SharpDXException)
                {
                    try
                    {
                        _keyboard.Acquire();
                    }
                    catch (SharpDX.SharpDXException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            /// <summary>
            /// Retrieve current keyboard state
            /// </summary>
            public KeyboardState State
            {
                get { return _state; }
            }

            /// <summary>
            /// Check if a key is currently pressed
            /// </summary>
            public bool IsKeyDown(Key key)
            {
                return _state != null && _state.IsPressed(key);
            }

            #region Dispose Pattern

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        if (_keyboard != null) { _keyboard.Unacquire(); _keyboard.Dispose(); }
                        if (_directInput != null) { _directInput.Dispose(); }
                    }
                }
                _disposed = true;
            }

            ~Keyboard()
            {
                Dispose(false);
            }

            private bool _disposed;

            #endregion
        }

        /// <summary>
        /// Mouse input manager
        /// </summary>
        public class Mouse : IDisposable
        {
            private DirectInput _directInput;
            private SharpDX.DirectInput.Mouse _mouse;
            private MouseState _state;

            /// <summary>
            /// Initialize mouse input
            /// </summary>
            public Mouse(Form form)
            {
                _directInput = new DirectInput();
                _mouse = new SharpDX.DirectInput.Mouse(_directInput);
                _mouse.SetCooperativeLevel(form.Handle, CooperativeLevel.Background | CooperativeLevel.NonExclusive);

                try
                {
                    _mouse.Acquire();
                }
                catch (SharpDX.SharpDXException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            /// <summary>
            /// Retrieve input state
            /// </summary>
            public void Poll()
            {
                try
                {
                    _mouse.Poll();
                    _state = _mouse.GetCurrentState();
                }
                catch (SharpDX.SharpDXException)
                {
                    try
                    {
                        _mouse.Acquire();
                    }
                    catch (SharpDX.SharpDXException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            /// <summary>
            /// Retrieve current mouse state
            /// </summary>
            public MouseState State
            {
                get { return _state; }
            }

            /// <summary>
            /// Get mouse button states (true = pressed)
            /// </summary>
            public bool[] MouseButtons
            {
                get { return _state != null ? _state.Buttons : new bool[8]; }
            }

            /// <summary>
            /// Mouse X axis delta
            /// </summary>
            public int X { get { return _state != null ? _state.X : 0; } }

            /// <summary>
            /// Mouse Y axis delta
            /// </summary>
            public int Y { get { return _state != null ? _state.Y : 0; } }

            /// <summary>
            /// Mouse scroll wheel delta
            /// </summary>
            public int Z { get { return _state != null ? _state.Z : 0; } }

            #region Dispose Pattern

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        if (_mouse != null) { _mouse.Unacquire(); _mouse.Dispose(); }
                        if (_directInput != null) { _directInput.Dispose(); }
                    }
                }
                _disposed = true;
            }

            ~Mouse()
            {
                Dispose(false);
            }

            private bool _disposed;

            #endregion
        }

    }

}
