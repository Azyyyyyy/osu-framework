// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Framework.Platform.SDL2;
using Silk.NET.SDL;

namespace osu.Framework.Platform.MacOS
{
    public class MacOSReadableKeyCombinationProvider : SDL2ReadableKeyCombinationProvider
    {
        protected override string GetReadableKey(InputKey key)
        {
            switch (key)
            {
                case InputKey.Super:
                    return "Cmd";

                case InputKey.Alt:
                    return "Opt";

                default:
                    return base.GetReadableKey(key);
            }
        }

        protected override bool TryGetNameFromKeycode(KeyCode keycode, out string name)
        {
            switch (keycode)
            {
                case KeyCode.KLgui:
                    name = "LCmd";
                    return true;

                case KeyCode.KRgui:
                    name = "RCmd";
                    return true;

                case KeyCode.KLalt:
                    name = "LOpt";
                    return true;

                case KeyCode.KRalt:
                    name = "ROpt";
                    return true;

                default:
                    return base.TryGetNameFromKeycode(keycode, out name);
            }
        }
    }
}
