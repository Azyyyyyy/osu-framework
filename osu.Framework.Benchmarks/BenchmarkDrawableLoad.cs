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
    public class BenchmarkDrawableLoad : GameBenchmark
    {
        private TestGame game;

        private const int nesting_level = 100;

        [Test]
        [Benchmark]
        public void NonRecursive()
        {
            game.Schedule(() =>
            {
                Container container = new Container();

                for (int i = 0; i < nesting_level; i++)
                {
                    var box = new Box
                    {
                        Size = new Vector2(100),
                        Colour = Colour4.Black
                    };

                    container.Add(box);
                }

                game.Clear();
                game.Add(container);
            });

            RunSingleFrame();
        }

        [Test]
        [Benchmark]
        public void SlightlyNested()
        {
            game.Schedule(() =>
            {
                Container container = new Container();

                for (int i = 0; i < nesting_level; i++)
                {
                    container.Add(new Container
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

                game.Clear();
                game.Add(container);
            });

            RunSingleFrame();
        }

        [Test]
        [Benchmark]
        public void VeryNested()
        {
            game.Schedule(() =>
            {
                Container container = new Container();
                Container target = container;

                for (int i = 0; i < nesting_level; i++)
                {
                    var newContainer = new Container { Size = new Vector2(100), Colour = Colour4.Black };

                    target.Add(newContainer);
                    target = newContainer;
                }

                game.Clear();
                game.Add(container);
            });

            RunSingleFrame();
        }

        protected override Game CreateGame() => game = new TestGame();

        private class TestGame : Game
        {
        }
    }
}
