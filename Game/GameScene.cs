using Godot;
using System;
using System.Collections.Generic;
using SharpNeat.Phenomes;
using A_NEAT_arena.NEAT;
using SharpNeat.EvolutionAlgorithms;
using System.Threading.Tasks;

namespace A_NEAT_arena.Game
{
    public class GameScene : Node2D
    {
        [Export] public float LaserSpeed;

        private SceneTree _tree;
        private Camera2D Camera;
        public PlayArea PlayArea;
        private SetupOptions SetupOptions;
        private Label ScoreLabel;
        private NEATSettings NeatSettings;
        private Control GameHUD, SettingsHUD;

        public List<BaseRunner> Runners;
        //public List<PhenomePack> Boxes;
        private RunnerMode Mode;
        private int generation;
        private int batch;
        private int batchSize;

        private NeatController neat;
        private PlayerGameController pgc;

        private IGameController controller;

        public bool Stop { get; set; }

        public GameScene() : base()
        {
            Runners = new List<BaseRunner>();
            Mode = RunnerMode.Player;

            pgc = new PlayerGameController(this);
            neat = new NeatController(this);
            controller = pgc;
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

            // Signals and events
            SetupOptions.StartEvent += OnStart;
            PlayArea.GameOverEvent += OnGameOver;
            NeatSettings.ParametersSet += On_NeatSettings_ParametersSet;
            SetupOptions.GamemodeSwitched += On_SetupOptions_GamemodeSwitched;

            PlayArea.CourseSegments = SetupOptions.LoadedSegments;

            Camera.MakeCurrent();
        }


        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _PhysicsProcess(float delta)
        {
            controller.Process();
        }

        public async void StartRun()
        {
            switch (Mode)
            {
                case RunnerMode.Player:
                    Runners.Clear();
                    Runners.Add(Preloads.PlayerRunner.Instance() as PlayerRunner);
                    break;

                case RunnerMode.AI:
                    
                    batch++;
                    break;
            }

            SettingsHUD.Hide();
            PlayArea.LaserSpeed = SetupOptions.LaserSpeed;
            PlayArea.LaserAcc = SetupOptions.LaserAcc;
            await PlayArea.Restart(Runners, SetupOptions.Seed);
            _tree.Paused = false;
        }

        #region Signals and events
        private void On_NeatSettings_ParametersSet(object sender, EventArgs e)
        {
            if (Mode != RunnerMode.AI) throw new Exception("NEAT parameters can only be set when game is in AI mode");

            Task.Run(async () => await PlayArea.Reset());
            _tree.Paused = true;
            neat.InitializeNetwork(NeatSettings.NeatParams, NeatSettings.Population, NeatSettings.Population / NeatSettings.Batches);
        }

        private void On_NeatExperiment_GenerationEnded(object sender, EventArgs e)
        {
            //generation++;
        }

        

        private void OnStart()
        {
            controller.BeginGame();

        }

        private void OnGameOver()
        {
            controller.OnRoundEnded();
        }

        private void On_SetupOptions_GamemodeSwitched(object sender, EventArgs e)
        {
            if (((CheckButton)sender).Pressed) // Evolution mode
            {
                controller = neat;
                Mode = RunnerMode.AI;
                NeatSettings.Show();
            }
            else // player mode
            {
                controller = pgc;
                Mode = RunnerMode.Player;
                NeatSettings.Hide();
            }
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
            return (int)(y.Score - x.Score);
        }
        #endregion
    }
}
