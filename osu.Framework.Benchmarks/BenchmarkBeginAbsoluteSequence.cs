// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Framework.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkBeginAbsoluteSequence : GameBenchmark
    {
        private TestGame game;

        [Test]
        [Benchmark]
        public void NonRecursive()
        {
            game.Schedule(() =>
            {
                using (game.Flat.BeginAbsoluteSequence(1000, false))
                {
                }
            });

            RunSingleFrame();
        }

        [Test]
        [Benchmark]
        public void Recursive()
        {
            game.Schedule(() =>
            {
                using (game.Flat.BeginAbsoluteSequence(1000, true))
                {
                }
            });

            RunSingleFrame();
        }

        [Test]
        [Benchmark]
        public void SlightlyNestedNonRecursive()
        {
            game.Schedule(() =>
            {
                using (game.SlightlyNested.BeginAbsoluteSequence(1000, false))
                {
                }
            });

            RunSingleFrame();
        }

        [Test]
        [Benchmark]
        public void SlightlyNestedRecursive()
        {
            game.Schedule(() =>
            {
                using (game.SlightlyNested.BeginAbsoluteSequence(1000, true))
                {
                }
            });

            RunSingleFrame();
        }

        [Test]
        [Benchmark]
        public void VeryNestedNonRecursive()
        {
            game.Schedule(() =>
            {
                using (game.VeryNested.BeginAbsoluteSequence(1000, false))
                {
                }
            });

            RunSingleFrame();
        }

        [Test]
        [Benchmark]
        public void VeryNestedRecursive()
        {
            game.Schedule(() =>
            {
                using (game.VeryNested.BeginAbsoluteSequence(1000, true))
                {
                }
            });

            RunSingleFrame();
        }

        protected override Game CreateGame() => game = new TestGame();

        private class TestGame : Game
        {
            public Container Flat;
            public Container VeryNested;
            public Container SlightlyNested;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Add(Flat = new Container());

                for (int i = 0; i < 1000; i++)
                {
                    var box = new Box
                    {
                        Size = new Vector2(100),
                        Colour = Colour4.Black
                    };

                    Flat.Add(box);
                }

                Add(SlightlyNested = new Container());

                for (int i = 0; i < 1000; i++)
                {
                    SlightlyNested.Add(new Container
                    {
                        Size = new Vector2(100),
                        Colour = Colour4.Black,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = Colour4.Black,
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    });
                }

                Add(VeryNested = new Container());

                Container target = VeryNested;

                for (int i = 0; i < 1000; i++)
                {
                    var container = new Container { Size = new Vector2(100), Colour = Colour4.Black };

                    target.Add(container);

                    target = container;
                }
            }
        }
    }
}
