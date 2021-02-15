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

        public SceneTree Tree { get; private set; }
        public Camera2D Camera { get; private set; }
        public PlayArea PlayArea { get; private set; }
        private SetupOptions SetupOptions;
        private Label ScoreLabel;
        private NEATSettings NeatSettings;
        private Control GameHUD, SettingsHUD;
        private AcceptDialog Warning;

        public List<BaseRunner> Runners;
        //public List<PhenomePack> Boxes;

        private RunnerMode mode;
        private RunnerMode Mode
        {
            get => mode;
            set
            {
                switch (value)
                {
                    case RunnerMode.Player:
                        NeatSettings.Hide();
                        SetupOptions.IsEvolutionMode = false;
                        controller = pgc;
                        break;
                    case RunnerMode.AI:
                        NeatSettings.Show();
                        SetupOptions.IsEvolutionMode = true;
                        controller = neat;
                        break;
                    default:
                        throw new ArgumentException("RunnerMode value is not valid.");
                }
                mode = value;
            }
        }


        // game controllers
        private NeatController neat;
        private PlayerGameController pgc;
        // selected game controller
        private IGameController controller;

        public bool Stop { get; set; }

        public GameScene() : base()
        {
            Runners = new List<BaseRunner>();

            pgc = new PlayerGameController(this);
            neat = new NeatController(this);
        }

        public override void _Ready()
        {
            base._Ready();
            Tree = GetTree();
            Tree.Paused = true;

            // Nodes
            Camera = GetNode<Camera2D>("MainCamera");

            GameHUD = Camera.GetNode<Control>("GameHUD");
            ScoreLabel = GameHUD.GetNode<Label>("ScoreLabel");

            SettingsHUD = Camera.GetNode<Control>("SettingsHUD");
            SetupOptions = SettingsHUD.GetNode<SetupOptions>("SetupOptions");
            NeatSettings = SettingsHUD.GetNode<NEATSettings>("NEATSettings");
            Warning = SettingsHUD.GetNode<AcceptDialog>("WarningPopup");
            PlayArea = GetNode<PlayArea>("PlayArea");

            // Signals and events
            SetupOptions.StartEvent += On_StartButton_Pressed;
            PlayArea.GameOverEvent += OnGameOver;
            NeatSettings.ParametersSet += On_NeatSettings_ParametersSet;
            SetupOptions.GamemodeSwitched += On_SetupOptions_GamemodeSwitched;

            PlayArea.CourseSegments = SetupOptions.LoadedSegments;

            Camera.MakeCurrent();

            Mode = RunnerMode.Player;
        }


        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _PhysicsProcess(float delta)
        {
            controller.Process();
        }

        public void SetScore(int score)
        {
            ScoreLabel.Text = $"{score}";
        }

        public async void StartRun(List<BaseRunner> runners = null)
        {
            if (runners != null) Runners = runners;
            SettingsHUD.Hide();
            PlayArea.LaserSpeed = SetupOptions.LaserSpeed;
            PlayArea.LaserAcc = SetupOptions.LaserAcc;
            await PlayArea.Restart(Runners, SetupOptions.Seed);
            Tree.Paused = false;
        }

        #region Signals and events
        private void On_NeatSettings_ParametersSet(object sender, EventArgs e)
        {
            if (Mode != RunnerMode.AI) throw new Exception("NEAT parameters can only be set when game is in AI mode");

            Task.Run(async () => await PlayArea.Reset()).Wait();
            Tree.Paused = true;
            neat.InitializeNetwork(NeatSettings.NeatParams, NeatSettings.Population, NeatSettings.BatchSize);
        }

        private void On_NeatExperiment_GenerationEnded(object sender, EventArgs e)
        {
            //generation++;
        }

        

        private void On_StartButton_Pressed()
        {
            if(SetupOptions.LoadedSegments.Count <= 0)
            {
                ShowWarningPopup("Unable to start!", "No loaded course segments.");
                return;
            }
            if(Mode == RunnerMode.AI && NeatSettings.NeatParams == null)
            {
                ShowWarningPopup("Unable to start!", "NEAT parameters not set.");
                return;
            }

            controller.BeginGame();
        }

        private void OnGameOver()
        {
            controller.OnRoundEnded();
        }

        private void On_SetupOptions_GamemodeSwitched(object sender, EventArgs e)
        {
            Mode = SetupOptions.IsEvolutionMode ? RunnerMode.AI : RunnerMode.Player;
        }

        public void On_QuitButton_Pressed()
        {
            
        }

        #endregion

        #region Utilities

        private void ShowWarningPopup(string title, string message)
        {
            Warning.WindowTitle = title;
            Warning.DialogText = message;
            Warning.PopupCentered();
        }

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

        public static int RunnerComparer(BaseRunner x, BaseRunner y)
        {
            return (int)(y.Score - x.Score);
        }
        #endregion
    }
}
