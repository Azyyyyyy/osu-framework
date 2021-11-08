// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Logging;
using Silk.NET.OpenGL;

namespace osu.Framework.Platform
{
    /// <summary>
    /// Implementation of <see cref="IGraphicsBackend"/> that force-loads OpenGL
    /// endpoints into Silk.NET's bindings.
    /// </summary>
    public abstract class PassthroughGraphicsBackend : IGraphicsBackend
    {
        internal IntPtr Context;

        internal Version GLVersion { get; private set; }

        internal Version GLSLVersion { get; private set; }

        internal bool IsEmbedded { get; private set; }

        public abstract bool VerticalSync { get; set; }

        protected abstract IntPtr CreateContext();
        protected abstract void MakeCurrent(IntPtr context);
        protected abstract IntPtr GetProcAddress(string symbol);

        public abstract void SwapBuffers();

        public virtual void Initialise(IWindow window)
        {
            Context = CreateContext();

            MakeCurrent(Context);

            string version = GLWrapper.GL.GetStringS(StringName.Version);
            string versionNumberSubstring = getVersionNumberSubstring(version);

            GLVersion = new Version(versionNumberSubstring);

            // As defined by https://www.khronos.org/registry/OpenGL-Refpages/es2.0/xhtml/glGetString.xml
            IsEmbedded = version.Contains("OpenGL ES");
            GLWrapper.IsEmbedded = IsEmbedded;

            version = GLWrapper.GL.GetStringS(StringName.ShadingLanguageVersion);

            if (!string.IsNullOrEmpty(version))
            {
                try
                {
                    GLSLVersion = new Version(versionNumberSubstring);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"couldn't set GLSL version using string '{version}'");
                }
            }

            if (GLSLVersion == null)
                GLSLVersion = new Version();

            Logger.Log($@"GL Initialized
                        GL Version:                 {GLWrapper.GL.GetStringS(StringName.Version)}
                        GL Renderer:                {GLWrapper.GL.GetStringS(StringName.Renderer)}
                        GL Shader Language version: {GLWrapper.GL.GetStringS(StringName.ShadingLanguageVersion)}
                        GL Vendor:                  {GLWrapper.GL.GetStringS(StringName.Vendor)}
                        GL Extensions:              {GLWrapper.GL.GetStringS(StringName.Extensions)}");

            // We need to release the context in this thread, since Windows locks it and prevents
            // the draw thread from taking it. macOS seems to gracefully ignore this.
            MakeCurrent(IntPtr.Zero);
        }

        public void MakeCurrent() => MakeCurrent(Context);

        public void ClearCurrent() => MakeCurrent(IntPtr.Zero);

        private string getVersionNumberSubstring(string version)
        {
            string result = version.Split(' ').FirstOrDefault(s => char.IsDigit(s, 0));
            if (result != null) return result;

            throw new ArgumentException($"Invalid version string: \"{version}\"", nameof(version));
        }
    }
}
