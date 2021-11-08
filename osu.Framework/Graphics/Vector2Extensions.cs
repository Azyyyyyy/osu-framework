// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using osu.Framework.Graphics.Primitives;
using Silk.NET.Maths;

namespace osu.Framework.Graphics
{
    public static class Vector2Extensions
    {
        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        public static Vector2 Transform(Vector2 position, Matrix3X2<float> matrix)
        {
            return new Vector2(
                position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M31,
                position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M32);
        }

        /// <summary>
        /// Returns a vector created from the largest of the corresponding components of the given vectors.
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise maximum</returns>
        public static Vector2 ComponentMax(Vector2 a, Vector2 b)
        {
            a.X = a.X > b.X ? a.X : b.X;
            a.Y = a.Y > b.Y ? a.Y : b.Y;
            return a;
        }

        /// <summary>
        /// Returns a vector created from the largest of the corresponding components of the given vectors.
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise maximum</param>
        public static void ComponentMax(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.X = a.X > b.X ? a.X : b.X;
            result.Y = a.Y > b.Y ? a.Y : b.Y;
        }

        /// <summary>Transform a Position by the given Matrix</summary>
        /// <param name="pos">The position to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed position</returns>
        public static Vector2 Transform(Vector2 pos, Matrix3X3<float> mat)
        {
            Transform(ref pos, ref mat, out Vector2 result);
            return result;
        }

        /// <summary>Transform a Position by the given Matrix</summary>
        /// <param name="pos">The position to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void Transform(ref Vector2 pos, ref Matrix3X3<float> mat, out Vector2 result)
        {
            result.X = mat.Row1.X * pos.X + mat.Row2.X * pos.Y + mat.Row3.X;
            result.Y = mat.Row1.Y * pos.X + mat.Row2.Y * pos.Y + mat.Row3.Y;
        }

        /// <summary>
        /// Compute the euclidean distance between two vectors.
        /// </summary>
        /// <param name="vec1">The first vector</param>
        /// <param name="vec2">The second vector</param>
        /// <returns>The distance</returns>
        public static float Distance(Vector2 vec1, Vector2 vec2)
        {
            Distance(ref vec1, ref vec2, out float result);
            return result;
        }

        /// <summary>
        /// Compute the euclidean distance between two vectors.
        /// </summary>
        /// <param name="vec1">The first vector</param>
        /// <param name="vec2">The second vector</param>
        /// <param name="result">The distance</param>
        public static void Distance(ref Vector2 vec1, ref Vector2 vec2, out float result)
        {
            result = MathF.Sqrt((vec2.X - vec1.X) * (vec2.X - vec1.X) + (vec2.Y - vec1.Y) * (vec2.Y - vec1.Y));
        }

        /// <summary>
        /// Compute the squared euclidean distance between two vectors.
        /// </summary>
        /// <param name="vec1">The first vector</param>
        /// <param name="vec2">The second vector</param>
        /// <returns>The squared distance</returns>
        public static float DistanceSquared(Vector2 vec1, Vector2 vec2)
        {
            DistanceSquared(ref vec1, ref vec2, out float result);
            return result;
        }

        /// <summary>
        /// Compute the squared euclidean distance between two vectors.
        /// </summary>
        /// <param name="vec1">The first vector</param>
        /// <param name="vec2">The second vector</param>
        /// <param name="result">The squared distance</param>
        public static void DistanceSquared(ref Vector2 vec1, ref Vector2 vec2, out float result)
        {
            result = (vec2.X - vec1.X) * (vec2.X - vec1.X) + (vec2.Y - vec1.Y) * (vec2.Y - vec1.Y);
        }

        /// <summary>
        /// Retrieves the orientation of a set of vertices using the Shoelace formula (https://en.wikipedia.org/wiki/Shoelace_formula)
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns>Twice the area enclosed by the vertices.
        /// The vertices are clockwise-oriented if the value is positive.
        /// The vertices are counter-clockwise-oriented if the value is negative.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetOrientation(in ReadOnlySpan<Vector2> vertices)
        {
            if (vertices.Length == 0)
                return 0;

            float rotation = 0;
            for (int i = 0; i < vertices.Length - 1; ++i)
                rotation += (vertices[i + 1].X - vertices[i].X) * (vertices[i + 1].Y + vertices[i].Y);

            rotation += (vertices[0].X - vertices[^1].X) * (vertices[0].Y + vertices[^1].Y);

            return rotation;
        }

        /// <summary>
        /// Determines whether a point is within the right half-plane of a line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="point">The point.</param>
        /// <returns>Whether <paramref name="point"/> is in the right half-plane of <paramref name="line"/>.
        /// If the point is colinear to the line, it is said to be in the right half-plane of the line.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InRightHalfPlaneOf(this Vector2 point, in Line line)
            => (line.EndPoint.X - line.StartPoint.X) * (point.Y - line.StartPoint.Y)
                - (line.EndPoint.Y - line.StartPoint.Y) * (point.X - line.StartPoint.X) <= 0;
    }
}
