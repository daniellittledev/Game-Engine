using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

namespace EngineX
{

    namespace Input
    {

        /// <summary>
        /// Keyboard input manager
        /// </summary>
        public class Keyboard : IDisposable
        {
            /// <summary>
            /// Initilize
            /// </summary>
            /// <param name="form"></param>
            public Keyboard(Form form)
            {
                _device = new Device(SystemGuid.Keyboard);
                _device.SetCooperativeLevel(form, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                _device.SetDataFormat(DeviceDataFormat.Keyboard);

                try
                {
                    _device.Acquire();
                }
                catch (DirectXException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            /// <summary>
            /// Retrieve input
            /// </summary>
            public void Poll()
            {
                try
                {
                    _device.Poll();
                    _state = _device.GetCurrentKeyboardState();
                }
                catch (NotAcquiredException)
                {
                    // try to reqcquire the device
                    try
                    {
                        _device.Acquire();
                    }
                    catch (InputException iex)
                    {
                        Console.WriteLine(iex.Message);
                        // could not get the device
                    }
                }
                catch (InputException ex2)
                {
                    Console.WriteLine(ex2.Message);
                }
            }

            /// <summary>
            /// Retrieve State
            /// </summary>
            public KeyboardState State
            {
                get { return _state; }
            }

            # region Dispose Pattern
            /// <summary>
            /// Dispose object
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool disposing)
            {
                if (!this._disposed)
                {
                    if (disposing)
                    {
                        // Free other state (managed objects).
                    }
                    // Free your own state (unmanaged objects).
                    // Set large fields to null.
                    //                if ( _device != null )
                    //                    _device.Unacquire ( );
                }
                _disposed = true;
            }

            // Use C# destructor syntax for finalization code.
            ~Keyboard()
            {
                // Simply call Dispose(false).
                Dispose(false);
            }

            private bool _disposed;

            # endregion Dispose Pattern

            /// <summary>
            /// Keyboard Device
            /// </summary>
            private Device _device;
            /// <summary>
            /// Keyboard State
            /// </summary>
            private KeyboardState _state;
        }

        /// <summary>
        /// Mouse input manager
        /// </summary>
        public class Mouse : IDisposable
        {
            /// <summary>
            /// Initilize
            /// </summary>
            /// <param name="form"></param>
            public Mouse(Form form)
            {
                _device = new Device(SystemGuid.Mouse);
                _device.SetCooperativeLevel(form, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                _device.SetDataFormat(DeviceDataFormat.Mouse);

                try
                {
                    _device.Acquire();
                }
                catch (DirectXException dex)
                {
                    Console.WriteLine(dex.Message);
                }
            }

            /// <summary>
            /// Retrieve Input
            /// </summary>
            public void Poll()
            {
                try
                {
                    _device.Poll();
                    _state = _device.CurrentMouseState;
                    _buttonBuffer = _state.GetMouseButtons();
                }
                catch (NotAcquiredException)
                {
                    // try to reqcquire the device
                    try
                    {
                        _device.Acquire();
                    }
                    catch (InputException iex)
                    {
                        Console.WriteLine(iex.Message);
                        // could not get the device
                    }
                }
                catch (InputException ex2)
                {
                    Console.WriteLine(ex2.Message);
                }
            }

            /// <summary>
            /// Retrieve State
            /// </summary>
            public MouseState State
            {
                get { return _state; }
            }

            /// <summary>
            /// Retrieve Buttons
            /// </summary>
            public byte[] MouseButtons
            {
                get { return _buttonBuffer; }
            }

            # region Dispose Pattern

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this._disposed)
                {
                    if (disposing)
                    {
                        // Free other state (managed objects).
                    }
                    // Free your own state (unmanaged objects).
                    // Set large fields to null.
                    //                if ( _device != null )
                    //                    _device.Unacquire ( );
                }
                _disposed = true;
            }

            // Use C# destructor syntax for finalization code.
            ~Mouse()
            {
                // Simply call Dispose(false).
                Dispose(false);
            }

            private bool _disposed;

            # endregion Dispose Pattern

            /// <summary>
            /// Mouse Device
            /// </summary>
            private Device _device;
            /// <summary>
            /// Mouse State
            /// </summary>
            private MouseState _state;
            /// <summary>
            /// Mouse Buttons
            /// </summary>
            private byte[] _buttonBuffer;
        }

    }

}