﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using osu.Framework.Input.States;
using Silk.NET.Input;

namespace osu.Framework.Input.Events
{
    /// <summary>
    /// An event representing a mouse double click.
    /// </summary>
    public class DoubleClickEvent : MouseButtonEvent
    {
        public DoubleClickEvent(InputState state, MouseButton button, Vector2? screenSpaceMouseDownPosition = null)
            : base(state, button, screenSpaceMouseDownPosition)
        {
        }
    }
}
