﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using System;
using System.Linq;

namespace osu.Framework.Graphics
{
    public static class TransformableExtensions
    {
        /// <summary>
        /// Transforms a given property or field member of a given <see cref="ITransformable"/> <typeparamref name="TThis"/> to <paramref name="newValue"/>.
        /// The value of the given member is smoothly changed over time using the given <paramref name="easing"/> for tweening.
        /// </summary>
        /// <typeparam name="TThis">The type of the <see cref="ITransformable"/> to apply the <see cref="Transform{TValue, T}"/> to.</typeparam>
        /// <typeparam name="TValue">The value type which is being transformed.</typeparam>
        /// <param name="t">The <see cref="ITransformable"/> to apply the <see cref="Transform{TValue, T}"/> to.</param>
        /// <param name="propertyOrFieldName">The property or field name of the member ot <typeparamref name="TThis"/> to transform.</param>
        /// <param name="newValue">The value to transform to.</param>
        /// <param name="duration">The transform duration.</param>
        /// <param name="easing">The transform easing to be used for tweening.</param>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<TThis> TransformTo<TThis, TValue>(this TThis t, string propertyOrFieldName, TValue newValue, double duration = 0, Easing easing = Easing.None)
            where TThis : ITransformable =>
            t.TransformTo(t.MakeTransform(propertyOrFieldName, newValue, duration, easing));

        /// <summary>
        /// Applies a <see cref="Transform"/> to a given <see cref="ITransformable"/>.
        /// </summary>
        /// <typeparam name="TThis">The type of the <see cref="ITransformable"/> to apply the <see cref="Transform"/> to.</typeparam>
        /// <param name="t">The <see cref="ITransformable"/> to apply the <see cref="Transform{TValue, T}"/> to.</param>
        /// <param name="transform">The transform to use.</param>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<TThis> TransformTo<TThis>(this TThis t, Transform transform) where TThis : ITransformable
        {
            var result = new TransformSequence<TThis>(t);
            result.Add(transform);
            t.AddTransform(transform);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Transform{TValue, T}"/> for smoothly changing <paramref name="propertyOrFieldName"/>
        /// over time using the given <paramref name="easing"/> for tweening.
        /// <see cref="PopulateTransform{TValue, TThis}(TThis, Transform{TValue, TThis}, TValue, double, Easing)"/>
        /// is invoked as part of this method.
        /// </summary>
        /// <typeparam name="TThis">The type of the <see cref="ITransformable"/> the <see cref="Transform{TValue, T}"/> can be applied to.</typeparam>
        /// <typeparam name="TValue">The value type which is being transformed.</typeparam>
        /// <param name="t">The <see cref="ITransformable"/> the <see cref="Transform{TValue, T}"/> will be applied to.</param>
        /// <param name="propertyOrFieldName">The property or field name of the member ot <typeparamref name="TThis"/> to transform.</param>
        /// <param name="newValue">The value to transform to.</param>
        /// <param name="duration">The transform duration.</param>
        /// <param name="easing">The transform easing to be used for tweening.</param>
        /// <returns>The resulting <see cref="Transform{TValue, T}"/>.</returns>
        public static Transform<TValue, TThis> MakeTransform<TThis, TValue>(this TThis t, string propertyOrFieldName, TValue newValue, double duration = 0, Easing easing = Easing.None)
            where TThis : ITransformable =>
            t.PopulateTransform(new TransformCustom<TValue, TThis>(propertyOrFieldName), newValue, duration, easing);

        /// <summary>
        /// Populates a newly created <see cref="Transform{TValue, T}"/> with necessary values.
        /// All <see cref="Transform{TValue, T}"/>s must be populated by this method prior to being used.
        /// </summary>
        /// <typeparam name="TThis">The type of the <see cref="ITransformable"/> the <see cref="Transform{TValue, T}"/> can be applied to.</typeparam>
        /// <typeparam name="TValue">The value type which is being transformed.</typeparam>
        /// <param name="t">The <see cref="ITransformable"/> the <see cref="Transform{TValue, T}"/> will be applied to.</param>
        /// <param name="transform">The transform to populate.</param>
        /// <param name="newValue">The value to transform to.</param>
        /// <param name="duration">The transform duration.</param>
        /// <param name="easing">The transform easing to be used for tweening.</param>
        /// <returns>The populated <paramref name="transform"/>.</returns>
        public static Transform<TValue, TThis> PopulateTransform<TValue, TThis>(this TThis t, Transform<TValue, TThis> transform, TValue newValue, double duration = 0, Easing easing = Easing.None)
            where TThis : ITransformable
        {
            if (duration < 0)
                throw new ArgumentOutOfRangeException(nameof(duration), $"{nameof(duration)} must be positive.");

            if (transform.Target != null)
                throw new InvalidOperationException($"May not {nameof(PopulateTransform)} the same {nameof(Transform<TValue, TThis>)} more than once.");

            transform.Target = t;

            double startTime = t.TransformStartTime;

            transform.StartTime = startTime;
            transform.EndTime = startTime + duration;
            transform.EndValue = newValue;
            transform.Easing = easing;

            return transform;
        }

        public static TransformSequence<T> Animate<T>(this T transformable, params TransformSequence<T>.Generator[] childGenerators) where T : ITransformable =>
            transformable.Delay(0, childGenerators);

        public static TransformSequence<T> Delay<T>(this T transformable, double delay, params TransformSequence<T>.Generator[] childGenerators) where T : ITransformable =>
            new TransformSequence<T>(transformable).Delay(delay, childGenerators);

        public static TransformSequence<T> Loop<T>(this T transformable, double pause, int numIters, params TransformSequence<T>.Generator[] childGenerators)
            where T : ITransformable =>
            transformable.Delay(0).Loop(pause, numIters, childGenerators);

        public static TransformSequence<T> Loop<T>(this T transformable, double pause, params TransformSequence<T>.Generator[] childGenerators)
            where T : ITransformable =>
            transformable.Delay(0).Loop(pause, childGenerators);

        public static TransformSequence<T> Loop<T>(this T transformable, params TransformSequence<T>.Generator[] childGenerators)
            where T : ITransformable =>
            transformable.Delay(0).Loop(childGenerators);

        public static TransformSequence<T> Loop<T>(this T transformable, double pause = 0)
            where T : ITransformable =>
            transformable.Delay(0).Loop(pause);

        public static TransformSequence<T> Spin<T>(this T drawable, double revolutionDuration, RotationDirection direction, float startRotation = 0) where T : Drawable =>
            drawable.Delay(0).Spin(revolutionDuration, direction, startRotation);

        public static TransformSequence<T> Spin<T>(this T drawable, double revolutionDuration, RotationDirection direction, float startRotation, int numRevolutions) where T : Drawable =>
            drawable.Delay(0).Spin(revolutionDuration, direction, startRotation, numRevolutions);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Alpha"/> to 1 over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeIn<T>(this T drawable, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.FadeTo(1, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Alpha"/> from 0 to 1 over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeInFromZero<T>(this T drawable, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.FadeTo(0).FadeIn(duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Alpha"/> to 0 over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeOut<T>(this T drawable, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.FadeTo(0, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Alpha"/> from 1 to 0 over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeOutFromOne<T>(this T drawable, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.FadeTo(1).FadeOut(duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Alpha"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeTo<T>(this T drawable, float newAlpha, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Alpha), newAlpha, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Colour"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeColour<T>(this T drawable, ColourInfo newColour, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Colour), newColour, duration, easing);

        /// <summary>
        /// Instantaneously flashes <see cref="Drawable.Colour"/>, then smoothly changes it back over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FlashColour<T>(this T drawable, ColourInfo flashColour, double duration, Easing easing = Easing.None) where T : Drawable
        {
            ColourInfo endValue = (drawable.Transforms.LastOrDefault(t => t.TargetMember == nameof(drawable.Colour)) as Transform<ColourInfo>)?.EndValue ?? drawable.Colour;
            return drawable.FadeColour(flashColour).FadeColour(endValue, duration, easing);
        }

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Rotation"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> RotateTo<T>(this T drawable, float newRotation, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Rotation), newRotation, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Scale"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> ScaleTo<T>(this T drawable, float newScale, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.ScaleTo(new Vector2(newScale), duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Scale"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> ScaleTo<T>(this T drawable, Vector2 newScale, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Scale), newScale, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Size"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> ResizeTo<T>(this T drawable, float newSize, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.ResizeTo(new Vector2(newSize), duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Size"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> ResizeTo<T>(this T drawable, Vector2 newSize, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Size), newSize, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Width"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> ResizeWidthTo<T>(this T drawable, float newWidth, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Width), newWidth, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Height"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> ResizeHeightTo<T>(this T drawable, float newHeight, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Height), newHeight, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Position"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> MoveTo<T>(this T drawable, Vector2 newPosition, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Position), newPosition, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.X"/> or <see cref="Drawable.Y"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> MoveTo<T>(this T drawable, Direction direction, float destination, double duration = 0, Easing easing = Easing.None) where T : Drawable
        {
            switch (direction)
            {
                case Direction.Horizontal:
                    return drawable.MoveToX(destination, duration, easing);
                case Direction.Vertical:
                    return drawable.MoveToY(destination, duration, easing);
            }

            throw new InvalidOperationException($"Invalid direction ({direction}) passed to {nameof(MoveTo)}.");
        }

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.X"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> MoveToX<T>(this T drawable, float destination, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.X), destination, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Y"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> MoveToY<T>(this T drawable, float destination, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.TransformTo(nameof(drawable.Y), destination, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="Drawable.Position"/> by an offset to its final value over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> MoveToOffset<T>(this T drawable, Vector2 offset, double duration = 0, Easing easing = Easing.None) where T : Drawable =>
            drawable.MoveTo((drawable.Transforms.LastOrDefault(t => t.TargetMember == nameof(drawable.Position)) as Transform<Vector2>)?.EndValue ?? drawable.Position + offset, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="IContainer.RelativeChildSize"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformRelativeChildSizeTo<T>(this T container, Vector2 newSize, double duration = 0, Easing easing = Easing.None)
            where T : IContainer =>
            container.TransformTo(nameof(container.RelativeChildSize), newSize, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="IContainer.RelativeChildOffset"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformRelativeChildOffsetTo<T>(this T container, Vector2 newOffset, double duration = 0, Easing easing = Easing.None)
            where T : IContainer =>
            container.TransformTo(nameof(container.RelativeChildOffset), newOffset, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="IBufferedContainer.BlurSigma"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> BlurTo<T>(this T bufferedContainer, Vector2 newBlurSigma, double duration = 0, Easing easing = Easing.None)
            where T : IBufferedContainer =>
            bufferedContainer.TransformTo(nameof(bufferedContainer.BlurSigma), newBlurSigma, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="IFillFlowContainer.Spacing"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformSpacingTo<T>(this T flowContainer, Vector2 newSpacing, double duration = 0, Easing easing = Easing.None)
            where T : IFillFlowContainer =>
            flowContainer.TransformTo(nameof(flowContainer.Spacing), newSpacing, duration, easing);

        /// <summary>
        /// Smoothly adjusts the alpha channel of the colour of <see cref="IContainer.EdgeEffect"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeEdgeEffectTo<T>(this T container, float newAlpha, double duration = 0, Easing easing = Easing.None)
            where T : IContainer
        {
            Color4 targetColour = container.EdgeEffect.Colour;
            targetColour.A = newAlpha;
            return container.FadeEdgeEffectTo(targetColour, duration, easing);
        }

        /// <summary>
        /// Smoothly adjusts the colour of <see cref="IContainer.EdgeEffect"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeEdgeEffectTo<T>(this T container, Color4 newColour, double duration = 0, Easing easing = Easing.None) where T : IContainer
        {
            var effect = container.EdgeEffect;
            effect.Colour = newColour;
            return container.TweenEdgeEffectTo(effect, duration, easing);
        }

        /// <summary>
        /// Smoothly adjusts all parameters of <see cref="IContainer.EdgeEffect"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TweenEdgeEffectTo<T>(this T container, EdgeEffectParameters newParameters, double duration = 0, Easing easing = Easing.None)
            where T : IContainer =>
            container.TransformTo(nameof(container.EdgeEffect), newParameters, duration, easing);
    }
}
