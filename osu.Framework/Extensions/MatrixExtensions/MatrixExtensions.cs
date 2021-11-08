// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Numerics;
using Silk.NET.Maths;

namespace osu.Framework.Extensions.MatrixExtensions
{
    public static class MatrixExtensions
    {
        /// <summary>
        /// Returns an inverted copy of this instance.
        /// </summary>
        public static Matrix3X3<float> Inverted(this Matrix3X3<float> mat)
        {
            Matrix3X3<float> m = mat;

            if (m.Determinant() != 0)
            {
                m.Invert();
            }

            return m;
        }

        /// <summary>
        /// Gets the determinant of this matrix.
        /// </summary>
        public static float Determinant(this Matrix3X3<float> mat)
        {
            float m11 = mat.Row1.X,
                  m12 = mat.Row1.Y,
                  m13 = mat.Row1.Z,
                  m21 = mat.Row2.X,
                  m22 = mat.Row2.Y,
                  m23 = mat.Row2.Z,
                  m31 = mat.Row3.X,
                  m32 = mat.Row3.Y,
                  m33 = mat.Row3.Z;

            return m11 * m22 * m33 + m12 * m23 * m31 + m13 * m21 * m32
                   - m13 * m22 * m31 - m11 * m23 * m32 - m12 * m21 * m33;
        }

        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <returns>The inverse of the given matrix if it has one, or the input if it is singular</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Matrix4 is singular.</exception>
        public static Matrix3X3<float> Invert(this Matrix3X3<float> mat)
        {
            Matrix3X3<float> result;
            Invert(ref mat, out result);
            return result;
        }

        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <param name="result">The inverse of the given matrix if it has one, or the input if it is singular</param>
        /// <exception cref="InvalidOperationException">Thrown if the Matrix3 is singular.</exception>
        public static void Invert(ref Matrix3X3<float> mat, out Matrix3X3<float> result)
        {
            int[] colIdx = { 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1 };

            float[,] inverse = {{mat.Row1.X, mat.Row1.Y, mat.Row1.Z},
                {mat.Row2.X, mat.Row2.Y, mat.Row2.Z},
                {mat.Row3.X, mat.Row3.Y, mat.Row3.Z}};

            int icol = 0;
            int irow = 0;

            for (int i = 0; i < 3; i++)
            {
                float maxPivot = 0.0f;

                for (int j = 0; j < 3; j++)
                {
                    if (pivotIdx[j] == 0) continue;

                    for (int k = 0; k < 3; ++k)
                    {
                        if (pivotIdx[k] == -1)
                        {
                            float absVal = System.Math.Abs(inverse[j, k]);
                            if (!(absVal > maxPivot)) continue;

                            maxPivot = absVal;
                            irow = j;
                            icol = k;
                        }
                        else if (pivotIdx[k] > 0)
                        {
                            result = mat;
                            return;
                        }
                    }
                }

                ++(pivotIdx[icol]);

                if (irow != icol)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        (inverse[irow, k], inverse[icol, k]) = (inverse[icol, k], inverse[irow, k]);
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];

                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                }

                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;

                for (int k = 0; k < 3; ++k)
                {
                    inverse[icol, k] *= oneOverPivot;
                }

                for (int j = 0; j < 3; ++j)
                {
                    if (icol != j)
                    {
                        float f = inverse[j, icol];
                        inverse[j, icol] = 0.0f;

                        for (int k = 0; k < 3; ++k)
                        {
                            inverse[j, k] -= inverse[icol, k] * f;
                        }
                    }
                }
            }

            for (int j = 2; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];

                for (int k = 0; k < 3; ++k)
                {
                    (inverse[k, ir], inverse[k, ic]) = (inverse[k, ic], inverse[k, ir]);
                }
            }

            result.Row1.X = inverse[0, 0];
            result.Row1.Y = inverse[0, 1];
            result.Row1.Z = inverse[0, 2];
            result.Row2.X = inverse[1, 0];
            result.Row2.Y = inverse[1, 1];
            result.Row2.Z = inverse[1, 2];
            result.Row3.X = inverse[2, 0];
            result.Row3.Y = inverse[2, 1];
            result.Row3.Z = inverse[2, 2];
        }

        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <returns>The transpose of the given matrix</returns>
        public static Matrix3X3<float> Transpose(this Matrix3X3<float> mat)
        {
            return new Matrix3X3<float>(mat.Column1, mat.Column2, mat.Column3);
        }

        public static void TranslateFromLeft(ref Matrix3X3<float> m, Vector2 v)
        {
            m.Row3 += m.Row1 * v.X + m.Row2 * v.Y;
        }

        public static void TranslateFromRight(ref Matrix3X3<float> m, Vector2 v)
        {
            //m.Column0 += m.Column2 * v.X;
            m.M11 += m.M13 * v.X;
            m.M21 += m.M23 * v.X;
            m.M31 += m.M33 * v.X;

            //m.Column1 += m.Column2 * v.Y;
            m.M12 += m.M13 * v.Y;
            m.M22 += m.M23 * v.Y;
            m.M32 += m.M33 * v.Y;
        }

        public static void RotateFromLeft(ref Matrix3X3<float> m, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            var row1 = m.Row1 * cos + m.Row2 * sin;
            m.Row2 = m.Row2 * cos - m.Row1 * sin;
            m.Row1 = row1;
        }

        public static void RotateFromRight(ref Matrix3X3<float> m, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            //Vector3 column0 = m.Column0 * cos + m.Column1 * sin;
            float m11 = m.M11 * cos - m.M12 * sin;
            float m21 = m.M21 * cos - m.M22 * sin;
            float m31 = m.M31 * cos - m.M32 * sin;

            //m.Column1 = m.Column1 * cos - m.Column0 * sin;
            m.M12 = m.M12 * cos + m.M11 * sin;
            m.M22 = m.M22 * cos + m.M21 * sin;
            m.M32 = m.M32 * cos + m.M31 * sin;

            //m.Column0 = row0;
            m.M11 = m11;
            m.M21 = m21;
            m.M31 = m31;
        }

        public static void ScaleFromLeft(ref Matrix3X3<float> m, Vector2 v)
        {
            m.Row1 *= v.X;
            m.Row2 *= v.Y;
        }

        public static void ScaleFromRight(ref Matrix3X3<float> m, Vector2 v)
        {
            //m.Column0 *= v.X;
            m.M11 *= v.X;
            m.M21 *= v.X;
            m.M31 *= v.X;

            //m.Column1 *= v.Y;
            m.M12 *= v.Y;
            m.M22 *= v.Y;
            m.M32 *= v.Y;
        }

        /// <summary>
        /// Apply shearing in X and Y direction from the left hand side.
        /// Since shearing is non-commutative it is important to note that we
        /// first shear in the X direction, and then in the Y direction.
        /// </summary>
        /// <param name="m">The matrix to apply the shearing operation to.</param>
        /// <param name="v">The X and Y amounts of shearing.</param>
        public static void ShearFromLeft(ref Matrix3X3<float> m, Vector2 v)
        {
            var row1 = m.Row1 + m.Row2 * v.Y + m.Row1 * v.X * v.Y;
            m.Row2 += m.Row1 * v.X;
            m.Row1 = row1;
        }

        /// <summary>
        /// Apply shearing in X and Y direction from the right hand side.
        /// Since shearing is non-commutative it is important to note that we
        /// first shear in the Y direction, and then in the X direction.
        /// </summary>
        /// <param name="m">The matrix to apply the shearing operation to.</param>
        /// <param name="v">The X and Y amounts of shearing.</param>
        public static void ShearFromRight(ref Matrix3X3<float> m, Vector2 v)
        {
            float xy = v.X * v.Y;

            //m.Column0 += m.Column1 * v.X;
            float m11 = m.M11 + m.M12 * v.X;
            float m21 = m.M21 + m.M22 * v.X;
            float m31 = m.M31 + m.M32 * v.X;

            //m.Column1 += m.Column0 * v.Y + m.Column1 * xy;
            m.M12 += m.M11 * v.Y + m.M12 * xy;
            m.M22 += m.M21 * v.Y + m.M22 * xy;
            m.M32 += m.M31 * v.Y + m.M32 * xy;

            m.M11 = m11;
            m.M21 = m21;
            m.M31 = m31;
        }

        public static Vector2 XY(this Matrix3X3<float> m) => new Vector2(m.Column1.Length, m.Column2.Length);

        public static Vector3 ExtractScale(this Matrix3X3<float> m) => new Vector3(m.Column1.Length, m.Column2.Length, m.Column3.Length);

        public static Matrix3X3<float> Zero = new Matrix3X3<float>();

        public static void FastInvert(ref Matrix3X3<float> value)
        {
            float d11 = value.M22 * value.M33 + value.M23 * -value.M32;
            float d12 = value.M21 * value.M33 + value.M23 * -value.M31;
            float d13 = value.M21 * value.M32 + value.M22 * -value.M31;

            float det = value.M11 * d11 - value.M12 * d12 + value.M13 * d13;

            if (Math.Abs(det) == 0.0f)
            {
                value = Zero;
                return;
            }

            det = 1f / det;

            float d21 = value.M12 * value.M33 + value.M13 * -value.M32;
            float d22 = value.M11 * value.M33 + value.M13 * -value.M31;
            float d23 = value.M11 * value.M32 + value.M12 * -value.M31;

            float d31 = value.M12 * value.M23 - value.M13 * value.M22;
            float d32 = value.M11 * value.M23 - value.M13 * value.M21;
            float d33 = value.M11 * value.M22 - value.M12 * value.M21;

            value.M11 = +d11 * det;
            value.M12 = -d21 * det;
            value.M13 = +d31 * det;
            value.M21 = -d12 * det;
            value.M22 = +d22 * det;
            value.M23 = -d32 * det;
            value.M31 = +d13 * det;
            value.M32 = -d23 * det;
            value.M33 = +d33 * det;
        }
    }
}
