// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Platform.MacOS.Native;

namespace osu.Framework.Platform.MacOS
{
    internal class MacOSTextInput : GameWindowTextInput
    {
        // Defined as kCGEventFlagMaskAlphaShift in CoreGraphics
        private const ulong event_flag_mask_alpha_shift = 65536;

        // Defined as kCGEventSourceStateHIDSystemState in CoreGraphics
        private const int event_source_state_hid_system_state = 1;

        private static bool isCapsLockOn => (Cocoa.CGEventSourceFlagsState(event_source_state_hid_system_state) & event_flag_mask_alpha_shift) != 0;

        public MacOSTextInput(IWindow window)
            : base(window)
        {
        }

        protected override void HandleKeyPress(object sender, KeyDownEvent e)
        {
            // arbitrary choice here, but it caters for any non-printable keys on an A1243 Apple Keyboard
            if ((int)e.Key > 63000)
                return;

            base.HandleKeyPress(sender, e);
        }
    }
}
