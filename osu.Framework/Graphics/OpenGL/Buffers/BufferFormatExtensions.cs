// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Silk.NET.OpenGL;

namespace osu.Framework.Graphics.OpenGL.Buffers
{
    public static class BufferFormatExtensions
    {
        public static FramebufferAttachment GetAttachmentType(this InternalFormat format)
        {
            switch (format)
            {
                case InternalFormat.R8:
                case InternalFormat.R8SNorm:
                case InternalFormat.R16f:
                case InternalFormat.R32f:
                case InternalFormat.R8ui:
                case InternalFormat.R8i:
                case InternalFormat.R16ui:
                case InternalFormat.R16i:
                case InternalFormat.R32ui:
                case InternalFormat.R32i:
                case InternalFormat.RG8:
                case InternalFormat.RG8SNorm:
                case InternalFormat.RG16f:
                case InternalFormat.RG32f:
                case InternalFormat.RG8ui:
                case InternalFormat.RG8i:
                case InternalFormat.RG16ui:
                case InternalFormat.RG16i:
                case InternalFormat.RG32ui:
                case InternalFormat.RG32i:
                case InternalFormat.Rgb8:
                case InternalFormat.Srgb8:
                case InternalFormat.Rgb565:
                case InternalFormat.Rgb8SNorm:
                case InternalFormat.R11fG11fB10f:
                case InternalFormat.Rgb9E5:
                case InternalFormat.Rgb16f:
                case InternalFormat.Rgb32f:
                case InternalFormat.Rgb8ui:
                case InternalFormat.Rgb8i:
                case InternalFormat.Rgb16ui:
                case InternalFormat.Rgb16i:
                case InternalFormat.Rgb32ui:
                case InternalFormat.Rgb32i:
                case InternalFormat.Rgba8:
                case InternalFormat.Srgb8Alpha8:
                case InternalFormat.Rgba8SNorm:
                case InternalFormat.Rgb5A1:
                case InternalFormat.Rgba4:
                case InternalFormat.Rgb10A2:
                case InternalFormat.Rgba16f:
                case InternalFormat.Rgba32f:
                case InternalFormat.Rgba8i:
                case InternalFormat.Rgba8ui:
                case InternalFormat.Rgb10A2ui:
                case InternalFormat.Rgba16i:
                case InternalFormat.Rgba16ui:
                case InternalFormat.Rgba32i:
                case InternalFormat.Rgba32ui:
                    return FramebufferAttachment.ColorAttachment0;

                case InternalFormat.DepthComponent16:
                case InternalFormat.DepthComponent24:
                case InternalFormat.DepthComponent32f:
                    return FramebufferAttachment.DepthAttachment;

                case InternalFormat.StencilIndex8:
                    return FramebufferAttachment.StencilAttachment;

                //TODO: Find out if this is missing in Silk.NET or if I'm doing a dumb
                //case RenderbufferInternalFormat.Depth24Stencil8:
                //case RenderbufferInternalFormat.Depth32fStencil8:
                //    return FramebufferAttachment.DepthStencilAttachment;

                default:
                    throw new InvalidOperationException($"{format} is not a valid {nameof(InternalFormat)} type.");
            }
        }

        public static int GetBytesPerPixel(this InternalFormat format)
        {
            // cross-reference: https://www.khronos.org/registry/OpenGL-Refpages/es3.0/html/glTexImage2D.xhtml
            switch (format)
            {
                // GL_RED
                case InternalFormat.R8:
                case InternalFormat.R8SNorm:
                    return 1;

                case InternalFormat.R16f:
                    return 2;

                case InternalFormat.R32f:
                    return 4;

                // GL_RED_INTEGER
                case InternalFormat.R8ui:
                case InternalFormat.R8i:
                    return 1;

                case InternalFormat.R16ui:
                case InternalFormat.R16i:
                    return 2;

                case InternalFormat.R32ui:
                case InternalFormat.R32i:
                    return 4;

                // GL_RG
                case InternalFormat.RG8:
                case InternalFormat.RG8SNorm:
                    return 2;

                case InternalFormat.RG16f:
                    return 4;

                case InternalFormat.RG32f:
                    return 8;

                // GL_RG_INTEGER
                case InternalFormat.RG8ui:
                case InternalFormat.RG8i:
                    return 2;

                case InternalFormat.RG16ui:
                case InternalFormat.RG16i:
                    return 4;

                case InternalFormat.RG32ui:
                case InternalFormat.RG32i:
                    return 8;

                // GL_RGB
                case InternalFormat.Rgb8:
                case InternalFormat.Srgb8:
                    return 3;

                case InternalFormat.Rgb565:
                    return 2;

                case InternalFormat.Rgb8SNorm:
                    return 3;

                case InternalFormat.R11fG11fB10f:
                case InternalFormat.Rgb9E5:
                    return 4;

                case InternalFormat.Rgb16f:
                    return 6;

                case InternalFormat.Rgb32f:
                    return 12;

                // GL_RGB_INTEGER
                case InternalFormat.Rgb8ui:
                case InternalFormat.Rgb8i:
                    return 3;

                case InternalFormat.Rgb16ui:
                case InternalFormat.Rgb16i:
                    return 6;

                case InternalFormat.Rgb32ui:
                case InternalFormat.Rgb32i:
                    return 12;

                // GL_RGBA
                case InternalFormat.Rgba8:
                case InternalFormat.Srgb8Alpha8:
                case InternalFormat.Rgba8SNorm:
                    return 4;

                case InternalFormat.Rgb5A1:
                case InternalFormat.Rgba4:
                    return 2;

                case InternalFormat.Rgb10A2:
                    return 4;

                case InternalFormat.Rgba16f:
                    return 8;

                case InternalFormat.Rgba32f:
                    return 16;

                // GL_RGBA_INTEGER
                case InternalFormat.Rgba8i:
                case InternalFormat.Rgba8ui:
                case InternalFormat.Rgb10A2ui:
                    return 4;

                case InternalFormat.Rgba16i:
                case InternalFormat.Rgba16ui:
                    return 8;

                case InternalFormat.Rgba32i:
                case InternalFormat.Rgba32ui:
                    return 16;

                // GL_DEPTH_COMPONENT
                case InternalFormat.DepthComponent16:
                    return 2;

                case InternalFormat.DepthComponent24:
                    return 3;

                case InternalFormat.DepthComponent32f:
                    return 4;

                // GL_DEPTH_STENCIL
                case InternalFormat.Depth24Stencil8:
                    return 4;

                case InternalFormat.Depth32fStencil8:
                    return 5;

                case InternalFormat.StencilIndex8:
                    return 1;

                default:
                    throw new InvalidOperationException($"{format} is not a valid {nameof(InternalFormat)} type.");
            }
        }
    }
}
