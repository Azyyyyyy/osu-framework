// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Silk.NET.Input;

namespace osu.Framework.Input.States
{
    public class KeyboardState
    {
        public readonly ButtonStates<Key> Keys = new ButtonStates<Key>();

        /// <summary>
        /// Whether left or right control key is pressed.
        /// </summary>
        public bool ControlPressed => Keys.IsPressed(Key.ControlLeft) || Keys.IsPressed(Key.ControlRight);

        /// <summary>
        /// Whether left or right alt key is pressed.
        /// </summary>
        public bool AltPressed => Keys.IsPressed(Key.AltLeft) || Keys.IsPressed(Key.AltRight);

        /// <summary>
        /// Whether left or right shift key is pressed.
        /// </summary>
        public bool ShiftPressed => Keys.IsPressed(Key.ShiftLeft) || Keys.IsPressed(Key.ShiftRight);

        /// <summary>
        /// Whether left or right super key (Win key on Windows, or Command key on Mac) is pressed.
        /// </summary>
        public bool SuperPressed => Keys.IsPressed(Key.SuperLeft) || Keys.IsPressed(Key.SuperRight);
    }
}
