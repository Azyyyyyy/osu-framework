﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;

namespace osu.Framework.Input.Handlers
{
    /// <summary>
    /// An input handler which needs to get a feedback of final mouse position.
    /// </summary>
    public interface INeedsMousePositionFeedback
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        void FeedbackMousePositionChange(Vector2 position);
    }
}
