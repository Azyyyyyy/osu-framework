// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Framework.Platform.SDL2;
using Silk.NET.SDL;

namespace osu.Framework.Platform.Windows
{
    public class WindowsReadableKeyCombinationProvider : SDL2ReadableKeyCombinationProvider
    {
        protected override string GetReadableKey(InputKey key)
        {
            switch (key)
            {
                case InputKey.Super:
                    return "Win";

                default:
                    return base.GetReadableKey(key);
            }
        }

        protected override bool TryGetNameFromKeycode(KeyCode keycode, out string name)
        {
            switch (keycode)
            {
                case KeyCode.KLgui:
                    name = "LWin";
                    return true;

                case KeyCode.KRgui:
                    name = "RWin";
                    return true;

                default:
                    return base.TryGetNameFromKeycode(keycode, out name);
            }
        }
    }
}
