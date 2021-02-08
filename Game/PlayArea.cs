using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace A_NEAT_arena.Game
{
    public class PlayArea : Node2D
    {
        [Signal] public delegate void GameOver();
        public event GameOver GameOverEvent;

        public List<BaseRunner> Runners { get; set; }
        public List<PackedScene> CourseSegments { get; set; }

        public float Time { get; set; }
        public float LaserSpeed { get; set; }
        public float LaserAcc { get; set; }

        private List<Segment> Course { get; set; }
        private PackedScene Start;
        private RandomNumberGenerator Gen;
        private bool IsReset;

        private Area2D RunnerDetector;
        private Node2D DeathLaser;

        public PlayArea() : base()
        {
            Gen = new RandomNumberGenerator();
            Runners = new List<BaseRunner>();
            Course = new List<Segment>();
            CourseSegments = new List<PackedScene>();
            IsReset = false;
        }

        // Called when the node enters the scene tree for the first time.
        public override async void _Ready()
        {
            Start = GD.Load<PackedScene>("res://Game/Start.tscn");
            RunnerDetector = GetNode<Area2D>("RunnerDetector");
            DeathLaser = GetNode<Node2D>("DeathLaser");

            await Reset();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _PhysicsProcess(float delta)
        {
            Time += delta;
            DeathLaser.Position = new Vector2(DeathLaser.Position.x + LaserSpeed * delta, 0);
            LaserSpeed += LaserAcc * delta;
        }

        /// <summary>
        /// Restarts the runner
        /// </summary>
        /// <param name="runners">New batch of runners</param>
        /// <param name="seed">Course generation seed</param>
        /// <returns>Executable task</returns>
        public async Task Restart(List<BaseRunner> runners, ulong seed)
        {
            Gen.Seed = seed;
            Runners = runners;

            await Reset();
            await GenerateCourse(2);

            var pos = Course[0].Flag.Position + new Vector2(30, 30);
            foreach (var runner in runners)
            {
                runner.Position = pos;
                CallDeferred("add_child", runner);
                await ToSignal(runner, "ready");
                runner.DiedEvent += OnRunnerDied;
            }
        }

        /// <summary>
        /// Generates a new course segment from the <c>CourseSegments</c> List.
        /// </summary>
        /// <param name="rep">Amount of course segments to generate</param>
        /// <exception cref="System.Exception">Thrown when <c>CourseSegments</c> is empty.</exception>
        private async Task GenerateCourse(int rep = 1)
        {
            if (CourseSegments.Count == 0) throw new Exception("No course segments given.");

            for (; rep >= 0; rep--)
            {
                uint genidx = Gen.Randi() % (uint)CourseSegments.Count;
                GD.Print($"Loaded course segments: {CourseSegments.Count}, selected: {genidx}");

                var seg = CourseSegments[(int)genidx].Instance() as Segment;

                var last = Course[Course.Count - 1];
                var pos = new Vector2(last.Position.x + last.UsedRect.End.x * 60, 0);

                seg.Position = pos;
                Course.Add(seg);
                seg.FreedEvent += OnSegmentFreed;
                CallDeferred("add_child", seg);
                await ToSignal(seg, "ready");

                IsReset = false;
            }
        }

        /// <summary>
        /// Resets the PlayArea
        /// </summary>
        public async Task Reset()
        {
            if (IsReset) return;

            Time = 0f;

            Course.ForEach(s => s.QueueFree());
            Course.Clear();


            var start = Start.Instance() as Segment;
            start.Position = new Vector2(0, 0);
            start.FreedEvent += OnSegmentFreed;
            Course.Add(start);
            CallDeferred("add_child", start);
            await ToSignal(start, "ready");

            RunnerDetector.Position = new Vector2(1920, 0);
            DeathLaser.Position = new Vector2(-30, 0);

            IsReset = true;
        }

        /// <summary>
        /// Marks nodes of type <c>Segment</c> as  obsolete,
        /// so that they can be deleted, when they leave the viewport.
        /// </summary>
        /// <param name="body">Obsolete node</param>
        public void OnBeamBodyExited(Node body)
        {
            var type = body.GetType();
            if (type == typeof(TileMap))
            {
                body.GetParent<Segment>().IsObsolete = true;
            }
            else if (type.IsSubclassOf(typeof(BaseRunner)))
            {
                GD.Print("player");
                (body as BaseRunner).Die(this);
            }
        }

        /// <summary>
        /// Detects when a runner is nearing the end of the course.
        /// Adds a new segment to the course and moves the RunnerDetector.
        /// </summary>
        /// <param name="body">Trigger node</param>
        public async void OnRunnerDetectorBodyEntered(Node body)
        {
            if (body.GetType().IsSubclassOf(typeof(BaseRunner)))
            {
                await GenerateCourse();
                var pos = Course[Course.Count - 2].Position;
                RunnerDetector.Position = pos;
            }
        }

        private void OnSegmentFreed(Segment sender)
        {
            Course.Remove(sender);
        }

        private async void OnRunnerDied(BaseRunner runner)
        {
            if (runner.GetType() != typeof(PlayerRunner))
            {

            }

            Runners.Remove(runner);
            runner.QueueFree();

            if (Runners.Count < 1)
            {
                await Reset();
                GetTree().Paused = true;
                GameOverEvent?.Invoke();
            }
        }
    }
}