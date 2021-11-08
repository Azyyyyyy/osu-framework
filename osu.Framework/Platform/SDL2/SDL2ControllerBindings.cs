// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Silk.NET.SDL;

namespace osu.Framework.Platform.SDL2
{
    /// <summary>
    /// Maintain a copy of the SDL-provided bindings for the given controller.
    /// Used to determine whether a given event's joystick button or axis is unmapped.
    /// </summary>
    public unsafe class SDL2ControllerBindings
    {
        public readonly Joystick* JoystickHandle;
        public readonly GameController? ControllerHandle;
        private readonly Sdl sdl = SdlProvider.SDL.Value;

        /// <summary>
        /// Bindings returned from <see cref="Sdl.GameControllerGetBindForButton(GameController*, GameControllerButton)"/>, indexed by <see cref="GameControllerButton"/>.
        /// Empty if the joystick does not have a corresponding ControllerHandle.
        /// </summary>
        public GameControllerButtonBind[] ButtonBindings;

        /// <summary>
        /// Bindings returned from <see cref="Sdl.GameControllerGetBindForAxis(GameController*, GameControllerAxis)"/>, indexed by <see cref="GameControllerAxis"/>.
        /// Empty if the joystick does not have a corresponding ControllerHandle.
        /// </summary>
        public GameControllerButtonBind[] AxisBindings;

        public SDL2ControllerBindings(Joystick* joystickHandle, GameController? controllerHandle)
        {
            JoystickHandle = joystickHandle;
            ControllerHandle = controllerHandle;

            PopulateBindings();
        }

        public void PopulateBindings()
        {
            if (ControllerHandle == null)
            {
                ButtonBindings = Array.Empty<GameControllerButtonBind>();
                AxisBindings = Array.Empty<GameControllerButtonBind>();
                return;
            }

            ButtonBindings = Enumerable.Range(0, (int)GameControllerButton.ControllerButtonMax)
                                       .Select(i =>
                                       {
                                           var conHandle = (GameController)ControllerHandle;
                                           return sdl.GameControllerGetBindForButton(&conHandle, (GameControllerButton)i);
                                       }).ToArray();

            AxisBindings = Enumerable.Range(0, (int)GameControllerAxis.ControllerAxisMax)
                                     .Select(i =>
                                     {
                                         var conHandle = (GameController)ControllerHandle;
                                         return sdl.GameControllerGetBindForAxis(&conHandle, (GameControllerAxis)i);
                                     }).ToArray();
        }

        public GameControllerButton GetButtonForIndex(byte index)
        {
            for (int i = 0; i < ButtonBindings.Length; i++)
            {
                if (ButtonBindings[i].BindType != GameControllerBindType.ControllerBindtypeNone && ButtonBindings[i].Value.Button == index)
                    return (GameControllerButton)i;
            }

            return GameControllerButton.ControllerButtonInvalid;
        }

        public GameControllerAxis GetAxisForIndex(byte index)
        {
            for (int i = 0; i < AxisBindings.Length; i++)
            {
                if (AxisBindings[i].BindType != GameControllerBindType.ControllerBindtypeNone && AxisBindings[i].Value.Button == index)
                    return (GameControllerAxis)i;
            }

            return GameControllerAxis.ControllerAxisInvalid;
        }
    }
}
