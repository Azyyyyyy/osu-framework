﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using Silk.NET.OpenGL;

namespace osu.Framework.Graphics.Video
{
    internal unsafe class VideoTexture : TextureGLSingle
    {
        private uint[] textureIds;

        /// <summary>
        /// Whether the latest frame data has been uploaded.
        /// </summary>
        public bool UploadComplete { get; private set; }

        public VideoTexture(int width, int height, WrapMode wrapModeS = WrapMode.None, WrapMode wrapModeT = WrapMode.None)
            : base(width, height, true, GLEnum.Linear, wrapModeS, wrapModeT)
        {
        }

        private NativeMemoryTracker.NativeMemoryLease memoryLease;

        internal override void SetData(ITextureUpload upload, WrapMode wrapModeS, WrapMode wrapModeT, Opacity? uploadOpacity)
        {
            if (uploadOpacity != null && uploadOpacity != Opacity.Opaque)
                throw new InvalidOperationException("Video texture uploads must always be opaque");

            UploadComplete = false;

            // We do not support videos with transparency at this point,
            // so the upload's opacity as well as the texture's opacity
            // is always opaque.
            base.SetData(upload, wrapModeS, wrapModeT, Opacity = Opacity.Opaque);
        }

        public override uint TextureId => textureIds?[0] ?? 0;

        private int textureSize;

        public override int GetByteSize() => textureSize;

        internal override bool Bind(TextureUnit unit, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            if (!Available)
                throw new ObjectDisposedException(ToString(), "Can not bind a disposed texture.");

            Upload();

            if (textureIds == null)
                return false;

            bool anyBound = false;

            for (int i = 0; i < textureIds.Length; i++)
                anyBound |= GLWrapper.BindTexture(textureIds[i], unit + i, wrapModeS, wrapModeT);

            if (anyBound)
                BindCount++;

            return true;
        }

        protected override void DoUpload(ITextureUpload upload, void* dataPointer)
        {
            if (!(upload is VideoTextureUpload videoUpload))
                return;

            // Do we need to generate a new texture?
            if (textureIds == null)
            {
                Debug.Assert(memoryLease == null);
                memoryLease = NativeMemoryTracker.AddMemory(this, Width * Height * 3 / 2);

                textureIds = new uint[3];
                GLWrapper.GL.GenTextures((uint)textureIds.Length, textureIds);

                for (int i = 0; i < textureIds.Length; i++)
                {
                    GLWrapper.BindTexture(textureIds[i]);

                    if (i == 0)
                    {
                        int width = videoUpload.Frame->width;
                        int height = videoUpload.Frame->height;

                        GLWrapper.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, (uint)width, (uint)height, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);

                        textureSize += width * height;
                    }
                    else
                    {
                        int width = (videoUpload.Frame->width + 1) / 2;
                        int height = (videoUpload.Frame->height + 1) / 2;

                        GLWrapper.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, (uint)width, (uint)height, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);

                        textureSize += width * height;
                    }

                    GLWrapper.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
                    GLWrapper.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

                    GLWrapper.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GLWrapper.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                }
            }

            for (int i = 0; i < textureIds.Length; i++)
            {
                GLWrapper.BindTexture(textureIds[i]);

                GLWrapper.GL.PixelStore(PixelStoreParameter.UnpackRowLength, videoUpload.Frame->linesize[(uint)i]);
                GLWrapper.GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)(videoUpload.Frame->width / (i > 0 ? 2 : 1)), (uint)(videoUpload.Frame->height / (i > 0 ? 2 : 1)),
                    PixelFormat.Red, PixelType.UnsignedByte, (IntPtr)videoUpload.Frame->data[(uint)i]);
            }

            GLWrapper.GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);

            UploadComplete = true;
        }

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            memoryLease?.Dispose();

            GLWrapper.ScheduleDisposal(unload);
        }

        private void unload()
        {
            if (textureIds == null)
                return;

            for (int i = 0; i < textureIds.Length; i++)
            {
                if (textureIds[i] > 0)
                    GLWrapper.GL.DeleteTextures(1, new[] { textureIds[i] });
            }
        }

        #endregion
    }
}
