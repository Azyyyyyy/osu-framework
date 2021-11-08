// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Framework.Tests.Visual.Containers
{
    public class TestSceneFrontToBackTriangle : FrameworkTestScene
    {
        public TestSceneFrontToBackTriangle()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.LightPink
                },
                new Triangle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(200),
                    Colour = Colour4.Red
                }
            };
        }
    }
}
