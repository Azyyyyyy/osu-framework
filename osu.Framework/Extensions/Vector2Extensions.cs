// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Numerics;

namespace osu.Framework.Extensions
{
    public static class Vector2Extensions
    {
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

        public static float GetIndex(this Vector2 vec, int index)
        {
            return index switch
            {
                0 => vec.X,
                1 => vec.Y,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Range for this is 0-1")
            };
        }

        public static void SetIndex(this Vector2 vec, int index, float val)
        {
            if (index == 0)
            {
                vec.X = val;
            }
            else if (index == 1)
            {
                vec.Y = val;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Range for this is 0-1");
            }
        }

        public static Vector2 PerpendicularRight(this Vector2 vec) => new Vector2(vec.Y, -vec.X);

        public static Vector2 PerpendicularLeft(this Vector2 vec) => new Vector2(-vec.Y, vec.X);

        public static Vector2 ComponentMin(Vector2 a, Vector2 b)
        {
            a.X = a.X < b.X ? a.X : b.X;
            a.Y = a.Y < b.Y ? a.Y : b.Y;
            return a;
        }

        public static Vector2 XY(this Vector3 vector3) => new Vector2(vector3.X, vector3.Y);

        /// <summary>
        /// Returns a copy of the Vector2 scaled to unit length.
        /// </summary>
        /// <returns></returns>
        public static Vector2 Normalized(this Vector2 vec)
        {
            Vector2 v = vec;
            v.Normalize();
            return v;
        }

        /// <summary>
        /// Scale a vector to unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <returns>The normalized vector</returns>
        public static Vector2 Normalize(this Vector2 vec)
        {
            float scale = 1.0f / vec.Length();
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }
    }
}
