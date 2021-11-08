// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using Silk.NET.OpenGL;

namespace osu.Framework.Graphics
{
    /// <summary>
    /// Contains information about how an <see cref="IDrawable"/> should be blended into its destination.
    /// </summary>
    public struct BlendingParameters : IEquatable<BlendingParameters>
    {
        #region Public Members

        /// <summary>
        /// The blending factor for the source color of the blend.
        /// </summary>
        public BlendingType Source;

        /// <summary>
        /// The blending factor for the destination color of the blend.
        /// </summary>
        public BlendingType Destination;

        /// <summary>
        /// The blending factor for the source alpha of the blend.
        /// </summary>
        public BlendingType SourceAlpha;

        /// <summary>
        /// The blending factor for the destination alpha of the blend.
        /// </summary>
        public BlendingType DestinationAlpha;

        /// <summary>
        /// Gets or sets the <see cref="BlendingEquation"/> to use for the RGB components of the blend.
        /// </summary>
        public BlendingEquation RGBEquation;

        /// <summary>
        /// Gets or sets the <see cref="BlendingEquation"/> to use for the alpha component of the blend.
        /// </summary>
        public BlendingEquation AlphaEquation;

        #endregion

        #region Default Blending Parameter Types

        public static BlendingParameters None => new BlendingParameters
        {
            Source = BlendingType.One,
            Destination = BlendingType.Zero,
            SourceAlpha = BlendingType.One,
            DestinationAlpha = BlendingType.Zero,
            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add,
        };

        public static BlendingParameters Inherit => new BlendingParameters
        {
            Source = BlendingType.Inherit,
            Destination = BlendingType.Inherit,
            SourceAlpha = BlendingType.Inherit,
            DestinationAlpha = BlendingType.Inherit,
            RGBEquation = BlendingEquation.Inherit,
            AlphaEquation = BlendingEquation.Inherit,
        };

        public static BlendingParameters Mixture => new BlendingParameters
        {
            Source = BlendingType.SrcAlpha,
            Destination = BlendingType.OneMinusSrcAlpha,
            SourceAlpha = BlendingType.One,
            DestinationAlpha = BlendingType.One,
            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add,
        };

        public static BlendingParameters Additive => new BlendingParameters
        {
            Source = BlendingType.SrcAlpha,
            Destination = BlendingType.One,
            SourceAlpha = BlendingType.One,
            DestinationAlpha = BlendingType.One,
            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add,
        };

        #endregion

        /// <summary>
        /// Copy all properties that are marked as inherited from a parent <see cref="BlendingParameters"/> object.
        /// </summary>
        /// <param name="parent">The parent <see cref="BlendingParameters"/> from which to copy inherited properties.</param>
        public void CopyFromParent(BlendingParameters parent)
        {
            if (Source == BlendingType.Inherit)
                Source = parent.Source;

            if (Destination == BlendingType.Inherit)
                Destination = parent.Destination;

            if (SourceAlpha == BlendingType.Inherit)
                SourceAlpha = parent.SourceAlpha;

            if (DestinationAlpha == BlendingType.Inherit)
                DestinationAlpha = parent.DestinationAlpha;

            if (RGBEquation == BlendingEquation.Inherit)
                RGBEquation = parent.RGBEquation;

            if (AlphaEquation == BlendingEquation.Inherit)
                AlphaEquation = parent.AlphaEquation;
        }

        /// <summary>
        /// Any properties marked as inherited will have their blending mode changed to the default type. This can occur when a root element is set to inherited.
        /// </summary>
        public void ApplyDefaultToInherited()
        {
            if (Source == BlendingType.Inherit)
                Source = BlendingType.SrcAlpha;

            if (Destination == BlendingType.Inherit)
                Destination = BlendingType.OneMinusSrcAlpha;

            if (SourceAlpha == BlendingType.Inherit)
                SourceAlpha = BlendingType.One;

            if (DestinationAlpha == BlendingType.Inherit)
                DestinationAlpha = BlendingType.One;

            if (RGBEquation == BlendingEquation.Inherit)
                RGBEquation = BlendingEquation.Add;

            if (AlphaEquation == BlendingEquation.Inherit)
                AlphaEquation = BlendingEquation.Add;
        }

        public readonly bool Equals(BlendingParameters other) =>
            other.Source == Source
            && other.Destination == Destination
            && other.SourceAlpha == SourceAlpha
            && other.DestinationAlpha == DestinationAlpha
            && other.RGBEquation == RGBEquation
            && other.AlphaEquation == AlphaEquation;

        public static bool operator ==(in BlendingParameters left, in BlendingParameters right) =>
            left.Source == right.Source &&
            left.Destination == right.Destination &&
            left.SourceAlpha == right.SourceAlpha &&
            left.DestinationAlpha == right.DestinationAlpha &&
            left.RGBEquation == right.RGBEquation &&
            left.AlphaEquation == right.AlphaEquation;

        public static bool operator !=(in BlendingParameters left, in BlendingParameters right) => !(left == right);

        public override readonly bool Equals(object obj) => obj is BlendingParameters other && this == other;

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override readonly int GetHashCode() => HashCode.Combine(Source, Destination, SourceAlpha, DestinationAlpha, RGBEquation, AlphaEquation);

        public readonly bool IsDisabled =>
            Source == BlendingType.One
            && Destination == BlendingType.Zero
            && SourceAlpha == BlendingType.One
            && DestinationAlpha == BlendingType.Zero
            && RGBEquation == BlendingEquation.Add
            && AlphaEquation == BlendingEquation.Add;

        public override readonly string ToString() => $"BlendingParameter: Factor: {Source}/{Destination}/{SourceAlpha}/{DestinationAlpha} RGBEquation: {RGBEquation} AlphaEquation: {AlphaEquation}";

        #region GL Type Getters

        /// <summary>
        /// Gets the <see cref="BlendingEquation"/> for the currently specified RGB Equation.
        /// </summary>
        public readonly BlendEquationModeEXT RGBEquationMode => translateEquation(RGBEquation);

        /// <summary>
        /// Gets the <see cref="BlendingEquation"/> for the currently specified Alpha Equation.
        /// </summary>
        public readonly BlendEquationModeEXT AlphaEquationMode => translateEquation(AlphaEquation);

        /// <summary>
        /// Gets the <see cref="BlendingFactor"/> for the currently specified source blending mode.
        /// </summary>
        public readonly BlendingFactor SourceBlendingFactor => translateBlendingFactorSrc(Source);

        /// <summary>
        /// Gets the <see cref="BlendingFactor"/> for the currently specified destination blending mode.
        /// </summary>
        public readonly BlendingFactor DestinationBlendingFactor => translateBlendingFactorDest(Destination);

        /// <summary>
        /// Gets the <see cref="BlendingFactor"/> for the currently specified source alpha mode.
        /// </summary>
        public readonly BlendingFactor SourceAlphaBlendingFactor => translateBlendingFactorSrc(SourceAlpha);

        /// <summary>
        /// Gets the <see cref="BlendingFactor"/> for the currently specified destination alpha mode.
        /// </summary>
        public readonly BlendingFactor DestinationAlphaBlendingFactor => translateBlendingFactorDest(DestinationAlpha);

        private static BlendingFactor translateBlendingFactorSrc(BlendingType factor)
        {
            switch (factor)
            {
                case BlendingType.ConstantAlpha:
                    return BlendingFactor.ConstantAlpha;

                case BlendingType.ConstantColor:
                    return BlendingFactor.ConstantColor;

                case BlendingType.DstAlpha:
                    return BlendingFactor.SrcAlpha;

                case BlendingType.DstColor:
                    return BlendingFactor.SrcColor;

                case BlendingType.One:
                    return BlendingFactor.One;

                case BlendingType.OneMinusConstantAlpha:
                    return BlendingFactor.OneMinusConstantAlpha;

                case BlendingType.OneMinusConstantColor:
                    return BlendingFactor.OneMinusConstantColor;

                case BlendingType.OneMinusDstAlpha:
                    return BlendingFactor.OneMinusDstAlpha;

                case BlendingType.OneMinusDstColor:
                    return BlendingFactor.OneMinusDstColor;

                case BlendingType.OneMinusSrcAlpha:
                    return BlendingFactor.OneMinusSrcColor;

                case BlendingType.SrcAlpha:
                    return BlendingFactor.SrcAlpha;

                case BlendingType.SrcAlphaSaturate:
                    return BlendingFactor.SrcAlphaSaturate;

                case BlendingType.SrcColor:
                    return BlendingFactor.SrcColor;

                default:
                case BlendingType.Zero:
                    return BlendingFactor.Zero;
            }
        }

        private static BlendingFactor translateBlendingFactorDest(BlendingType factor)
        {
            switch (factor)
            {
                case BlendingType.ConstantAlpha:
                    return BlendingFactor.DstAlpha;

                case BlendingType.ConstantColor:
                    return BlendingFactor.ConstantColor;

                case BlendingType.DstAlpha:
                    return BlendingFactor.DstAlpha;

                case BlendingType.DstColor:
                    return BlendingFactor.DstColor;

                case BlendingType.One:
                    return BlendingFactor.One;

                case BlendingType.OneMinusConstantAlpha:
                    return BlendingFactor.OneMinusConstantAlpha;

                case BlendingType.OneMinusConstantColor:
                    return BlendingFactor.OneMinusConstantColor;

                case BlendingType.OneMinusDstAlpha:
                    return BlendingFactor.OneMinusDstAlpha;

                case BlendingType.OneMinusDstColor:
                    return BlendingFactor.OneMinusDstColor;

                case BlendingType.OneMinusSrcAlpha:
                    return BlendingFactor.OneMinusSrcAlpha;

                case BlendingType.OneMinusSrcColor:
                    return BlendingFactor.OneMinusSrcColor;

                case BlendingType.SrcAlpha:
                    return BlendingFactor.DstAlpha;

                case BlendingType.SrcAlphaSaturate:
                    return BlendingFactor.DstAlpha;

                case BlendingType.SrcColor:
                    return BlendingFactor.DstColor;

                default:
                case BlendingType.Zero:
                    return BlendingFactor.Zero;
            }
        }

        private static BlendEquationModeEXT translateEquation(BlendingEquation blendingEquation)
        {
            switch (blendingEquation)
            {
                default:
                case BlendingEquation.Inherit:
                case BlendingEquation.Add:
                    return BlendEquationModeEXT.FuncAdd;

                case BlendingEquation.Min:
                    return BlendEquationModeEXT.Min;

                case BlendingEquation.Max:
                    return BlendEquationModeEXT.Max;

                case BlendingEquation.Subtract:
                    return BlendEquationModeEXT.FuncSubtract;

                case BlendingEquation.ReverseSubtract:
                    return BlendEquationModeEXT.FuncReverseSubtract;
            }
        }

        #endregion
    }
}
