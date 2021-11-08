// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using Silk.NET.SDL;

namespace osu.Framework.Platform.SDL2
{
    public class SDL2ReadableKeyCombinationProvider : ReadableKeyCombinationProvider
    {
        private readonly Sdl sdl = SdlProvider.SDL.Value;

        protected override string GetReadableKey(InputKey key)
        {
            var keycode = (KeyCode)sdl.GetKeyFromScancode(key.ToScancode());

            // early return if unknown. probably because key isn't a keyboard key, or doesn't map to an `SDL_Scancode`.
            if (keycode == KeyCode.KUnknown)
                return base.GetReadableKey(key);

            string name;

            // overrides for some keys that we want displayed differently from SDL_GetKeyName().
            if (TryGetNameFromKeycode(keycode, out name))
                return name;

            name = sdl.GetKeyNameS((int)keycode);

            // fall back if SDL couldn't find a name.
            if (string.IsNullOrEmpty(name))
                return base.GetReadableKey(key);

            // true if SDL_GetKeyName() returned a proper key/scancode name.
            // see https://github.com/libsdl-org/SDL/blob/release-2.0.16/src/events/SDL_keyboard.c#L1012
            if (((int)keycode & (1 << 30)) != 0)
                return name;

            // SDL_GetKeyName() returned a unicode character that would be produced if that key was pressed.
            // consumers expect an uppercase letter.
            return name.ToUpper();
        }

        /// <summary>
        /// Provides overrides for some keys that we want displayed differently from SDL_GetKeyName().
        /// </summary>
        /// <remarks>
        /// Should be overriden per-platform to provide platform-specific names for applicable keys.
        /// </remarks>
        protected virtual bool TryGetNameFromKeycode(KeyCode keycode, out string name)
        {
            switch (keycode)
            {
                case KeyCode.KReturn:
                    name = "Enter";
                    return true;

                case KeyCode.KEscape:
                    name = "Esc";
                    return true;

                case KeyCode.KBackspace:
                    name = "Backsp";
                    return true;

                case KeyCode.KTab:
                    name = "Tab";
                    return true;

                case KeyCode.KSpace:
                    name = "Space";
                    return true;

                case KeyCode.KPlus:
                    name = "Plus";
                    return true;

                case KeyCode.KMinus:
                    name = "Minus";
                    return true;

                case KeyCode.KDelete:
                    name = "Del";
                    return true;

                case KeyCode.KCapslock:
                    name = "Caps";
                    return true;

                case KeyCode.KInsert:
                    name = "Ins";
                    return true;

                case KeyCode.KPageup:
                    name = "PgUp";
                    return true;

                case KeyCode.KPagedown:
                    name = "PgDn";
                    return true;

                case KeyCode.KNumlockclear:
                    name = "NumLock";
                    return true;

                case KeyCode.KKPDivide:
                    name = "NumpadDivide";
                    return true;

                case KeyCode.KKPMultiply:
                    name = "NumpadMultiply";
                    return true;

                case KeyCode.KKPMinus:
                    name = "NumpadMinus";
                    return true;

                case KeyCode.KKPPlus:
                    name = "NumpadPlus";
                    return true;

                case KeyCode.KKPEnter:
                    name = "NumpadEnter";
                    return true;

                case KeyCode.KKPPeriod:
                    name = "NumpadDecimal";
                    return true;

                case KeyCode.KKP0:
                    name = "Numpad0";
                    return true;

                case KeyCode.KKP1:
                    name = "Numpad1";
                    return true;

                case KeyCode.KKP2:
                    name = "Numpad2";
                    return true;

                case KeyCode.KKP3:
                    name = "Numpad3";
                    return true;

                case KeyCode.KKP4:
                    name = "Numpad4";
                    return true;

                case KeyCode.KKP5:
                    name = "Numpad5";
                    return true;

                case KeyCode.KKP6:
                    name = "Numpad6";
                    return true;

                case KeyCode.KKP7:
                    name = "Numpad7";
                    return true;

                case KeyCode.KKP8:
                    name = "Numpad8";
                    return true;

                case KeyCode.KKP9:
                    name = "Numpad9";
                    return true;

                case KeyCode.KLctrl:
                    name = "LCtrl";
                    return true;

                case KeyCode.KLshift:
                    name = "LShift";
                    return true;

                case KeyCode.KLalt:
                    name = "LAlt";
                    return true;

                case KeyCode.KRctrl:
                    name = "RCtrl";
                    return true;

                case KeyCode.KRshift:
                    name = "RShift";
                    return true;

                case KeyCode.KRalt:
                    name = "RAlt";
                    return true;

                case KeyCode.KVolumeup:
                    name = "Vol. Up";
                    return true;

                case KeyCode.KVolumedown:
                    name = "Vol. Down";
                    return true;

                case KeyCode.KAudionext:
                    name = "Media Next";
                    return true;

                case KeyCode.KAudioprev:
                    name = "Media Previous";
                    return true;

                case KeyCode.KAudiostop:
                    name = "Media Stop";
                    return true;

                case KeyCode.KAudioplay:
                    name = "Media Play";
                    return true;

                case KeyCode.KAudiomute:
                    name = "Mute";
                    return true;

                default:
                    name = string.Empty;
                    return false;
            }
        }
    }
}
