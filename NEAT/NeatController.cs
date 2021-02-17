using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Decoders;
using SharpNeat.Core;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Decoders.Neat;
using SharpNeat.Phenomes;
using SharpNeat.DistanceMetrics;
using SharpNeat.SpeciationStrategies;
using A_NEAT_arena.Game;
using Godot;
//using Godot.Collections;

namespace A_NEAT_arena.NEAT
{
    class NeatController : IGameController
    {
        private NeatEvolutionAlgorithmParameters _eaParams;
        private NeatGenomeParameters _neatGenomeParameters;
        private int _popSize;
        private NetworkActivationScheme _activationScheme;
        private int _batchSize;

        private GameScene testEnv;
        private IGenomeFactory<NeatGenome> _genomeFactory;
        private IGenomeDecoder<NeatGenome, IBlackBox> _genomeDecoder;
        private ISpeciationStrategy<NeatGenome> _speciationStrategy;
        private RunnerGenomeListEvaluator _listEvaluator;

        #region Public properties
        public int Batch { get => _listEvaluator.Batch; }
        #endregion

        private Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, float>> ResultSet;

        private NeatEvolutionAlgorithm<NeatGenome> network;

        public event EventHandler GenerationEnded;

        public int BatchSize
        {
            get => _batchSize;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("Batch size cannot be less than 1.");
                else _batchSize = value;
            }
        }

        public NeatController(GameScene env)
        {
            testEnv = env;

            _speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(new ManhattanDistanceMetric());

            _activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1);
            _genomeDecoder = new NeatGenomeDecoder(_activationScheme);

        }

        #region NEAT stuff
        /// <summary>
        /// Initializes the NEAT evolution algorithm
        /// </summary>
        /// <param name="eaparams">User evoution algorithm parameters</param>
        public void InitializeNetwork(BundledExperimentSettings settings, int inputs = 18, int outputs = 2)
        {
            _eaParams = settings.EAParams;
            BatchSize = settings.BatchSize;

            _popSize = settings.Population;
            _genomeFactory = new NeatGenomeFactory(inputs, outputs, settings.GenomeParams);

            // instance new NeatEvolutionAlgorithm
            var complexityReg = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Absolute, (inputs + outputs) * 3);
            network = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, _speciationStrategy, complexityReg);

            // initialize objects needed for the network
            _listEvaluator = new RunnerGenomeListEvaluator(_genomeDecoder, testEnv);
            _listEvaluator.BatchSize = (uint)_batchSize;


            // set update scheme and connect update event
            network.UpdateScheme = new UpdateScheme(1);
            network.UpdateEvent += OnNetworkUpdate;
        }
        #endregion

        private void OnNetworkUpdate(object sender, EventArgs e)
        {
            GD.Print($"Generation: {network.CurrentGeneration}, " +
                $"Max fitness: {network.Statistics._maxFitness:F0}, " +
                $"Mean fitness: {network.Statistics._meanFitness:F0}, " +
                $"Max complexity: {network.Statistics._maxComplexity:F2}, " +
                $"Mean complexity: {network.Statistics._meanComplexity:F2}");

            // TODO: set game pausing and stat log when generation ends
            AddResultSet(network.Statistics);
        }

        #region IGameController implementation
        public async void BeginGame()
        {
            if (network != null)
            {
                var genomeList = _genomeFactory.CreateGenomeList(_popSize, 0);
                // initialize network
                await Task.Run(() => network.Initialize(_listEvaluator, _genomeFactory, genomeList));
                network.StartContinue();
            }
        }

        public void Pause()
        {
            if (network != null)
            {
                network.RequestPause();
            }
        }

        public void Resume()
        {
            if (network != null) network.StartContinue();
        }

        public void OnRoundEnded()
        {
            //throw new NotImplementedException();
        }

        public void Process()
        {
            if (!testEnv.Tree.Paused && testEnv.Runners.Count > 0)
            {
                testEnv.Runners.Sort(GameScene.RunnerComparer);

                try
                {
                    var best = testEnv.Runners[0];
                    testEnv.Camera.Position = new Vector2(best.Position.x - 860, 0);
                    testEnv.SetScore((int)best.Score);
                }
                catch (ObjectDisposedException)
                {
                    //ex.
                    /*throw*/;
                }
            }
        }
        #endregion

        private void ExportToJSON()
        {
            string res = JSON.Print(ResultSet);
        }

        private void AddResultSet(NeatAlgorithmStats stats)
        {
            var set = new Godot.Collections.Dictionary<string, float>
            {
                ["max_fitness"] = (float)stats._maxFitness,
                ["mean_fitness"] = (float)stats._meanFitness,
                ["max_complexity"] = (float)stats._maxComplexity,
                ["mean_complexitiy"] = (float)stats._meanComplexity,
                ["sexual_offspring_count"] = stats._sexualOffspringCount,
                ["asexual_offspring_count"] = stats._asexualOffspringCount
            };

            ResultSet[$"{stats._generation}"] = set;

            //JSON.Print(ResultSet)
        }
    }
}
