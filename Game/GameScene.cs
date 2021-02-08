using Godot;
using System;
using System.Collections.Generic;
using SharpNeat.Phenomes;
using A_NEAT_arena.NEAT;
using SharpNeat.EvolutionAlgorithms;

namespace A_NEAT_arena.Game
{
    public class GameScene : Node2D
    {
        [Export] public float LaserSpeed;

        private SceneTree _tree;
        private Camera2D Camera;
        private PlayArea PlayArea;
        private SetupOptions SetupOptions;
        private Label ScoreLabel;
        private NEATSettings NeatSettings;
        private Control GameHUD, SettingsHUD;

        private List<BaseRunner> Runners;
        public List<PhenomePack> Boxes;
        private RunnerMode Mode;
        private int generation;
        private int batch;
        private int batchSize;

        private NeatExperiment neat;

        public bool Stop { get; set; }

        public GameScene() : base()
        {
            Runners = new List<BaseRunner>();
            Boxes = new List<PhenomePack>();
            Mode = RunnerMode.Player;
        }

        public override void _Ready()
        {
            base._Ready();
            _tree = GetTree();
            _tree.Paused = true;

            // Nodes
            Camera = GetNode<Camera2D>("MainCamera");

            GameHUD = Camera.GetNode<Control>("GameHUD");
            ScoreLabel = GameHUD.GetNode<Label>("ScoreLabel");

            SettingsHUD = Camera.GetNode<Control>("SettingsHUD");
            SetupOptions = SettingsHUD.GetNode<SetupOptions>("SetupOptions");
            NeatSettings = SettingsHUD.GetNode<NEATSettings>("NEATSettings");

            PlayArea = GetNode<PlayArea>("PlayArea");

            PlayArea.CourseSegments = SetupOptions.LoadedSegments;

            // Signals and events
            SetupOptions.StartEvent += OnStart;
            PlayArea.GameOverEvent += OnGameOver;
            NeatSettings.ParametersSet += On_NeatSettings_ParametersSet;

            Camera.MakeCurrent();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _PhysicsProcess(float delta)
        {
            if (PlayArea.Runners.Count > 0)
            {
                PlayArea.Runners.Sort(RunnerComparer);
                Camera.Position = new Vector2(PlayArea.Runners[0].Position.x - 960, 0);
                ScoreLabel.Text = PlayArea.Runners[0].Score.ToString("F0");
            }
        }

        public ANNRunner AddANNRunner(IBlackBox box)
        {
            var runner = Preloads.ANNRunner.Instance() as ANNRunner;
            //runner.Init(box);
            Runners.Add(runner);
            StartRun();
            return runner;
        }

        public void AddANNRunner(ANNRunner runner)
        {
            Runners.Add(runner);
        }

        public void AddPhenomePack(PhenomePack pack)
        {
            Boxes.Add(pack);
            StartRun();
        }

        private async void StartRun()
        {
            var newRunners = new List<BaseRunner>();

            switch (Mode)
            {
                case RunnerMode.Player:
                    newRunners.Add(Preloads.PlayerRunner.Instance() as PlayerRunner);
                    break;

                case RunnerMode.AI:
                    if (Runners.Count != NeatSettings.Population) return;

                    var newBoxes = Boxes.GetRange(batch * batchSize, batchSize);
                    foreach (var b in newBoxes)
                    {
                        var r = Preloads.ANNRunner.Instance() as ANNRunner;
                        r.Init(b);
                        newRunners.Add(r);
                    }
                    batch++;
                    break;
            }

            SettingsHUD.Hide();
            PlayArea.LaserSpeed = SetupOptions.LaserSpeed;
            PlayArea.LaserAcc = SetupOptions.LaserAcc;
            await PlayArea.Restart(newRunners, SetupOptions.Seed);
            _tree.Paused = false;
        }

        #region Signals and events
        private async void On_NeatSettings_ParametersSet(object sender, EventArgs e)
        {
            _tree.Paused = true;
            await PlayArea.Reset();

            //var eaparams = new NeatEvolutionAlgorithmParameters()
            //{
            //    SpecieCount = NeatSettings.SpeciesNum
            //};

            Stop = false;
            neat = new NeatExperiment(this, NeatSettings.NeatParams, NeatSettings.Population);
            neat.InitializeNetwork();
            neat.GenerationEnded += On_NeatExperiment_GenerationEnded;

            batch = 0;
            batchSize = NeatSettings.Population / NeatSettings.Batches;
            generation = 0;
        }

        private void On_NeatExperiment_GenerationEnded(object sender, EventArgs e)
        {
            generation++;
        }

        private void OnStart()
        {
            switch (Mode)
            {
                case RunnerMode.Player:
                    StartRun();
                    break;
                case RunnerMode.AI:
                    neat.StartContinue();
                    break;
            }

        }

        private void OnGameOver()
        {
            switch (Mode)
            {
                case RunnerMode.Player:
                    StartRun();
                    break;
                case RunnerMode.AI:
                    StartRun();
                    break;
            }
            Camera.Position = new Vector2();
            //SettingsHUD.Show();
        }
        #endregion

        #region Utilities
        private enum CameraFollowMode
        {
            Best = 1,
            Laser = 2,
            Worst = 3,
        }

        private enum RunnerMode
        {
            AI = 0,
            Player = 1
        }

        private static int RunnerComparer(BaseRunner x, BaseRunner y)
        {
            return (int)(x.Score - y.Score);
        }
        #endregion
    }
}
