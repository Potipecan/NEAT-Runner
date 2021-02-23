using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace A_NEAT_arena.Game
{
    public class PlayArea : Node2D
    {
        public static float LaserPosition;

        [Signal] public delegate void GameOver();
        public event GameOver GameOverEvent;

        [Export] public float Timeout { get; set; }

        private delegate void KillSignal(BaseRunner.CauseOfDeath cod);
        private event KillSignal KillAllRunners;

        public List<BaseRunner> Runners { get; set; }
        public List<PackedScene> CourseSegments { get; set; }

        public float Time { get; set; }
        public float LaserSpeed { get; set; }
        public float LaserAcc { get; set; }

        private List<Segment> Course { get; set; }
        private PackedScene Start;
        private RandomNumberGenerator Gen;
        private bool IsReset;
        private CourseGenerationStatus GenStatus;

        private SemaphoreSlim RunnerDeletionManager;

        private Area2D RunnerDetector;
        private Node2D DeathLaser;
        private Godot.Timer BatchTimer;

        public PlayArea() : base()
        {
            Gen = new RandomNumberGenerator();
            Runners = new List<BaseRunner>();
            Course = new List<Segment>();
            CourseSegments = new List<PackedScene>();
            IsReset = false;
            GenStatus = CourseGenerationStatus.Free;
            RunnerDeletionManager = new SemaphoreSlim(1, 1);
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Start = GD.Load<PackedScene>("res://Game/Start.tscn");
            RunnerDetector = GetNode<Area2D>("RunnerDetector");
            DeathLaser = GetNode<Node2D>("Beam");
            BatchTimer = GetNode<Godot.Timer>("BatchTimer");

            Reset();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _PhysicsProcess(float delta)
        {
            Time += delta;
            DeathLaser.Position = new Vector2(DeathLaser.Position.x + LaserSpeed * delta, 0);
            UpdateLaserPosition();
            LaserSpeed += LaserAcc * delta;
        }

        /// <summary>
        /// Restarts the runner
        /// </summary>
        /// <param name="runners">New batch of runners</param>
        /// <param name="seed">Course generation seed</param>
        /// <returns>Executable task</returns>
        public void Restart(List<BaseRunner> runners, ulong seed)
        {
            Gen.Seed = seed;
            Runners = runners;

            Reset();
            GenerateCourse(2);

            BatchTimer.Start();

            var pos = Course[0].Flag.Position + new Vector2(30, 30);
            int spawnedcnt = 0;
            foreach (var runner in Runners)
            {
                runner.Position = pos;
                KillAllRunners += runner.Die;
                CallDeferred("add_child", runner);
                //await ToSignal(runner, "ready");
                spawnedcnt++;
                runner.DiedEvent += OnRunnerDied;
            }
            //GD.Print($"Runner count: {Runners.Count}, Spawned: {spawnedcnt}");

        }

        /// <summary>
        /// Generates a new course segment from the <c>CourseSegments</c> List.
        /// </summary>
        /// <param name="rep">Amount of course segments to generate</param>
        /// <exception cref="System.Exception">Thrown when <c>CourseSegments</c> is empty.</exception>
        private void GenerateCourse(int rep = 1)
        {
            //if (GenStatus == CourseGenerationStatus.Busy) return;
            if (CourseSegments.Count == 0) throw new Exception("No course segments given.");


            for (; rep >= 0; rep--)
            {
                uint genidx = Gen.Randi() % (uint)CourseSegments.Count;

                var seg = CourseSegments[(int)genidx].Instance() as Segment;

                var last = Course[Course.Count - 1];
                var pos = new Vector2(last.Position.x + last.UsedRect.End.x * 60, 0);

                seg.Position = pos;
                Course.Add(seg);
                seg.FreedEvent += OnSegmentFreed;
                CallDeferred("add_child", seg);
                //await ToSignal(seg, "ready");

                IsReset = false;
            }
        }

        /// <summary>
        /// Resets the PlayArea
        /// </summary>
        public void Reset()
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
            //await ToSignal(start, "ready");

            RunnerDetector.Position = new Vector2(1920, 0);
            DeathLaser.Position = new Vector2(-30, 0);

            BatchTimer.Stop();
            BatchTimer.WaitTime = Timeout;

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
        }

        /// <summary>
        /// Detects when a runner is nearing the end of the course.
        /// Adds a new segment to the course and moves the RunnerDetector.
        /// </summary>
        /// <param name="body">Trigger node</param>
        public void OnRunnerDetectorBodyEntered(Node body)
        {
            if (body.GetType().IsSubclassOf(typeof(BaseRunner)))
            {
                if (GenStatus != CourseGenerationStatus.Free) return;
                GenStatus = CourseGenerationStatus.Busy;

                GenerateCourse();
                var pos = Course[Course.Count - 2].Position;
                RunnerDetector.Position = pos;

                GenStatus = CourseGenerationStatus.Free;
            }
        }

        private void On_Beam_BodyEntered(Node body)
        {
            if (body.GetType().IsSubclassOf(typeof(BaseRunner))) (body as BaseRunner).Die(BaseRunner.CauseOfDeath.Laser);
        }

        private void OnSegmentFreed(Segment sender)
        {
            Course.Remove(sender);
        }

        private void OnRunnerDied(BaseRunner runner)
        {

            Runners.Remove(runner);
            KillAllRunners -= runner.Die;

            if (Runners.Count < 1)
            {
                GetTree().Paused = true;

                Reset();
                GameOverEvent?.Invoke();
            }

        }

        public void On_BatchTimer_Timeout()
        {
            GetTree().Paused = true;
            KillAllRunners?.Invoke(BaseRunner.CauseOfDeath.Timeout);
        }

        private void UpdateLaserPosition()
        {
            LaserPosition = DeathLaser.GlobalPosition.x;
        }

        public enum CourseGenerationStatus
        {
            Free = 0,
            Busy = 1
        }
    }
}