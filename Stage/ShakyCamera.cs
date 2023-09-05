using Godot;
using System;

namespace Hopper
{
    public class ShakyCamera : Camera2D
    {
        [Export]
        public float Decay = 0.8f;  // How quickly the shaking stops [0, 1].

        [Export]
        public Vector2 MaxOffset = new Vector2(10, 3);  // Maximum hor/ver shake in pixels.

        [Export]
        public float MaxRoll = 0.1f;  // Maximum rotation in radians (use sparingly).

        public float Trauma = 0f;  // Current shake strength.
        public float TraumaPower = 1.5f;  // Trauma exponent. Use [2, 3].

        private int NoiseY = 0;

        public RandomNumberGenerator Randomizer;
        private OpenSimplexNoise NoiseGen;

        public override void _Ready()
        {
            base._Ready();
            Randomizer = new RandomNumberGenerator();
            Randomizer.Randomize();

            NoiseGen = new OpenSimplexNoise
            {
                Seed = (int)Randomizer.Randi(),
                Period = 4,
                Octaves = 2
            };
        }

        public override void _Process(float delta)
        {
            if (Trauma > 0f)
            {
                Trauma = Mathf.Max(Trauma - Decay * delta, 0f);
                Shake();
            }
        }

        public void ApplyTrauma(float Amount)
        {
            Trauma = Mathf.Min(Trauma + Amount, 1.0f);
        }

        private void Shake()
        {
            NoiseY += 1;
            float Amount = Mathf.Pow(Trauma, TraumaPower);

            float Rand1 = NoiseGen.GetNoise2d(NoiseGen.Seed, NoiseY);
            float Rand2 = NoiseGen.GetNoise2d(NoiseGen.Seed * 2, NoiseY);
            float Rand3 = NoiseGen.GetNoise2d(NoiseGen.Seed * 3, NoiseY);

            Rotation = MaxRoll * Amount * Rand1;
            Offset = new Vector2(MaxOffset.x * Amount * Rand2,
                                MaxOffset.y * Amount * Rand3);
        }
    }
}

