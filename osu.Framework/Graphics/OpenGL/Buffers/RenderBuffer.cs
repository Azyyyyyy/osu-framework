// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Numerics;
using osu.Framework.Platform;
using Silk.NET.OpenGL;

namespace osu.Framework.Graphics.OpenGL.Buffers
{
    internal class RenderBuffer : IDisposable
    {
        private readonly InternalFormat format;
        private readonly uint renderBuffer;
        private readonly int sizePerPixel;

        private FramebufferAttachment attachment;

        public RenderBuffer(InternalFormat format)
        {
            this.format = format;

            renderBuffer = GLWrapper.GL.GenRenderbuffer();

            GLWrapper.GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);

            // OpenGL docs don't specify that this is required, but seems to be required on some platforms
            // to correctly attach in the GL.FramebufferRenderbuffer() call below
            GLWrapper.GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, format, 1, 1);

            attachment = format.GetAttachmentType();
            sizePerPixel = format.GetBytesPerPixel();

            GLWrapper.GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachment, RenderbufferTarget.Renderbuffer, renderBuffer);
        }

        private Vector2 internalSize;
        private NativeMemoryTracker.NativeMemoryLease memoryLease;

        public void Bind(Vector2 size)
        {
            size = Vector2.Clamp(size, Vector2.One, new Vector2(GLWrapper.MaxRenderBufferSize));

            // See: https://www.khronos.org/registry/OpenGL/extensions/EXT/EXT_multisampled_render_to_texture.txt
            //    + https://developer.apple.com/library/archive/documentation/3DDrawing/Conceptual/OpenGLES_ProgrammingGuide/WorkingwithEAGLContexts/WorkingwithEAGLContexts.html
            // OpenGL ES allows the driver to discard renderbuffer contents after they are presented to the screen, so the storage must always be re-initialised for embedded devices.
            // Such discard does not exist on non-embedded platforms, so they are only re-initialised when required.
            if (GLWrapper.IsEmbedded || internalSize.X < size.X || internalSize.Y < size.Y)
            {
                GLWrapper.GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
                GLWrapper.GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, format, (uint)Math.Ceiling(size.X), (uint)Math.Ceiling(size.Y));

                if (!GLWrapper.IsEmbedded)
                {
                    memoryLease?.Dispose();
                    memoryLease = NativeMemoryTracker.AddMemory(this, (long)(size.X * size.Y * sizePerPixel));
                }

                internalSize = size;
            }
        }

        public void Unbind()
        {
            if (GLWrapper.IsEmbedded)
            {
                // Renderbuffers are not automatically discarded on all embedded devices, so invalidation is forced for extra performance and to unify logic between devices.
                GLWrapper.GL.InvalidateFramebuffer(GLEnum.Framebuffer, 1, (GLEnum)attachment);
            }
        }

        #region Disposal

        ~RenderBuffer()
        {
            GLWrapper.ScheduleDisposal(() => Dispose(false));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (renderBuffer != 0)
            {
                memoryLease?.Dispose();
                GLWrapper.GL.DeleteRenderbuffer(renderBuffer);
            }

            isDisposed = true;
        }

        #endregion
    }
}
