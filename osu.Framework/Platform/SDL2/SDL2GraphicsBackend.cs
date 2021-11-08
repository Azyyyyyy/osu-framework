// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.OpenGL;
using Silk.NET.OpenGL;
using Silk.NET.SDL;

namespace osu.Framework.Platform.SDL2
{
    /// <summary>
    /// Implementation of <see cref="PassthroughGraphicsBackend"/> that uses SDL's OpenGL bindings.
    /// </summary>
    public unsafe class SDL2GraphicsBackend : PassthroughGraphicsBackend
    {
        private readonly Sdl sdl = SdlProvider.SDL.Value;
        private Window* window;

        public override bool VerticalSync
        {
            get => sdl.GLGetSwapInterval() != 0;
            set => sdl.GLSetSwapInterval(value ? 1 : 0);
        }

        public void SetWindow(Window* window)
        {
            this.window = window;
        }

        protected override IntPtr CreateContext()
        {
            sdl.GLSetAttribute(GLattr.GLContextProfileMask, (int)GLprofile.GLContextProfileCompatibility);

            IntPtr context = (IntPtr)sdl.GLCreateContext(window);
            var gl = GL.GetApi(GetProcAddress);
            GLWrapper.SetGL(gl);

            if (context == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to create an SDL2 GL context ({sdl.GetErrorS()})");

            return context;
        }

        protected override void MakeCurrent(IntPtr context)
        {
            int result = sdl.GLMakeCurrent(window, (void*)context);
            if (result < 0)
                throw new InvalidOperationException($"Failed to acquire GL context ({sdl.GetErrorS()})");
        }

        public override void SwapBuffers() => sdl.GLSwapWindow(window);

        protected override IntPtr GetProcAddress(string symbol) => (IntPtr)sdl.GLGetProcAddress(symbol);

        public override void Initialise(IWindow window)
        {
            if (window is not SDL2DesktopWindow)
                throw new ArgumentException("Unsupported window backend.", nameof(window));

            base.Initialise(window);
        }
    }
}
