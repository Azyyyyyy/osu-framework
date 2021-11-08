// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.ImageExtensions;
using osu.Framework.Input;
using osu.Framework.Platform.SDL2;
using osu.Framework.Platform.Windows.Native;
using osu.Framework.Threading;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

// ReSharper disable UnusedParameter.Local
// (Class regularly handles native events where we don't consume all parameters)

namespace osu.Framework.Platform
{
    /// <summary>
    /// Default implementation of a desktop window, using SDL for windowing and graphics support.
    /// </summary>
    public unsafe class SDL2DesktopWindow : IWindow
    {
        protected Window* WindowPtr;
        private readonly Sdl sdl;
        private readonly IGraphicsBackend graphicsBackend;

        private bool focused;

        /// <summary>
        /// Whether the window currently has focus.
        /// </summary>
        public bool Focused
        {
            get => focused;
            private set
            {
                if (value == focused)
                    return;

                isActive.Value = focused = value;
            }
        }

        /// <summary>
        /// Enables or disables vertical sync.
        /// </summary>
        public bool VerticalSync
        {
            get => graphicsBackend.VerticalSync;
            set => graphicsBackend.VerticalSync = value;
        }

        /// <summary>
        /// Returns true if window has been created.
        /// Returns false if the window has not yet been created, or has been closed.
        /// </summary>
        public bool Exists { get; private set; }

        public WindowMode DefaultWindowMode => Configuration.WindowMode.Windowed;

        /// <summary>
        /// Returns the window modes that the platform should support by default.
        /// </summary>
        protected virtual IEnumerable<WindowMode> DefaultSupportedWindowModes => Enum.GetValues(typeof(WindowMode)).OfType<WindowMode>();

        private Point position;

        /// <summary>
        /// Returns or sets the window's position in screen space. Only valid when in <see cref="osu.Framework.Configuration.WindowMode.Windowed"/>
        /// </summary>
        public Point Position
        {
            get => position;
            set
            {
                position = value;
                ScheduleCommand(() => sdl.SetWindowPosition(WindowPtr, value.X, value.Y));
            }
        }

        private bool resizable = true;

        /// <summary>
        /// Returns or sets whether the window is resizable or not. Only valid when in <see cref="osu.Framework.Platform.WindowState.Normal"/>.
        /// </summary>
        public bool Resizable
        {
            get => resizable;
            set
            {
                if (resizable == value)
                    return;

                resizable = value;
                ScheduleCommand(() => sdl.SetWindowResizable(WindowPtr, value ? SdlBool.True : SdlBool.False));
            }
        }

        private bool relativeMouseMode;

        /// <summary>
        /// Set the state of SDL2's RelativeMouseMode (https://wiki.libsdl.org/SDL_SetRelativeMouseMode).
        /// On all platforms, this will lock the mouse to the window (although escaping by setting <see cref="ConfineMouseMode"/> is still possible via a local implementation).
        /// On windows, this will use raw input if available.
        /// </summary>
        public bool RelativeMouseMode
        {
            get => relativeMouseMode;
            set
            {
                if (relativeMouseMode == value)
                    return;

                if (value && !CursorState.HasFlagFast(CursorMode.Hidden))
                    throw new InvalidOperationException($"Cannot set {nameof(RelativeMouseMode)} to true when the cursor is not hidden via {nameof(CursorState)}.");

                relativeMouseMode = value;
                ScheduleCommand(() => sdl.SetRelativeMouseMode(value ? SdlBool.True : SdlBool.False));
            }
        }

        private Size size = new Size(default_width, default_height);

        /// <summary>
        /// Returns or sets the window's internal size, before scaling.
        /// </summary>
        public Size Size
        {
            get => size;
            private set
            {
                if (value.Equals(size)) return;

                size = value;
                ScheduleEvent(() => Resized?.Invoke());
            }
        }

        /// <summary>
        /// Provides a bindable that controls the window's <see cref="CursorStateBindable"/>.
        /// </summary>
        public Bindable<CursorMode> CursorStateBindable { get; } = new Bindable<CursorMode>();

        public CursorMode CursorState
        {
            get => CursorStateBindable.Value;
            set => CursorStateBindable.Value = value;
        }

        public Bindable<Display> CurrentDisplayBindable { get; } = new Bindable<Display>();

        public Bindable<WindowMode> WindowMode { get; } = new Bindable<WindowMode>();

        private readonly BindableBool isActive = new BindableBool();

        public IBindable<bool> IsActive => isActive;

        private readonly BindableBool cursorInWindow = new BindableBool();

        public IBindable<bool> CursorInWindow => cursorInWindow;

        public IBindableList<WindowMode> SupportedWindowModes { get; }

        public BindableSafeArea SafeAreaPadding { get; } = new BindableSafeArea();

        public virtual Point PointToClient(Point point) => point;

        public virtual Point PointToScreen(Point point) => point;

        private const int default_width = 1366;
        private const int default_height = 768;

        private const int default_icon_size = 256;

        private readonly Scheduler commandScheduler = new Scheduler();
        private readonly Scheduler eventScheduler = new Scheduler();

        private readonly Dictionary<int, SDL2ControllerBindings> controllers = new Dictionary<int, SDL2ControllerBindings>();

        private string title = string.Empty;

        /// <summary>
        /// Gets and sets the window title.
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                title = value;
                ScheduleCommand(() => sdl.SetWindowTitle(WindowPtr, title));
            }
        }

        private bool visible;

        /// <summary>
        /// Enables or disables the window visibility.
        /// </summary>
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                ScheduleCommand(() =>
                {
                    if (value)
                        sdl.ShowWindow(WindowPtr);
                    else
                        sdl.HideWindow(WindowPtr);
                });
            }
        }

        private void updateCursorVisibility(bool visible) =>
            ScheduleCommand(() => sdl.ShowCursor(visible ? 1 : 0));

        private void updateCursorConfined(bool confined) =>
            ScheduleCommand(() => sdl.SetWindowGrab(WindowPtr, confined ? SdlBool.True : SdlBool.False));

        private WindowState windowState = WindowState.Normal;

        private WindowState? pendingWindowState;

        /// <summary>
        /// Returns or sets the window's current <see cref="WindowState"/>.
        /// </summary>
        public WindowState WindowState
        {
            get => windowState;
            set
            {
                if (pendingWindowState == null && windowState == value)
                    return;

                pendingWindowState = value;
            }
        }

        /// <summary>
        /// Stores whether the window used to be in maximised state or not.
        /// Used to properly decide what window state to pick when switching to windowed mode (see <see cref="WindowMode"/> change event)
        /// </summary>
        private bool windowMaximised;

        /// <summary>
        /// Returns the drawable area, after scaling.
        /// </summary>
        public Size ClientSize => new Size(Size.Width, Size.Height);

        public float Scale = 1;

        /// <summary>
        /// Queries the physical displays and their supported resolutions.
        /// </summary>
        public IEnumerable<Display> Displays => Enumerable.Range(0, sdl.GetNumVideoDisplays()).Select(displayFromSDL);

        /// <summary>
        /// Gets the <see cref="Display"/> that has been set as "primary" or "default" in the operating system.
        /// </summary>
        public virtual Display PrimaryDisplay => Displays.First();

        private Display currentDisplay;
        private int displayIndex = -1;

        /// <summary>
        /// Gets or sets the <see cref="Display"/> that this window is currently on.
        /// </summary>
        public Display CurrentDisplay { get; private set; }

        public readonly Bindable<ConfineMouseMode> ConfineMouseMode = new Bindable<ConfineMouseMode>();

        private readonly Bindable<DisplayMode> currentDisplayMode = new Bindable<DisplayMode>();

        /// <summary>
        /// The <see cref="DisplayMode"/> for the display that this window is currently on.
        /// </summary>
        public IBindable<DisplayMode> CurrentDisplayMode => currentDisplayMode;

        /// <summary>
        /// Gets the native window handle as provided by the operating system.
        /// </summary>
        public IntPtr WindowHandle
        {
            get
            {
                if ((IntPtr)WindowPtr == IntPtr.Zero)
                    return IntPtr.Zero;

                var wmInfo = getWindowWMInfo();

                // Window handle is selected per subsystem as defined at:
                // https://wiki.libsdl.org/SDL_SysWMinfo
                return wmInfo.Subsystem switch
                {
                    SysWMType.Windows => wmInfo.Info.Win.Hwnd,
                    SysWMType.X11 => (IntPtr)wmInfo.Info.X11.Window,
                    SysWMType.DirectFB => (IntPtr)wmInfo.Info.Dummy,
                    SysWMType.Cocoa => (IntPtr)wmInfo.Info.Cocoa.Window,
                    SysWMType.UIKit => (IntPtr)wmInfo.Info.UIKit.Window,
                    SysWMType.Wayland => (IntPtr)wmInfo.Info.Wayland.Display,
                    SysWMType.Android => (IntPtr)wmInfo.Info.Android.Window,
                    _ => IntPtr.Zero
                };
            }
        }

        private SysWMInfo getWindowWMInfo()
        {
            if ((IntPtr)WindowPtr == IntPtr.Zero)
                return default;

            var wmInfo = new SysWMInfo();
            sdl.GetWindowWMInfo(WindowPtr, &wmInfo);
            return wmInfo;
        }

        private Rectangle windowDisplayBounds
        {
            get
            {
                var rec = new Rectangle<int>();
                sdl.GetDisplayBounds(displayIndex, ref rec);
                return new Rectangle(rec.Origin.X, rec.Origin.Y, rec.Size.X, rec.Size.Y);
            }
        }

        public bool CapsLockPressed => sdl.GetModState().HasFlagFast(Keymod.KmodCaps);

        private bool firstDraw = true;

        private readonly BindableSize sizeFullscreen = new BindableSize();
        private readonly BindableSize sizeWindowed = new BindableSize();
        private readonly BindableDouble windowPositionX = new BindableDouble();
        private readonly BindableDouble windowPositionY = new BindableDouble();
        private readonly Bindable<Display> windowDisplayIndexBindable = new Bindable<Display>();

        public SDL2DesktopWindow()
        {
            SdlProvider.InitFlags = Sdl.InitVideo | Sdl.InitGamecontroller;
            sdl = SdlProvider.SDL.Value;

            graphicsBackend = CreateGraphicsBackend();

            SupportedWindowModes = new BindableList<WindowMode>(DefaultSupportedWindowModes);

            CursorStateBindable.ValueChanged += evt =>
            {
                updateCursorVisibility(!evt.NewValue.HasFlagFast(CursorMode.Hidden));
                updateCursorConfined(evt.NewValue.HasFlagFast(CursorMode.Raw));
            };

            populateJoysticks();
        }

        /// <summary>
        /// Creates the window and initialises the graphics backend.
        /// </summary>
        public virtual void Create()
        {
            WindowFlags flags = WindowFlags.WindowOpengl |
                                WindowFlags.WindowResizable |
                                WindowFlags.WindowAllowHighdpi |
                                WindowFlags.WindowHidden | // shown after first swap to avoid white flash on startup (windows)
                                WindowState.ToFlags();

            sdl.SetHint(Sdl.HintWindowsNoCloseOnAltF4, "1");
            sdl.SetHint(Sdl.HintVideoMinimizeOnFocusLoss, "1");
            WindowPtr = sdl.CreateWindow(title, Position.X, Position.Y, Size.Width, Size.Height, (uint)flags);

            Exists = true;

            MouseEntered += () => cursorInWindow.Value = true;
            MouseLeft += () => cursorInWindow.Value = false;

            if (graphicsBackend is SDL2GraphicsBackend sdl2GraphicsBackend)
            {
                sdl2GraphicsBackend.SetWindow(WindowPtr);
            }

            graphicsBackend.Initialise(this);

            updateWindowSpecifics();
            updateWindowSize();
            WindowMode.TriggerChange();
        }

        // reference must be kept to avoid GC, see https://stackoverflow.com/a/6193914
        [UsedImplicitly]
        private EventFilter eventFilterDelegate;

        /// <summary>
        /// Starts the window's run loop.
        /// </summary>
        public void Run()
        {
            // polling via SDL_PollEvent blocks on resizes (https://stackoverflow.com/a/50858339)
            sdl.SetEventFilter(eventFilterDelegate = (_, eventPtr) =>
            {
                // ReSharper disable once PossibleNullReferenceException
                var e = *eventPtr;

                if (e.Type == (uint)EventType.Windowevent && e.Window.Event == (byte)WindowEventID.WindoweventResized)
                {
                    updateWindowSize();
                }

                return 1;
            }, (void*)0);

            while (Exists)
            {
                commandScheduler.Update();

                if (!Exists)
                    break;

                if (pendingWindowState != null)
                    updateWindowSpecifics();

                pollSDLEvents();

                if (!cursorInWindow.Value)
                    pollMouse();

                eventScheduler.Update();

                Update?.Invoke();
            }

            Exited?.Invoke();

            if ((IntPtr)WindowPtr != IntPtr.Zero)
                sdl.DestroyWindow(WindowPtr);

            sdl.Quit();
        }

        /// <summary>
        /// Updates the client size and the scale according to the window.
        /// </summary>
        /// <returns>Whether the window size has been changed after updating.</returns>
        private void updateWindowSize()
        {
            int w;
            int h;
            int actualW;
            int tmp;
            sdl.GLGetDrawableSize(WindowPtr, &w, &h);
            sdl.GetWindowSize(WindowPtr, &actualW, &tmp);

            Scale = (float)w / actualW;
            Size = new Size(w, h);

            // This function may be invoked before the SDL internal states are all changed. (as documented here: https://wiki.libsdl.org/SDL_SetEventFilter)
            // Scheduling the store to config until after the event poll has run will ensure the window is in the correct state.
            eventScheduler.Add(storeWindowSizeToConfig, true);
        }

        /// <summary>
        /// Forcefully closes the window.
        /// </summary>
        public void Close() => ScheduleCommand(() => Exists = false);

        /// <summary>
        /// Attempts to close the window.
        /// </summary>
        public void RequestClose() => ScheduleEvent(() =>
        {
            if (ExitRequested?.Invoke() != true)
                Close();
        });

        public void SwapBuffers()
        {
            graphicsBackend.SwapBuffers();

            if (firstDraw)
            {
                Visible = true;
                firstDraw = false;
            }
        }

        /// <summary>
        /// Requests that the graphics backend become the current context.
        /// May not be required for some backends.
        /// </summary>
        public void MakeCurrent() => graphicsBackend.MakeCurrent();

        /// <summary>
        /// Requests that the current context be cleared.
        /// </summary>
        public void ClearCurrent() => graphicsBackend.ClearCurrent();

        private void enqueueJoystickAxisInput(JoystickAxisSource axisSource, short axisValue)
        {
            // SDL reports axis values in the range short.MinValue to short.MaxValue, so we scale and clamp it to the range of -1f to 1f
            float clamped = Math.Clamp((float)axisValue / short.MaxValue, -1f, 1f);
            ScheduleEvent(() => JoystickAxisChanged?.Invoke(new JoystickAxis(axisSource, clamped)));
        }

        private void enqueueJoystickButtonInput(JoystickButton button, bool isPressed)
        {
            if (isPressed)
                ScheduleEvent(() => JoystickButtonDown?.Invoke(button));
            else
                ScheduleEvent(() => JoystickButtonUp?.Invoke(button));
        }

        /// <summary>
        /// Attempts to set the window's icon to the specified image.
        /// </summary>
        /// <param name="image">An <see cref="Image{Rgba32}"/> to set as the window icon.</param>
        private void setSDLIcon(Image<Rgba32> image)
        {
            var pixelMemory = image.CreateReadOnlyPixelMemory();
            var imageSize = image.Size();

            ScheduleCommand(() =>
            {
                var pixelSpan = pixelMemory.Span;

                Surface* surface;
                fixed (void* ptr = pixelSpan)
                    surface = sdl.CreateRGBSurfaceFrom(ptr, imageSize.Width, imageSize.Height, 32, imageSize.Width * 4, 0xff, 0xff00, 0xff0000, 0xff000000);

                sdl.SetWindowIcon(WindowPtr, surface);
                sdl.FreeSurface(surface);
            });
        }

        private Point previousPolledPoint = Point.Empty;

        private void pollMouse()
        {
            int x = 0;
            int y = 0;
            sdl.GetGlobalMouseState(ref x, ref y);
            if (previousPolledPoint.X == x && previousPolledPoint.Y == y)
                return;

            previousPolledPoint = new Point(x, y);

            var pos = WindowMode.Value == Configuration.WindowMode.Windowed ? Position : windowDisplayBounds.Location;
            int rx = x - pos.X;
            int ry = y - pos.Y;

            ScheduleEvent(() => MouseMove?.Invoke(new Vector2(rx * Scale, ry * Scale)));
        }

        #region SDL Event Handling

        /// <summary>
        /// Adds an <see cref="Action"/> to the <see cref="Scheduler"/> expected to handle event callbacks.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to execute.</param>
        protected void ScheduleEvent(Action action) => eventScheduler.Add(action, false);

        protected void ScheduleCommand(Action action) => commandScheduler.Add(action, false);

        private const int events_per_peep = 64;
        private readonly Event[] events = new Event[events_per_peep];

        /// <summary>
        /// Poll for all pending events.
        /// </summary>
        private void pollSDLEvents()
        {
            int eventsRead;

            do
            {
                fixed (Event* evn = events)
                {
                    eventsRead = sdl.PollEvent(evn);
                }

                for (int i = 0; i < eventsRead; i++)
                    handleSDLEvent(events[i]);
            } while (eventsRead == events_per_peep);
        }

        private void handleSDLEvent(Event e)
        {
            switch ((EventType)e.Type)
            {
                case EventType.Quit:
                case EventType.AppTerminating:
                    handleQuitEvent(e.Quit);
                    break;

                case EventType.Windowevent:
                    handleWindowEvent(e.Window);
                    break;

                case EventType.Keydown:
                case EventType.Keyup:
                    handleKeyboardEvent(e.Key);
                    break;

                case EventType.Textediting:
                    handleTextEditingEvent(e.Edit);
                    break;

                case EventType.Textinput:
                    handleTextInputEvent(e.Text);
                    break;

                case EventType.Mousemotion:
                    handleMouseMotionEvent(e.Motion);
                    break;

                case EventType.Keymapchanged:
                    handleKeymapChangedEvent();
                    break;

                case EventType.Mousebuttondown:
                case EventType.Mousebuttonup:
                    handleMouseButtonEvent(e.Button);
                    break;

                case EventType.Mousewheel:
                    handleMouseWheelEvent(e.Wheel);
                    break;

                case EventType.Joyaxismotion:
                    handleJoyAxisEvent(e.Jaxis);
                    break;

                case EventType.Joyballmotion:
                    handleJoyBallEvent(e.Jball);
                    break;

                case EventType.Joyhatmotion:
                    handleJoyHatEvent(e.Jhat);
                    break;

                case EventType.Joybuttondown:
                case EventType.Joybuttonup:
                    handleJoyButtonEvent(e.Jbutton);
                    break;

                case EventType.Joydeviceadded:
                case EventType.Joydeviceremoved:
                    handleJoyDeviceEvent(e.Jdevice);
                    break;

                case EventType.Controlleraxismotion:
                    handleControllerAxisEvent(e.Caxis);
                    break;

                case EventType.Controllerbuttondown:
                case EventType.Controllerbuttonup:
                    handleControllerButtonEvent(e.Cbutton);
                    break;

                case EventType.Controllerdeviceadded:
                case EventType.Controllerdeviceremoved:
                case EventType.Controllerdeviceremapped:
                    handleControllerDeviceEvent(e.Cdevice);
                    break;

                case EventType.Fingerdown:
                case EventType.Fingerup:
                case EventType.Fingermotion:
                    handleTouchFingerEvent(e.Tfinger);
                    break;

                case EventType.Dropfile:
                case EventType.Droptext:
                case EventType.Dropbegin:
                case EventType.Dropcomplete:
                    handleDropEvent(e.Drop);
                    break;
            }
        }

        private void handleQuitEvent(QuitEvent evtQuit) => RequestClose();

        private void handleDropEvent(DropEvent evtDrop)
        {
            switch ((EventType)evtDrop.Type)
            {
                case EventType.Dropfile:
                    string str = "";
                    sdl.Utf8strlcpy(str, evtDrop.File, 1);
                    if (str != null)
                        ScheduleEvent(() => DragDrop?.Invoke(str));

                    break;
            }
        }

        private void handleTouchFingerEvent(TouchFingerEvent evtTfinger)
        {
        }

        private void handleControllerDeviceEvent(ControllerDeviceEvent evtCdevice)
        {
            switch ((EventType)evtCdevice.Type)
            {
                case EventType.Controllerdeviceadded:
                    addJoystick(evtCdevice.Which);
                    break;

                case EventType.Controllerdeviceremoved:
                    var cont = (GameController)controllers[evtCdevice.Which].ControllerHandle!;
                    sdl.GameControllerClose(&cont);

                    controllers.Remove(evtCdevice.Which);
                    break;

                case EventType.Controllerdeviceremapped:
                    if (controllers.TryGetValue(evtCdevice.Which, out var state))
                        state.PopulateBindings();

                    break;
            }
        }

        private void handleControllerButtonEvent(ControllerButtonEvent evtCbutton)
        {
            var button = ((GameControllerButton)evtCbutton.Button).ToJoystickButton();

            switch ((EventType)evtCbutton.Type)
            {
                case EventType.Controllerbuttondown:
                    enqueueJoystickButtonInput(button, true);
                    break;

                case EventType.Controllerbuttonup:
                    enqueueJoystickButtonInput(button, false);
                    break;
            }
        }

        private void handleControllerAxisEvent(ControllerAxisEvent evtCaxis) =>
            enqueueJoystickAxisInput(((GameControllerAxis)evtCaxis.Value).ToJoystickAxisSource(), evtCaxis.Axis);

        private void addJoystick(int which)
        {
            int instanceID = sdl.JoystickGetDeviceInstanceID(which);

            // if the joystick is already opened, ignore it
            if (controllers.ContainsKey(instanceID))
                return;

            var joystick = sdl.JoystickOpen(which);

            if (sdl.IsGameController(which) == SdlBool.True)
            {
                var controller = sdl.GameControllerOpen(which);
                controllers[instanceID] = new SDL2ControllerBindings(joystick, *controller);
            }

            controllers[instanceID] = new SDL2ControllerBindings(joystick, null);
        }

        /// <summary>
        /// Populates <see cref="controllers"/> with joysticks that are already connected.
        /// </summary>
        private void populateJoysticks()
        {
            for (int i = 0; i < sdl.NumJoysticks(); i++)
            {
                addJoystick(i);
            }
        }

        private void handleJoyDeviceEvent(JoyDeviceEvent evtJdevice)
        {
            switch ((EventType)evtJdevice.Type)
            {
                case EventType.Joydeviceadded:
                    addJoystick(evtJdevice.Which);
                    break;

                case EventType.Joydeviceremoved:
                    // if the joystick is already closed, ignore it
                    if (!controllers.ContainsKey(evtJdevice.Which))
                        break;

                    sdl.JoystickClose(controllers[evtJdevice.Which].JoystickHandle);
                    controllers.Remove(evtJdevice.Which);
                    break;
            }
        }

        private void handleJoyButtonEvent(JoyButtonEvent evtJbutton)
        {
            // if this button exists in the controller bindings, skip it
            if (controllers.TryGetValue(evtJbutton.Which, out var state) && state.GetButtonForIndex(evtJbutton.Button) != GameControllerButton.ControllerButtonInvalid)
                return;

            var button = JoystickButton.FirstButton + evtJbutton.Button;

            switch ((EventType)evtJbutton.Type)
            {
                case EventType.Joybuttondown:
                    enqueueJoystickButtonInput(button, true);
                    break;

                case EventType.Joybuttonup:
                    enqueueJoystickButtonInput(button, false);
                    break;
            }
        }

        private void handleJoyHatEvent(JoyHatEvent evtJhat)
        {
        }

        private void handleJoyBallEvent(JoyBallEvent evtJball)
        {
        }

        private void handleJoyAxisEvent(JoyAxisEvent evtJaxis)
        {
            // if this axis exists in the controller bindings, skip it
            if (controllers.TryGetValue(evtJaxis.Which, out var state) && state.GetAxisForIndex(evtJaxis.Axis) != GameControllerAxis.ControllerAxisInvalid)
                return;

            enqueueJoystickAxisInput(JoystickAxisSource.Axis1 + evtJaxis.Axis, evtJaxis.Value);
        }

        private void handleMouseWheelEvent(MouseWheelEvent evtWheel) =>
            ScheduleEvent(() => TriggerMouseWheel(new Vector2(evtWheel.X, evtWheel.Y), false));

        private void handleMouseButtonEvent(MouseButtonEvent evtButton)
        {
            MouseButton button = mouseButtonFromEvent(evtButton.Button);

            switch ((EventType)evtButton.Type)
            {
                case EventType.Mousebuttondown:
                    ScheduleEvent(() => MouseDown?.Invoke(button));
                    break;

                case EventType.Mousebuttonup:
                    ScheduleEvent(() => MouseUp?.Invoke(button));
                    break;
            }
        }

        private void handleMouseMotionEvent(MouseMotionEvent evtMotion)
        {
            if (sdl.GetRelativeMouseMode() == SdlBool.False)
                ScheduleEvent(() => MouseMove?.Invoke(new Vector2(evtMotion.X * Scale, evtMotion.Y * Scale)));
            else
                ScheduleEvent(() => MouseMoveRelative?.Invoke(new Vector2(evtMotion.Xrel * Scale, evtMotion.Yrel * Scale)));
        }

        private void handleTextInputEvent(TextInputEvent evtText)
        {
            var ptr = new IntPtr(evtText.Text);
            if (ptr == IntPtr.Zero)
                return;

            string text = Marshal.PtrToStringUTF8(ptr) ?? "";

            foreach (char c in text)
                ScheduleEvent(() => KeyTyped?.Invoke(c));
        }

        private void handleTextEditingEvent(TextEditingEvent evtEdit)
        {
        }

        private void handleKeyboardEvent(KeyboardEvent evtKey)
        {
            Key key = evtKey.Keysym.ToKey();

            if (key == Key.Unknown)
                return;

            switch ((EventType)evtKey.Type)
            {
                case EventType.Keydown:
                    ScheduleEvent(() => KeyDown?.Invoke(key));
                    break;

                case EventType.Keyup:
                    ScheduleEvent(() => KeyUp?.Invoke(key));
                    break;
            }
        }

        private void handleKeymapChangedEvent()
        {
            ScheduleEvent(() => KeymapChanged?.Invoke());
        }

        private void handleWindowEvent(WindowEvent evtWindow)
        {
            updateWindowSpecifics();

            switch ((WindowEventID)evtWindow.Event)
            {
                case WindowEventID.WindoweventMoved:
                    // explicitly requery as there are occasions where what SDL has provided us with is not up-to-date.
                    int x;
                    int y;
                    sdl.GetWindowPosition(WindowPtr, &x, &y);
                    var newPosition = new Point(x, y);

                    if (!newPosition.Equals(Position))
                    {
                        position = newPosition;
                        ScheduleEvent(() => Moved?.Invoke(newPosition));

                        if (WindowMode.Value == Configuration.WindowMode.Windowed)
                            storeWindowPositionToConfig();
                    }

                    break;

                case WindowEventID.WindoweventSizeChanged:
                    updateWindowSize();
                    break;

                case WindowEventID.WindoweventEnter:
                    cursorInWindow.Value = true;
                    ScheduleEvent(() => MouseEntered?.Invoke());
                    break;

                case WindowEventID.WindoweventLeave:
                    cursorInWindow.Value = false;
                    ScheduleEvent(() => MouseLeft?.Invoke());
                    break;

                case WindowEventID.WindoweventRestored:
                case WindowEventID.WindoweventFocusGained:
                    ScheduleEvent(() => Focused = true);
                    break;

                case WindowEventID.WindoweventMinimized:
                case WindowEventID.WindoweventFocusLost:
                    ScheduleEvent(() => Focused = false);
                    break;

                case WindowEventID.WindoweventClose:
                    break;
            }
        }

        /// <summary>
        /// Should be run on a regular basis to check for external window state changes.
        /// </summary>
        private void updateWindowSpecifics()
        {
            // don't attempt to run before the window is initialised, as Create() will do so anyway.
            if ((IntPtr)WindowPtr == IntPtr.Zero)
                return;

            var stateBefore = windowState;

            // check for a pending user state change and give precedence.
            if (pendingWindowState != null)
            {
                windowState = pendingWindowState.Value;
                pendingWindowState = null;

                updateWindowStateAndSize();
            }
            else
            {
                windowState = ((WindowFlags)sdl.GetWindowFlags(WindowPtr)).ToWindowState();
            }

            if (windowState != stateBefore)
            {
                ScheduleEvent(() => WindowStateChanged?.Invoke(windowState));
                updateMaximisedState();
            }

            int newDisplayIndex = sdl.GetWindowDisplayIndex(WindowPtr);

            if (displayIndex != newDisplayIndex)
            {
                displayIndex = newDisplayIndex;
                currentDisplay = Displays.ElementAtOrDefault(displayIndex) ?? PrimaryDisplay;
                ScheduleEvent(() =>
                {
                    CurrentDisplayBindable.Value = currentDisplay;
                });
            }
        }

        /// <summary>
        /// Should be run after a local window state change, to propagate the correct SDL actions.
        /// </summary>
        private void updateWindowStateAndSize()
        {
            switch (windowState)
            {
                case WindowState.Normal:
                    Size = (sizeWindowed.Value * Scale).ToSize();

                    sdl.RestoreWindow(WindowPtr);
                    sdl.SetWindowSize(WindowPtr, sizeWindowed.Value.Width, sizeWindowed.Value.Height);
                    sdl.SetWindowResizable(WindowPtr, Resizable ? SdlBool.True : SdlBool.False);

                    readWindowPositionFromConfig();
                    break;

                case WindowState.Fullscreen:
                    var closestMode = getClosestDisplayMode(sizeFullscreen.Value, currentDisplayMode.Value.RefreshRate, currentDisplay.Index);

                    Size = new Size(closestMode.W, closestMode.H);

                    sdl.SetWindowDisplayMode(WindowPtr, ref closestMode);
                    sdl.SetWindowFullscreen(WindowPtr, (uint)WindowFlags.WindowFullscreen);
                    break;

                case WindowState.FullscreenBorderless:
                    Size = SetBorderless();
                    break;

                case WindowState.Maximised:
                    sdl.RestoreWindow(WindowPtr);
                    sdl.MaximizeWindow(WindowPtr);

                    int w;
                    int h;
                    sdl.GLGetDrawableSize(WindowPtr, &w, &h);
                    Size = new Size(w, h);
                    break;

                case WindowState.Minimised:
                    sdl.MinimizeWindow(WindowPtr);
                    break;
            }

            updateMaximisedState();

            Silk.NET.SDL.DisplayMode mode;
            if (sdl.GetWindowDisplayMode(WindowPtr, &mode) >= 0)
                currentDisplayMode.Value = new DisplayMode(mode.Format.ToString(), new Size(mode.W, mode.H), 32, mode.RefreshRate, displayIndex, displayIndex);
        }

        private void updateMaximisedState()
        {
            if (windowState == WindowState.Normal || windowState == WindowState.Maximised)
                windowMaximised = windowState == WindowState.Maximised;
        }

        private void readWindowPositionFromConfig()
        {
            if (WindowState != WindowState.Normal)
                return;

            var configPosition = new Vector2((float)windowPositionX.Value, (float)windowPositionY.Value);

            var displayBounds = CurrentDisplay.Bounds;
            var windowSize = sizeWindowed.Value;
            int windowX = (int)Math.Round((displayBounds.Width - windowSize.Width) * configPosition.X);
            int windowY = (int)Math.Round((displayBounds.Height - windowSize.Height) * configPosition.Y);

            Position = new Point(windowX + displayBounds.X, windowY + displayBounds.Y);
        }

        private void storeWindowPositionToConfig()
        {
            if (WindowState != WindowState.Normal)
                return;

            var displayBounds = CurrentDisplay.Bounds;

            int windowX = Position.X - displayBounds.X;
            int windowY = Position.Y - displayBounds.Y;

            var windowSize = sizeWindowed.Value;

            windowPositionX.Value = displayBounds.Width > windowSize.Width ? (float)windowX / (displayBounds.Width - windowSize.Width) : 0;
            windowPositionY.Value = displayBounds.Height > windowSize.Height ? (float)windowY / (displayBounds.Height - windowSize.Height) : 0;
        }

        /// <summary>
        /// Set to <c>true</c> while the window size is being stored to config to avoid bindable feedback.
        /// </summary>
        private bool storingSizeToConfig;

        private void storeWindowSizeToConfig()
        {
            if (WindowState != WindowState.Normal)
                return;

            storingSizeToConfig = true;
            sizeWindowed.Value = (Size / Scale).ToSize();
            storingSizeToConfig = false;
        }

        /// <summary>
        /// Prepare display of a borderless window.
        /// </summary>
        /// <returns>
        /// The size of the borderless window's draw area.
        /// </returns>
        protected virtual Size SetBorderless()
        {
            // this is a generally sane method of handling borderless, and works well on macOS and linux.
            sdl.SetWindowFullscreen(WindowPtr, (uint)WindowFlags.WindowFullscreenDesktop);

            return currentDisplay.Bounds.Size;
        }

        private MouseButton mouseButtonFromEvent(byte button)
        {
            switch (button)
            {
                default:
                case 1:
                    return MouseButton.Left;

                case 2:
                    return MouseButton.Middle;

                case 3:
                    return MouseButton.Right;

                case 4:
                    return MouseButton.Button4;

                case 5:
                    return MouseButton.Button5;
            }
        }

        #endregion

        protected virtual IGraphicsBackend CreateGraphicsBackend()
        {
            return new SDL2GraphicsBackend();
        }

        public void SetupWindow(FrameworkConfigManager config)
        {
            CurrentDisplayBindable.ValueChanged += evt =>
            {
                windowDisplayIndexBindable.Value = evt.NewValue;
            };

            config.BindWith(FrameworkSetting.LastDisplayDevice, windowDisplayIndexBindable);
            windowDisplayIndexBindable.BindValueChanged(evt => CurrentDisplay = Displays.ElementAtOrDefault(evt.NewValue.Index) ?? PrimaryDisplay, true);

            sizeFullscreen.ValueChanged += evt =>
            {
                if (storingSizeToConfig) return;
                if (windowState != WindowState.Fullscreen) return;

                pendingWindowState = windowState;
            };

            sizeWindowed.ValueChanged += evt =>
            {
                if (storingSizeToConfig) return;
                if (windowState != WindowState.Normal) return;

                pendingWindowState = windowState;
            };

            config.BindWith(FrameworkSetting.SizeFullscreen, sizeFullscreen);
            config.BindWith(FrameworkSetting.WindowedSize, sizeWindowed);

            config.BindWith(FrameworkSetting.WindowedPositionX, windowPositionX);
            config.BindWith(FrameworkSetting.WindowedPositionY, windowPositionY);

            config.BindWith(FrameworkSetting.WindowMode, WindowMode);
            config.BindWith(FrameworkSetting.ConfineMouseMode, ConfineMouseMode);

            WindowMode.BindValueChanged(evt =>
            {
                switch (evt.NewValue)
                {
                    case Configuration.WindowMode.Fullscreen:
                        WindowState = WindowState.Fullscreen;
                        break;

                    case Configuration.WindowMode.Borderless:
                        WindowState = WindowState.FullscreenBorderless;
                        break;

                    case Configuration.WindowMode.Windowed:
                        WindowState = windowMaximised ? WindowState.Maximised : WindowState.Normal;
                        break;
                }

                updateConfineMode();
            });

            ConfineMouseMode.BindValueChanged(_ => updateConfineMode());
        }

        public void CycleMode()
        {
            var currentValue = WindowMode.Value;

            do
            {
                switch (currentValue)
                {
                    case Configuration.WindowMode.Windowed:
                        currentValue = Configuration.WindowMode.Borderless;
                        break;

                    case Configuration.WindowMode.Borderless:
                        currentValue = Configuration.WindowMode.Fullscreen;
                        break;

                    case Configuration.WindowMode.Fullscreen:
                        currentValue = Configuration.WindowMode.Windowed;
                        break;
                }
            } while (!SupportedWindowModes.Contains(currentValue) && currentValue != WindowMode.Value);

            WindowMode.Value = currentValue;
        }

        /// <summary>
        /// Update the host window manager's cursor position based on a location relative to window coordinates.
        /// </summary>
        /// <param name="position">A position inside the window.</param>
        public void UpdateMousePosition(Vector2 position) => ScheduleCommand(() =>
            sdl.WarpMouseInWindow(WindowPtr, (int)(position.X / Scale), (int)(position.Y / Scale)));

        public void SetIconFromStream(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                ms.Position = 0;

                var imageInfo = Image.Identify(ms);

                if (imageInfo != null)
                    SetIconFromImage(Image.Load<Rgba32>(ms.GetBuffer()));
                else if (IconGroup.TryParse(ms.GetBuffer(), out var iconGroup))
                    SetIconFromGroup(iconGroup);
            }
        }

        internal virtual void SetIconFromGroup(IconGroup iconGroup)
        {
            // LoadRawIcon returns raw PNG data if available, which avoids any Windows-specific pinvokes
            byte[] bytes = iconGroup.LoadRawIcon(default_icon_size, default_icon_size);
            if (bytes == null)
                return;

            SetIconFromImage(Image.Load<Rgba32>(bytes));
        }

        internal virtual void SetIconFromImage(Image<Rgba32> iconImage) => setSDLIcon(iconImage);

        private void updateConfineMode()
        {
            bool confine = false;

            switch (ConfineMouseMode.Value)
            {
                case Input.ConfineMouseMode.Fullscreen:
                    confine = WindowMode.Value != Configuration.WindowMode.Windowed;
                    break;

                case Input.ConfineMouseMode.Always:
                    confine = true;
                    break;
            }

            if (confine)
                CursorStateBindable.Value |= CursorMode.Raw;
            else
                CursorStateBindable.Value &= ~CursorMode.Raw;
        }

        #region Helper functions

        private Silk.NET.SDL.DisplayMode getClosestDisplayMode(Size size, int refreshRate, int displayIndex)
        {
            var targetMode = new Silk.NET.SDL.DisplayMode { W = size.Width, H = size.Height, RefreshRate = refreshRate };

            Silk.NET.SDL.DisplayMode mode;
            if (sdl.GetClosestDisplayMode(displayIndex, ref targetMode, &mode) != null)
                return mode;

            // fallback to current display's native bounds
            targetMode.W = currentDisplay.Bounds.Width;
            targetMode.H = currentDisplay.Bounds.Height;
            targetMode.RefreshRate = 0;

            if (sdl.GetClosestDisplayMode(displayIndex, ref targetMode, ref mode) != null)
                return mode;

            // finally return the current mode if everything else fails.
            // not sure this is required.
            if (sdl.GetWindowDisplayMode(WindowPtr, ref mode) >= 0)
                return mode;

            throw new InvalidOperationException("couldn't retrieve valid display mode");
        }

        private Display displayFromSDL(int displayIndex)
        {
            var displayModes = Enumerable.Range(0, sdl.GetNumDisplayModes(displayIndex))
                                         .Select(modeIndex =>
                                         {
                                             Silk.NET.SDL.DisplayMode mode = new Silk.NET.SDL.DisplayMode();
                                             sdl.GetDisplayMode(displayIndex, modeIndex, &mode);
                                             return displayModeFromSDL(mode, displayIndex, modeIndex);
                                         })
                                         .ToArray();

            Rectangle<int> rect;
            sdl.GetDisplayBounds(displayIndex, &rect);
            return new Display(displayIndex, sdl.GetDisplayNameS(displayIndex), new Rectangle(rect.Origin.X, rect.Origin.Y, rect.Size.X, rect.Size.Y), displayModes);
        }

        private DisplayMode displayModeFromSDL(Silk.NET.SDL.DisplayMode mode, int displayIndex, int modeIndex)
        {
            int bpp;
            uint tmp1;
            uint tmp2;
            uint tmp3;
            uint tmp4;
            sdl.PixelFormatEnumToMasks(mode.Format, &bpp, &tmp1, &tmp2, &tmp3, &tmp4);
            return new DisplayMode(sdl.GetPixelFormatNameS(mode.Format), new Size(mode.W, mode.H), bpp, mode.RefreshRate, modeIndex, displayIndex);
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked once every window event loop.
        /// </summary>
        public event Action Update;

        /// <summary>
        /// Invoked after the window has resized.
        /// </summary>
        public event Action Resized;

        /// <summary>
        /// Invoked after the window's state has changed.
        /// </summary>
        public event Action<WindowState> WindowStateChanged;

        /// <summary>
        /// Invoked when the user attempts to close the window. Return value of true will cancel exit.
        /// </summary>
        public event Func<bool> ExitRequested;

        /// <summary>
        /// Invoked when the window is about to close.
        /// </summary>
        public event Action Exited;

        /// <summary>
        /// Invoked when the mouse cursor enters the window.
        /// </summary>
        public event Action MouseEntered;

        /// <summary>
        /// Invoked when the mouse cursor leaves the window.
        /// </summary>
        public event Action MouseLeft;

        /// <summary>
        /// Invoked when the window moves.
        /// </summary>
        public event Action<Point> Moved;

        /// <summary>
        /// Invoked when the user scrolls the mouse wheel over the window.
        /// </summary>
        public event Action<Vector2, bool> MouseWheel;

        protected void TriggerMouseWheel(Vector2 delta, bool precise) => MouseWheel?.Invoke(delta, precise);

        /// <summary>
        /// Invoked when the user moves the mouse cursor within the window.
        /// </summary>
        public event Action<Vector2> MouseMove;

        /// <summary>
        /// Invoked when the user moves the mouse cursor within the window (via relative / raw input).
        /// </summary>
        public event Action<Vector2> MouseMoveRelative;

        /// <summary>
        /// Invoked when the user presses a mouse button.
        /// </summary>
        public event Action<MouseButton> MouseDown;

        /// <summary>
        /// Invoked when the user releases a mouse button.
        /// </summary>
        public event Action<MouseButton> MouseUp;

        /// <summary>
        /// Invoked when the user presses a key.
        /// </summary>
        public event Action<Key> KeyDown;

        /// <summary>
        /// Invoked when the user releases a key.
        /// </summary>
        public event Action<Key> KeyUp;

        /// <summary>
        /// Invoked when the user types a character.
        /// </summary>
        public event Action<char> KeyTyped;

        /// <inheritdoc cref="IWindow.KeymapChanged"/>
        public event Action KeymapChanged;

        /// <summary>
        /// Invoked when a joystick axis changes.
        /// </summary>
        public event Action<JoystickAxis> JoystickAxisChanged;

        /// <summary>
        /// Invoked when the user presses a button on a joystick.
        /// </summary>
        public event Action<JoystickButton> JoystickButtonDown;

        /// <summary>
        /// Invoked when the user releases a button on a joystick.
        /// </summary>
        public event Action<JoystickButton> JoystickButtonUp;

        /// <summary>
        /// Invoked when the user drops a file into the window.
        /// </summary>
        public event Action<string> DragDrop;

        #endregion

        public void Dispose()
        {
        }
    }
}
