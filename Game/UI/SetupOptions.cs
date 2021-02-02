using Godot;
using System.Collections.Generic;
using System;

namespace A_NEAT_arena.Game
{
    public class SetupOptions : ColorRect
    {
        public delegate void SegmentsLoaded();
        public event SegmentsLoaded SegmentsLoadedEvent;

        public delegate void Start();
        public event Start StartEvent;

        private ItemList SegmentList;
        private FileDialog SegmentLoader;
        private Button StartButton;
        private Button RandomizeButton;
        private SpinBox SeedBox;
        private SpinBox LSpeedInput;
        private SpinBox LAccInput;

        private List<string> SegmentPaths;

        private bool isSeedRandom;
        private bool IsSeedRandom
        {
            get => isSeedRandom;
            set
            {
                isSeedRandom = value;
                SeedBox.Editable = !value;
                RandomizeButton.Disabled = value;
            }
        }

        private RandomNumberGenerator SeedGen;

        public ulong Seed
        {
            get
            {
                if (IsSeedRandom) SeedBox.Value = SeedGen.Randi();
                return (ulong)SeedBox.Value;
            }
        }
        public float LaserSpeed { get => (float)LSpeedInput.Value; }
        public float LaserAcc { get => (float)LAccInput.Value; }
        public List<PackedScene> LoadedSegments { get; set; }

        public SetupOptions() : base()
        {
            LoadedSegments = new List<PackedScene>();
            SegmentPaths = new List<string>();
            isSeedRandom = false;
            SeedGen = new RandomNumberGenerator();
            SeedGen.Seed = (ulong)DateTime.Now.Ticks;
        }

        public override void _Ready()
        {
            base._Ready();

            SegmentList = GetNode<ItemList>("LoadedSegments");
            SegmentLoader = GetNode<FileDialog>("SegmentLoader");
            StartButton = GetNode<Button>("StartButton");
            SeedBox = GetNode<SpinBox>("SeedInput");
            RandomizeButton = SeedBox.GetNode<Button>("RandomizeButton");
            LSpeedInput = GetNode<SpinBox>("LaserSpeedInput");
            LAccInput = GetNode<SpinBox>("LaserAccInput");

            SeedBox.Value = SeedGen.Randi();
        }

        public void OnLoadButtonPressed()
        {
            SegmentLoader.PopupCentered();
        }

        public void OnDeleteButtonPressed()
        {
            if (SegmentList.IsAnythingSelected())
            {
                int[] ids = SegmentList.GetSelectedItems();
                foreach (int id in ids)
                {
                    LoadedSegments.RemoveAt(id);
                    SegmentList.RemoveItem(id);
                    SegmentPaths.RemoveAt(id);
                }

                if (LoadedSegments.Count == 0) StartButton.Disabled = true;
            }
        }

        public void OnFilesLoaded(string[] paths)
        {
            foreach (var path in paths)
            {
                if (!SegmentPaths.Contains(path))
                {
                    SegmentPaths.Add(path);
                    SegmentList.AddItem(path.Substring(path.LastIndexOf('/') + 1).TrimEnd('.', 't', 's', 'c', 'n'));
                    LoadedSegments.Add(GD.Load<PackedScene>(path));
                }
            }

            if (SegmentList.GetItemCount() > 0) StartButton.Disabled = false;
            SegmentsLoadedEvent?.Invoke();
        }

        public void OnStartButtonPressed()
        {
            Hide();
            StartEvent?.Invoke();
        }

        public void OnRandomSeedCheckToggled(bool state)
        {
            IsSeedRandom = state;
        }

        public void OnRandomizeButtonPressed()
        {
            SeedBox.Value = SeedGen.Randi();
        }
    }
}