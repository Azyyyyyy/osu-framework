// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Numerics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Threading;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using static osu.Framework.Threading.ScheduledDelegate;

namespace osu.Framework.Graphics.Shaders
{
    public class Shader : IShader, IDisposable
    {
        private readonly string name;
        private readonly List<ShaderPart> parts;

        private readonly ScheduledDelegate shaderCompileDelegate;

        internal readonly Dictionary<string, IUniform> Uniforms = new Dictionary<string, IUniform>();

        /// <summary>
        /// Holds all the <see cref="Uniforms"/> values for faster access than iterating on <see cref="Dictionary{TKey,TValue}.Values"/>.
        /// </summary>
        private IUniform[] uniformsValues;

        public bool IsLoaded { get; private set; }

        internal bool IsBound { get; private set; }

        private uint programID = 0;

        internal Shader(string name, List<ShaderPart> parts)
        {
            this.name = name;
            this.parts = parts;

            GLWrapper.ScheduleExpensiveOperation(shaderCompileDelegate = new ScheduledDelegate(compile));
        }

        private void compile()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString(), "Can not compile a disposed shader.");

            if (IsLoaded)
                throw new InvalidOperationException("Attempting to compile an already-compiled shader.");

            parts.RemoveAll(p => p == null);
            if (parts.Count == 0)
                return;

            programID = CreateProgram();

            if (!CompileInternal())
                throw new ProgramLinkingFailedException(name, GetProgramLog());

            IsLoaded = true;

            SetupUniforms();

            GlobalPropertyManager.Register(this);
        }

        internal void EnsureShaderCompiled()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString(), "Can not compile a disposed shader.");

            if (shaderCompileDelegate.State == RunState.Waiting)
                shaderCompileDelegate.RunTask();
        }

        public void Bind()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString(), "Can not bind a disposed shader.");

            if (IsBound)
                return;

            EnsureShaderCompiled();

            GLWrapper.UseProgram(this);

            foreach (var uniform in uniformsValues)
                uniform?.Update();

            IsBound = true;
        }

        public void Unbind()
        {
            if (!IsBound)
                return;

            GLWrapper.UseProgram(null);

            IsBound = false;
        }

        /// <summary>
        /// Returns a uniform from the shader.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <returns>Returns a base uniform.</returns>
        public Uniform<T> GetUniform<T>(string name)
            where T : struct, IEquatable<T>
        {
            if (IsDisposed)
                throw new ObjectDisposedException(ToString(), "Can not retrieve uniforms from a disposed shader.");

            EnsureShaderCompiled();

            return (Uniform<T>)Uniforms[name];
        }

        private protected virtual bool CompileInternal()
        {
            foreach (ShaderPart p in parts)
            {
                if (!p.Compiled) p.Compile();
                GLWrapper.GL.AttachShader(this, p);

                foreach (ShaderInputInfo input in p.ShaderInputs)
                    GLWrapper.GL.BindAttribLocation(this, (uint)input.Location, input.Name);
            }

            GLWrapper.GL.LinkProgram(this);
            GLWrapper.GL.GetProgram(this, GLEnum.LinkStatus, out int linkResult);

            foreach (var part in parts)
                GLWrapper.GL.DetachShader(this, part);

            return linkResult == 1;
        }

        private protected virtual void SetupUniforms()
        {
            GLWrapper.GL.GetProgram(this, GLEnum.ActiveUniforms, out int uniformCount);

            uniformsValues = new IUniform[uniformCount];

            for (int i = 0; i < uniformCount; i++)
            {
                GLWrapper.GL.GetActiveUniform(this, (uint)i, 100, out _, out _, out UniformType type, out string uniformName);

                IUniform uniform;

                switch (type)
                {
                    case UniformType.Bool:
                        uniform = createUniform<bool>(uniformName);
                        break;

                    case UniformType.Float:
                        uniform = createUniform<float>(uniformName);
                        break;

                    case UniformType.Int:
                        uniform = createUniform<int>(uniformName);
                        break;

                    case UniformType.FloatMat3:
                        uniform = createUniform<Matrix3X3<float>>(uniformName);
                        break;

                    case UniformType.FloatMat4:
                        uniform = createUniform<Matrix4X4<float>>(uniformName);
                        break;

                    case UniformType.FloatVec2:
                        uniform = createUniform<Vector2>(uniformName);
                        break;

                    case UniformType.FloatVec3:
                        uniform = createUniform<Vector3>(uniformName);
                        break;

                    case UniformType.FloatVec4:
                        uniform = createUniform<Vector4>(uniformName);
                        break;

                    case UniformType.Sampler2D:
                        uniform = createUniform<int>(uniformName);
                        break;

                    default:
                        continue;
                }

                Uniforms.Add(uniformName, uniform);
                uniformsValues[i] = uniform;
            }

            IUniform createUniform<T>(string name)
                where T : struct, IEquatable<T>
            {
                int location = GLWrapper.GL.GetUniformLocation(this, name);

                if (GlobalPropertyManager.CheckGlobalExists(name)) return new GlobalUniform<T>(this, name, location);

                return new Uniform<T>(this, name, location);
            }
        }

        private protected virtual string GetProgramLog() => GLWrapper.GL.GetProgramInfoLog(this);

        private protected virtual uint CreateProgram() => GLWrapper.GL.CreateProgram();

        private protected virtual void DeleteProgram(uint id) => GLWrapper.GL.DeleteProgram(id);

        public override string ToString() => $@"{name} Shader (Compiled: {programID != 0})";

        public static implicit operator uint(Shader shader) => shader.programID;

        #region IDisposable Support

        protected internal bool IsDisposed { get; private set; }

        ~Shader()
        {
            GLWrapper.ScheduleDisposal(() => Dispose(false));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                shaderCompileDelegate?.Cancel();

                GlobalPropertyManager.Unregister(this);

                if (programID != 0)
                    DeleteProgram(this);
            }
        }

        #endregion

        public class PartCompilationFailedException : Exception
        {
            public PartCompilationFailedException(string partName, string log)
                : base($"A {typeof(ShaderPart)} failed to compile: {partName}:\n{log.Trim()}")
            {
            }
        }

        public class ProgramLinkingFailedException : Exception
        {
            public ProgramLinkingFailedException(string programName, string log)
                : base($"A {typeof(Shader)} failed to link: {programName}:\n{log.Trim()}")
            {
            }
        }
    }
}
