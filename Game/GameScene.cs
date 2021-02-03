using Godot;
using System;
using System.Collections.Generic;

namespace A_NEAT_arena.Game
{
    public class GameScene : Node2D
    {
        [Export] public float LaserSpeed;

        private Camera2D Camera;
        private PlayArea PlayArea;
        private SetupOptions SetupOptions;
        private Label ScoreLabel;

        private List<BaseRunner> Runners;

        public GameScene() : base() 
        {
            Runners = new List<BaseRunner>();    
        }


        public override void _Ready()
        {
            GetTree().Paused = true;

            // Nodes
            Camera = GetNode<Camera2D>("MainCamera");
            PlayArea = GetNode<PlayArea>("PlayArea");
            SetupOptions = GetNode<SetupOptions>("MainCamera/HUD/SetupOptions");
            ScoreLabel = GetNode<Label>("MainCamera/HUD/ScoreLabel");

            // Signals and events
            SetupOptions.StartEvent += OnStart;

            PlayArea.CourseSegments = SetupOptions.LoadedSegments;
            PlayArea.GameOverEvent += OnGameOver;

            Camera.MakeCurrent();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _PhysicsProcess(float delta)
        {
            if (Runners.Count > 0)
            {
                Runners.Sort(RunnerComparer);
                Camera.Position = new Vector2(Runners[0].Position.x - 960, 0);
                ScoreLabel.Text = Runners[0].Score.ToString("F0");
            }
        }

        private async void OnStart()
        {
            var runner = Preloads.PlayerRunner.Instance() as PlayerRunner;
            Runners.Add(runner);
            PlayArea.LaserSpeed = 100;

            PlayArea.LaserSpeed = SetupOptions.LaserSpeed;
            PlayArea.LaserAcc = SetupOptions.LaserAcc;

            await PlayArea.Restart(Runners, SetupOptions.Seed);

            GetTree().Paused = false;
        }


        private void OnGameOver()
        {
            Camera.Position = new Vector2();
            SetupOptions.Show();

        }

        #region Utilities
        private enum CameraFollowMode
        {
            Best = 1,
            Laser = 2,
            Worst = 3,
        }

        private enum RunnerMode
        {
            NEAT = 0,
            Player = 1
        }

        private static int RunnerComparer(BaseRunner x, BaseRunner y)
        {
            return (int)(x.Score - y.Score);
        }
        #endregion
    }
}
