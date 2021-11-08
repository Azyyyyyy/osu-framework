// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Framework.Platform.SDL2;
using Silk.NET.SDL;

namespace osu.Framework.Platform.Linux
{
    public class LinuxReadableKeyCombinationProvider : SDL2ReadableKeyCombinationProvider
    {
        protected override string GetReadableKey(InputKey key)
        {
            switch (key)
            {
                case InputKey.Super:
                    return "Super";

                default:
                    return base.GetReadableKey(key);
            }
        }

        protected override bool TryGetNameFromKeycode(KeyCode keycode, out string name)
        {
            switch (keycode)
            {
                case KeyCode.KLgui:
                    name = "LSuper";
                    return true;

                case KeyCode.KRgui:
                    name = "RSuper";
                    return true;

                default:
                    return base.TryGetNameFromKeycode(keycode, out name);
            }
        }
    }
}
