// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.OpenGL.Vertices;
using Silk.NET.OpenGL;

namespace osu.Framework.Graphics.OpenGL.Buffers
{
    internal static class LinearIndexData
    {
        static LinearIndexData()
        {
            GLWrapper.GL.GenBuffers(1, out EBO_ID);
        }

        public static readonly uint EBO_ID;
        public static int MaxAmountIndices;
    }

    /// <summary>
    /// This type of vertex buffer lets the ith vertex be referenced by the ith index.
    /// </summary>
    public class LinearVertexBuffer<T> : VertexBuffer<T>
        where T : struct, IEquatable<T>, IVertex
    {
        private readonly int amountVertices;

        internal LinearVertexBuffer(int amountVertices, PrimitiveType type, BufferUsageARB usage)
            : base(amountVertices, usage)
        {
            this.amountVertices = amountVertices;
            Type = type;
        }

        protected override void Initialise()
        {
            base.Initialise();

            if (amountVertices > LinearIndexData.MaxAmountIndices)
            {
                short[] indices = new short[amountVertices];

                for (short i = 0; i < amountVertices; i++)
                    indices[i] = i;

                GLWrapper.BindBuffer(BufferTargetARB.ElementArrayBuffer, LinearIndexData.EBO_ID);
                GLWrapper.GL.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(amountVertices * sizeof(short)), indices.AsSpan().GetPinnableReference(), BufferUsageARB.StaticDraw);

                LinearIndexData.MaxAmountIndices = amountVertices;
            }
        }

        public override void Bind(bool forRendering)
        {
            base.Bind(forRendering);

            if (forRendering)
                GLWrapper.BindBuffer(BufferTargetARB.ElementArrayBuffer, LinearIndexData.EBO_ID);
        }

        protected override PrimitiveType Type { get; }
    }
}
