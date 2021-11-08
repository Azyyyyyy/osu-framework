// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace osu.Framework.Platform.SDL2
{
    public static class SDL2Extensions
    {
        public static Key ToKey(this Keysym sdlKeysym)
        {
            // Apple devices don't have the notion of NumLock (they have a Clear key instead).
            // treat them as if they always have NumLock on (the numpad always performs its primary actions).
            bool numLockOn = ((Keymod)sdlKeysym.Mod).HasFlagFast(Keymod.KmodNum) || RuntimeInfo.IsApple;

            switch (sdlKeysym.Scancode)
            {
                default:
                case Scancode.ScancodeUnknown:
                    return Key.Unknown;

                case Scancode.ScancodeKPComma:
                    return Key.Comma;

                case Scancode.ScancodeKPTab:
                    return Key.Tab;

                case Scancode.ScancodeKPBackspace:
                    return Key.Backspace;

                case Scancode.ScancodeKPA:
                    return Key.A;

                case Scancode.ScancodeKPB:
                    return Key.B;

                case Scancode.ScancodeKPC:
                    return Key.C;

                case Scancode.ScancodeKPD:
                    return Key.D;

                case Scancode.ScancodeKPE:
                    return Key.E;

                case Scancode.ScancodeKPF:
                    return Key.F;

                case Scancode.ScancodeKPSpace:
                    return Key.Space;

                //case Scancode.ScancodeKPClear:
                //    return Key.Clear;

                case Scancode.ScancodeReturn:
                    return Key.Enter;

                case Scancode.ScancodeEscape:
                    return Key.Escape;

                case Scancode.ScancodeBackspace:
                    return Key.Backspace;

                case Scancode.ScancodeTab:
                    return Key.Tab;

                case Scancode.ScancodeSpace:
                    return Key.Space;

                case Scancode.ScancodeApostrophe:
                    return Key.Apostrophe;

                case Scancode.ScancodeComma:
                    return Key.Comma;

                case Scancode.ScancodeMinus:
                    return Key.Minus;

                case Scancode.ScancodePeriod:
                    return Key.Period;

                case Scancode.ScancodeSlash:
                    return Key.Slash;

                case Scancode.Scancode0:
                    return Key.Number0;

                case Scancode.Scancode1:
                    return Key.Number1;

                case Scancode.Scancode2:
                    return Key.Number2;

                case Scancode.Scancode3:
                    return Key.Number3;

                case Scancode.Scancode4:
                    return Key.Number4;

                case Scancode.Scancode5:
                    return Key.Number5;

                case Scancode.Scancode6:
                    return Key.Number6;

                case Scancode.Scancode7:
                    return Key.Number7;

                case Scancode.Scancode8:
                    return Key.Number8;

                case Scancode.Scancode9:
                    return Key.Number9;

                case Scancode.ScancodeSemicolon:
                    return Key.Semicolon;

                case Scancode.ScancodeEquals:
                    return Key.Equal;

                case Scancode.ScancodeLeftbracket:
                    return Key.LeftBracket;

                case Scancode.ScancodeBackslash:
                    return Key.BackSlash;

                case Scancode.ScancodeRightbracket:
                    return Key.RightBracket;

                case Scancode.ScancodeGrave:
                    return Key.GraveAccent;

                case Scancode.ScancodeA:
                    return Key.A;

                case Scancode.ScancodeB:
                    return Key.B;

                case Scancode.ScancodeC:
                    return Key.C;

                case Scancode.ScancodeD:
                    return Key.D;

                case Scancode.ScancodeE:
                    return Key.E;

                case Scancode.ScancodeF:
                    return Key.F;

                case Scancode.ScancodeG:
                    return Key.G;

                case Scancode.ScancodeH:
                    return Key.H;

                case Scancode.ScancodeI:
                    return Key.I;

                case Scancode.ScancodeJ:
                    return Key.J;

                case Scancode.ScancodeK:
                    return Key.K;

                case Scancode.ScancodeL:
                    return Key.L;

                case Scancode.ScancodeM:
                    return Key.M;

                case Scancode.ScancodeN:
                    return Key.N;

                case Scancode.ScancodeO:
                    return Key.O;

                case Scancode.ScancodeP:
                    return Key.P;

                case Scancode.ScancodeQ:
                    return Key.Q;

                case Scancode.ScancodeR:
                    return Key.R;

                case Scancode.ScancodeS:
                    return Key.S;

                case Scancode.ScancodeT:
                    return Key.T;

                case Scancode.ScancodeU:
                    return Key.U;

                case Scancode.ScancodeV:
                    return Key.V;

                case Scancode.ScancodeW:
                    return Key.W;

                case Scancode.ScancodeX:
                    return Key.X;

                case Scancode.ScancodeY:
                    return Key.Y;

                case Scancode.ScancodeZ:
                    return Key.Z;

                case Scancode.ScancodeCapslock:
                    return Key.CapsLock;

                case Scancode.ScancodeF1:
                    return Key.F1;

                case Scancode.ScancodeF2:
                    return Key.F2;

                case Scancode.ScancodeF3:
                    return Key.F3;

                case Scancode.ScancodeF4:
                    return Key.F4;

                case Scancode.ScancodeF5:
                    return Key.F5;

                case Scancode.ScancodeF6:
                    return Key.F6;

                case Scancode.ScancodeF7:
                    return Key.F7;

                case Scancode.ScancodeF8:
                    return Key.F8;

                case Scancode.ScancodeF9:
                    return Key.F9;

                case Scancode.ScancodeF10:
                    return Key.F10;

                case Scancode.ScancodeF11:
                    return Key.F11;

                case Scancode.ScancodeF12:
                    return Key.F12;

                case Scancode.ScancodePrintscreen:
                    return Key.PrintScreen;

                case Scancode.ScancodeScrolllock:
                    return Key.ScrollLock;

                case Scancode.ScancodePause:
                    return Key.Pause;

                case Scancode.ScancodeInsert:
                    return Key.Insert;

                case Scancode.ScancodeHome:
                    return Key.Home;

                case Scancode.ScancodePageup:
                    return Key.PageUp;

                case Scancode.ScancodeDelete:
                    return Key.Delete;

                case Scancode.ScancodeEnd:
                    return Key.End;

                case Scancode.ScancodePagedown:
                    return Key.PageDown;

                case Scancode.ScancodeRight:
                    return Key.Right;

                case Scancode.ScancodeLeft:
                    return Key.Left;

                case Scancode.ScancodeDown:
                    return Key.Down;

                case Scancode.ScancodeUp:
                    return Key.Up;

                case Scancode.ScancodeNumlockclear:
                    return Key.NumLock;

                case Scancode.ScancodeKPDivide:
                    return Key.KeypadDivide;

                case Scancode.ScancodeKPMultiply:
                    return Key.KeypadMultiply;

                case Scancode.ScancodeKPMinus:
                    return Key.Minus;

                //case Scancode.ScancodeKPPlus:
                //    return Key.KeypadPlus;

                case Scancode.ScancodeKPEnter:
                    return Key.KeypadEnter;

                case Scancode.ScancodeKP1:
                    return numLockOn ? Key.Keypad1 : Key.End;

                case Scancode.ScancodeKP2:
                    return numLockOn ? Key.Keypad2 : Key.Down;

                case Scancode.ScancodeKP3:
                    return numLockOn ? Key.Keypad3 : Key.PageDown;

                case Scancode.ScancodeKP4:
                    return numLockOn ? Key.Keypad4 : Key.Left;

                case Scancode.ScancodeKP5:
                    return Key.Keypad5;

                case Scancode.ScancodeKP6:
                    return numLockOn ? Key.Keypad6 : Key.Right;

                case Scancode.ScancodeKP7:
                    return numLockOn ? Key.Keypad7 : Key.Home;

                case Scancode.ScancodeKP8:
                    return numLockOn ? Key.Keypad8 : Key.Up;

                case Scancode.ScancodeKP9:
                    return numLockOn ? Key.Keypad9 : Key.PageUp;

                case Scancode.ScancodeKP0:
                    return numLockOn ? Key.Keypad0 : Key.Insert;

                case Scancode.ScancodeKPPeriod:
                    return numLockOn ? Key.Period : Key.Delete;

                case Scancode.ScancodeNonusbackslash:
                    return Key.Backspace;

                case Scancode.ScancodeF13:
                    return Key.F13;

                case Scancode.ScancodeF14:
                    return Key.F14;

                case Scancode.ScancodeF15:
                    return Key.F15;

                case Scancode.ScancodeF16:
                    return Key.F16;

                case Scancode.ScancodeF17:
                    return Key.F17;

                case Scancode.ScancodeF18:
                    return Key.F18;

                case Scancode.ScancodeF19:
                    return Key.F19;

                case Scancode.ScancodeF20:
                    return Key.F20;

                case Scancode.ScancodeF21:
                    return Key.F21;

                case Scancode.ScancodeF22:
                    return Key.F22;

                case Scancode.ScancodeF23:
                    return Key.F23;

                case Scancode.ScancodeF24:
                    return Key.F24;

                case Scancode.ScancodeMenu:
                    return Key.Menu;

                //case Scancode.ScancodeStop:
                //    return Key.Stop;

                //case Scancode.ScancodeMute:
                //    return Key.Mute;

                case Scancode.ScancodeVolumeup:
                    return Key.Up;

                case Scancode.ScancodeVolumedown:
                    return Key.Down;

                //case Scancode.ScancodeClear:
                //    return Key.Clear;

                case Scancode.ScancodeDecimalseparator:
                    return Key.KeypadDecimal;

                case Scancode.ScancodeLctrl:
                    return Key.ControlLeft;

                case Scancode.ScancodeLshift:
                    return Key.ShiftLeft;

                case Scancode.ScancodeLalt:
                    return Key.AltLeft;

                case Scancode.ScancodeLgui:
                    return Key.SuperLeft;

                case Scancode.ScancodeRctrl:
                    return Key.ControlRight;

                case Scancode.ScancodeRshift:
                    return Key.ShiftRight;

                case Scancode.ScancodeRalt:
                    return Key.AltRight;

                case Scancode.ScancodeRgui:
                    return Key.SuperRight;

                /*case Scancode.ScancodeAudionext:
                    return Key.TrackNext;

                case Scancode.ScancodeAudioprev:
                    return Key.TrackPrevious;

                case Scancode.ScancodeAudiostop:
                    return Key.Stop;

                case Scancode.ScancodeAudioplay:
                    return Key.PlayPause;

                case Scancode.ScancodeAudiomute:
                    return Key.Mute;

                case Scancode.ScancodeSleep:
                    return Key.Sleep;*/
            }
        }

        /// <summary>
        /// Returns the corresponding <see cref="Scancode"/> for a given <see cref="InputKey"/>.
        /// </summary>
        /// <param name="inputKey">
        /// Should be a keyboard key.
        /// </param>
        /// <returns>
        /// The corresponding <see cref="Scancode"/> if the <see cref="InputKey"/> is valid.
        /// <see cref="Scancode.ScancodeUnknown"/> otherwise.
        /// </returns>
        public static Scancode ToScancode(this InputKey inputKey)
        {
            switch (inputKey)
            {
                default:
                case InputKey.Shift:
                case InputKey.Control:
                case InputKey.Alt:
                case InputKey.Super:
                case InputKey.F25:
                case InputKey.F26:
                case InputKey.F27:
                case InputKey.F28:
                case InputKey.F29:
                case InputKey.F30:
                case InputKey.F31:
                case InputKey.F32:
                case InputKey.F33:
                case InputKey.F34:
                case InputKey.F35:
                case InputKey.Clear:
                    return Scancode.ScancodeUnknown;

                case InputKey.Menu:
                    return Scancode.ScancodeMenu;

                case InputKey.F1:
                    return Scancode.ScancodeF1;

                case InputKey.F2:
                    return Scancode.ScancodeF2;

                case InputKey.F3:
                    return Scancode.ScancodeF3;

                case InputKey.F4:
                    return Scancode.ScancodeF4;

                case InputKey.F5:
                    return Scancode.ScancodeF5;

                case InputKey.F6:
                    return Scancode.ScancodeF6;

                case InputKey.F7:
                    return Scancode.ScancodeF7;

                case InputKey.F8:
                    return Scancode.ScancodeF8;

                case InputKey.F9:
                    return Scancode.ScancodeF9;

                case InputKey.F10:
                    return Scancode.ScancodeF10;

                case InputKey.F11:
                    return Scancode.ScancodeF11;

                case InputKey.F12:
                    return Scancode.ScancodeF12;

                case InputKey.F13:
                    return Scancode.ScancodeF13;

                case InputKey.F14:
                    return Scancode.ScancodeF14;

                case InputKey.F15:
                    return Scancode.ScancodeF15;

                case InputKey.F16:
                    return Scancode.ScancodeF16;

                case InputKey.F17:
                    return Scancode.ScancodeF17;

                case InputKey.F18:
                    return Scancode.ScancodeF18;

                case InputKey.F19:
                    return Scancode.ScancodeF19;

                case InputKey.F20:
                    return Scancode.ScancodeF20;

                case InputKey.F21:
                    return Scancode.ScancodeF21;

                case InputKey.F22:
                    return Scancode.ScancodeF22;

                case InputKey.F23:
                    return Scancode.ScancodeF23;

                case InputKey.F24:
                    return Scancode.ScancodeF24;

                case InputKey.Up:
                    return Scancode.ScancodeUp;

                case InputKey.Down:
                    return Scancode.ScancodeDown;

                case InputKey.Left:
                    return Scancode.ScancodeLeft;

                case InputKey.Right:
                    return Scancode.ScancodeRight;

                case InputKey.Enter:
                    return Scancode.ScancodeReturn;

                case InputKey.Escape:
                    return Scancode.ScancodeEscape;

                case InputKey.Space:
                    return Scancode.ScancodeSpace;

                case InputKey.Tab:
                    return Scancode.ScancodeTab;

                case InputKey.BackSpace:
                    return Scancode.ScancodeBackspace;

                case InputKey.Insert:
                    return Scancode.ScancodeInsert;

                case InputKey.Delete:
                    return Scancode.ScancodeDelete;

                case InputKey.PageUp:
                    return Scancode.ScancodePageup;

                case InputKey.PageDown:
                    return Scancode.ScancodePagedown;

                case InputKey.Home:
                    return Scancode.ScancodeHome;

                case InputKey.End:
                    return Scancode.ScancodeEnd;

                case InputKey.CapsLock:
                    return Scancode.ScancodeCapslock;

                case InputKey.ScrollLock:
                    return Scancode.ScancodeScrolllock;

                case InputKey.PrintScreen:
                    return Scancode.ScancodePrintscreen;

                case InputKey.Pause:
                    return Scancode.ScancodePause;

                case InputKey.NumLock:
                    return Scancode.ScancodeNumlockclear;

                case InputKey.Sleep:
                    return Scancode.ScancodeSleep;

                case InputKey.Keypad0:
                    return Scancode.ScancodeKP0;

                case InputKey.Keypad1:
                    return Scancode.ScancodeKP1;

                case InputKey.Keypad2:
                    return Scancode.ScancodeKP2;

                case InputKey.Keypad3:
                    return Scancode.ScancodeKP3;

                case InputKey.Keypad4:
                    return Scancode.ScancodeKP4;

                case InputKey.Keypad5:
                    return Scancode.ScancodeKP5;

                case InputKey.Keypad6:
                    return Scancode.ScancodeKP6;

                case InputKey.Keypad7:
                    return Scancode.ScancodeKP7;

                case InputKey.Keypad8:
                    return Scancode.ScancodeKP8;

                case InputKey.Keypad9:
                    return Scancode.ScancodeKP9;

                case InputKey.KeypadDivide:
                    return Scancode.ScancodeKPDivide;

                case InputKey.KeypadMultiply:
                    return Scancode.ScancodeKPMultiply;

                case InputKey.KeypadMinus:
                    return Scancode.ScancodeKPMinus;

                case InputKey.KeypadPlus:
                    return Scancode.ScancodeKPPlus;

                case InputKey.KeypadPeriod:
                    return Scancode.ScancodeKPPeriod;

                case InputKey.KeypadEnter:
                    return Scancode.ScancodeKPEnter;

                case InputKey.A:
                    return Scancode.ScancodeA;

                case InputKey.B:
                    return Scancode.ScancodeB;

                case InputKey.C:
                    return Scancode.ScancodeC;

                case InputKey.D:
                    return Scancode.ScancodeD;

                case InputKey.E:
                    return Scancode.ScancodeE;

                case InputKey.F:
                    return Scancode.ScancodeF;

                case InputKey.G:
                    return Scancode.ScancodeG;

                case InputKey.H:
                    return Scancode.ScancodeH;

                case InputKey.I:
                    return Scancode.ScancodeI;

                case InputKey.J:
                    return Scancode.ScancodeJ;

                case InputKey.K:
                    return Scancode.ScancodeK;

                case InputKey.L:
                    return Scancode.ScancodeL;

                case InputKey.M:
                    return Scancode.ScancodeM;

                case InputKey.N:
                    return Scancode.ScancodeN;

                case InputKey.O:
                    return Scancode.ScancodeO;

                case InputKey.P:
                    return Scancode.ScancodeP;

                case InputKey.Q:
                    return Scancode.ScancodeQ;

                case InputKey.R:
                    return Scancode.ScancodeR;

                case InputKey.S:
                    return Scancode.ScancodeS;

                case InputKey.T:
                    return Scancode.ScancodeT;

                case InputKey.U:
                    return Scancode.ScancodeU;

                case InputKey.V:
                    return Scancode.ScancodeV;

                case InputKey.W:
                    return Scancode.ScancodeW;

                case InputKey.X:
                    return Scancode.ScancodeX;

                case InputKey.Y:
                    return Scancode.ScancodeY;

                case InputKey.Z:
                    return Scancode.ScancodeZ;

                case InputKey.Number0:
                    return Scancode.Scancode0;

                case InputKey.Number1:
                    return Scancode.Scancode1;

                case InputKey.Number2:
                    return Scancode.Scancode2;

                case InputKey.Number3:
                    return Scancode.Scancode3;

                case InputKey.Number4:
                    return Scancode.Scancode4;

                case InputKey.Number5:
                    return Scancode.Scancode5;

                case InputKey.Number6:
                    return Scancode.Scancode6;

                case InputKey.Number7:
                    return Scancode.Scancode7;

                case InputKey.Number8:
                    return Scancode.Scancode8;

                case InputKey.Number9:
                    return Scancode.Scancode9;

                case InputKey.Grave:
                    return Scancode.ScancodeGrave;

                case InputKey.Minus:
                    return Scancode.ScancodeMinus;

                case InputKey.Plus:
                    return Scancode.ScancodeEquals;

                case InputKey.BracketLeft:
                    return Scancode.ScancodeLeftbracket;

                case InputKey.BracketRight:
                    return Scancode.ScancodeRightbracket;

                case InputKey.Semicolon:
                    return Scancode.ScancodeSemicolon;

                case InputKey.Quote:
                    return Scancode.ScancodeApostrophe;

                case InputKey.Comma:
                    return Scancode.ScancodeComma;

                case InputKey.Period:
                    return Scancode.ScancodePeriod;

                case InputKey.Slash:
                    return Scancode.ScancodeSlash;

                case InputKey.BackSlash:
                    return Scancode.ScancodeBackslash;

                case InputKey.NonUSBackSlash:
                    return Scancode.ScancodeNonusbackslash;

                case InputKey.Mute:
                    return Scancode.ScancodeAudiomute;

                case InputKey.PlayPause:
                    return Scancode.ScancodeAudioplay;

                case InputKey.Stop:
                    return Scancode.ScancodeAudiostop;

                case InputKey.VolumeUp:
                    return Scancode.ScancodeVolumeup;

                case InputKey.VolumeDown:
                    return Scancode.ScancodeVolumedown;

                case InputKey.TrackPrevious:
                    return Scancode.ScancodeAudioprev;

                case InputKey.TrackNext:
                    return Scancode.ScancodeAudionext;

                case InputKey.LShift:
                    return Scancode.ScancodeLshift;

                case InputKey.RShift:
                    return Scancode.ScancodeRshift;

                case InputKey.LControl:
                    return Scancode.ScancodeLctrl;

                case InputKey.RControl:
                    return Scancode.ScancodeRctrl;

                case InputKey.LAlt:
                    return Scancode.ScancodeLalt;

                case InputKey.RAlt:
                    return Scancode.ScancodeRalt;

                case InputKey.LSuper:
                    return Scancode.ScancodeLgui;

                case InputKey.RSuper:
                    return Scancode.ScancodeRgui;
            }
        }

        public static WindowState ToWindowState(this WindowFlags windowFlags)
        {
            if (windowFlags.HasFlagFast(WindowFlags.WindowFullscreenDesktop) ||
                windowFlags.HasFlagFast(WindowFlags.WindowBorderless))
                return WindowState.FullscreenBorderless;

            if (windowFlags.HasFlagFast(WindowFlags.WindowMinimized))
                return WindowState.Minimised;

            if (windowFlags.HasFlagFast(WindowFlags.WindowFullscreen))
                return WindowState.Fullscreen;

            if (windowFlags.HasFlagFast(WindowFlags.WindowMaximized))
                return WindowState.Maximised;

            return WindowState.Normal;
        }

        public static WindowFlags ToFlags(this WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return 0;

                case WindowState.Fullscreen:
                    return WindowFlags.WindowFullscreen;

                case WindowState.Maximised:
                    return WindowFlags.WindowMaximized;

                case WindowState.Minimised:
                    return WindowFlags.WindowMinimized;

                case WindowState.FullscreenBorderless:
                    return WindowFlags.WindowFullscreenDesktop;
            }

            return 0;
        }

        public static JoystickAxisSource ToJoystickAxisSource(this GameControllerAxis axis)
        {
            switch (axis)
            {
                default:
                case GameControllerAxis.ControllerAxisInvalid:
                    return 0;

                case GameControllerAxis.ControllerAxisLeftx:
                    return JoystickAxisSource.GamePadLeftStickX;

                case GameControllerAxis.ControllerAxisLefty:
                    return JoystickAxisSource.GamePadLeftStickY;

                case GameControllerAxis.ControllerAxisTriggerleft:
                    return JoystickAxisSource.GamePadLeftTrigger;

                case GameControllerAxis.ControllerAxisRightx:
                    return JoystickAxisSource.GamePadRightStickX;

                case GameControllerAxis.ControllerAxisRighty:
                    return JoystickAxisSource.GamePadRightStickY;

                case GameControllerAxis.ControllerAxisTriggerright:
                    return JoystickAxisSource.GamePadRightTrigger;
            }
        }

        public static JoystickButton ToJoystickButton(this GameControllerButton button)
        {
            switch (button)
            {
                default:
                case GameControllerButton.ControllerButtonInvalid:
                    return 0;

                case GameControllerButton.ControllerButtonA:
                    return JoystickButton.GamePadA;

                case GameControllerButton.ControllerButtonB:
                    return JoystickButton.GamePadB;

                case GameControllerButton.ControllerButtonX:
                    return JoystickButton.GamePadX;

                case GameControllerButton.ControllerButtonY:
                    return JoystickButton.GamePadY;

                case GameControllerButton.ControllerButtonBack:
                    return JoystickButton.GamePadBack;

                case GameControllerButton.ControllerButtonGuide:
                    return JoystickButton.GamePadGuide;

                case GameControllerButton.ControllerButtonStart:
                    return JoystickButton.GamePadStart;

                case GameControllerButton.ControllerButtonLeftstick:
                    return JoystickButton.GamePadLeftStick;

                case GameControllerButton.ControllerButtonRightstick:
                    return JoystickButton.GamePadRightStick;

                case GameControllerButton.ControllerButtonLeftshoulder:
                    return JoystickButton.GamePadLeftShoulder;

                case GameControllerButton.ControllerButtonRightshoulder:
                    return JoystickButton.GamePadRightShoulder;

                case GameControllerButton.ControllerButtonDpadUp:
                    return JoystickButton.GamePadDPadUp;

                case GameControllerButton.ControllerButtonDpadDown:
                    return JoystickButton.GamePadDPadDown;

                case GameControllerButton.ControllerButtonDpadLeft:
                    return JoystickButton.GamePadDPadLeft;

                case GameControllerButton.ControllerButtonDpadRight:
                    return JoystickButton.GamePadDPadRight;
            }
        }
    }
}
