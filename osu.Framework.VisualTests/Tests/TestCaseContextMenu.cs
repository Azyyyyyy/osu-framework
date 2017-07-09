﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;

namespace osu.Framework.VisualTests.Tests
{
    internal class TestCaseContextMenu : TestCase
    {
        public override string Description => @"Menu visible on right click";

        private const int start_time = 0;
        private const int duration = 1000;
        private ContextMenuBox movingBox;

        private ContextMenuBox makeBox(Anchor anchor)
        {
            return new ContextMenuBox
            {
                Size = new Vector2(200),
                Anchor = anchor,
                Origin = anchor,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Blue,
                    }
                }
            };
        }

        public TestCaseContextMenu()
        {
            Add(new ContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    makeBox(Anchor.TopLeft),
                    makeBox(Anchor.TopRight),
                    makeBox(Anchor.BottomLeft),
                    makeBox(Anchor.BottomRight),
                    movingBox = makeBox(Anchor.Centre),
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            using (movingBox.BeginLoopedSequece())
            {
                movingBox.MoveTo(new Vector2(0, 100), duration);
                using (movingBox.BeginDelayedSequence(duration))
                {
                    movingBox.MoveTo(new Vector2(100, 100), duration);
                    using (movingBox.BeginDelayedSequence(duration))
                    {
                        movingBox.MoveTo(new Vector2(100, 0), duration);
                        using (movingBox.BeginDelayedSequence(duration))
                            movingBox.MoveTo(Vector2.Zero, duration);
                    }
                }
            }
        }

        private class ContextMenuBox : Container, IHasContextMenu
        {
            public ContextMenuItem[] ContextMenuItems => new []
            {
                new ContextMenuItem(@"Change width") { Action = () => ResizeWidthTo(Width * 2, 100, EasingTypes.OutQuint) },
                new ContextMenuItem(@"Change height") { Action = () => ResizeHeightTo(Height * 2, 100, EasingTypes.OutQuint) },
                new ContextMenuItem(@"Change width back") { Action = () => ResizeWidthTo(Width / 2, 100, EasingTypes.OutQuint) },
                new ContextMenuItem(@"Change height back") { Action = () => ResizeHeightTo(Height / 2, 100, EasingTypes.OutQuint) },
            };
        }
    }
}
