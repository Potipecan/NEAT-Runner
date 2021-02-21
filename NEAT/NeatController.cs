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
using Newtonsoft.Json;
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

        private int runnerCount;
        private ANNRunner observedRunner;

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

            ResultSet = new Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, float>>();

        }

        #region NEAT stuff
        /// <summary>
        /// Initializes the NEAT evolution algorithm
        /// </summary>
        /// <param name="eaparams">User evoution algorithm parameters</param>
        public void InitializeNetwork(BundledExperimentSettings settings, int inputs = 18, int outputs = 2)
        {
            _eaParams = new NeatEvolutionAlgorithmParameters(settings.EAParams);
            _neatGenomeParameters = new NeatGenomeParameters(settings.GenomeParams);
            BatchSize = settings.BatchSize;

            _popSize = settings.Population;
            _genomeFactory = new NeatGenomeFactory(inputs, outputs, _neatGenomeParameters);

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
            _listEvaluator.Seed = testEnv.Seed;
            if (NEATSettings.CheckStopCondition((int)network.Statistics._generation, (int)network.Statistics._totalEvaluationCount, (float)network.Statistics._maxFitness)) Stop();

            GD.Print($"Generation: {network.CurrentGeneration}, " +
                $"Max fitness: {network.Statistics._maxFitness:F0}, " +
                $"Mean fitness: {network.Statistics._meanFitness:F0}, " +
                $"Max complexity: {network.Statistics._maxComplexity:F2}, " +
                $"Mean complexity: {network.Statistics._meanComplexity:F2} " +
                $"Course seed: {_listEvaluator.Seed}");

            // TODO: set game pausing and stat log when generation ends
            AddResultSet(network.Statistics);

            testEnv.EvolutionInfoPanel.UpdateStats(
                (float)network.Statistics._maxFitness,
                (float)network.Statistics._meanFitness,
                (float)network.Statistics._maxComplexity,
                (float)network.Statistics._meanComplexity
                );

            testEnv.EvolutionInfoPanel.UpdateEvoutionInfo((int)network.CurrentGeneration, _popSize, _eaParams.SpecieCount);


        }

        #region IGameController implementation
        public async void BeginGame()
        {
            _listEvaluator.Seed = testEnv.Seed;
            if (network != null)
            {
                var genomeList = _genomeFactory.CreateGenomeList(_popSize, 1);
                // initialize network
                await Task.Run(() => network.Initialize(_listEvaluator, _genomeFactory, genomeList));
                if(network.RunState != RunState.NotReady) network.StartContinue();
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
                    if (observedRunner != best)
                    {
                        observedRunner = best as ANNRunner;
                        testEnv.RunnerInfoPanel.UpdateRunnerData(observedRunner.Genome);
                    }

                    if (testEnv.Runners.Count != runnerCount)
                    {
                        runnerCount = testEnv.Runners.Count;
                        testEnv.EvolutionInfoPanel.UpdateBatchInfo(Batch, BatchSize, runnerCount);
                    }
                }
                catch (ObjectDisposedException)
                {
                    //ex.
                    /*throw*/
                    GD.Print("Runner disposed!");
                }
            }
        }

        public void Stop()
        {
            
            network.Stop();
            testEnv.Reset();
            Task.Run(() => ExportToJSON());
        }

        #endregion

        private void ExportToJSON()
        {
            //var eaParams = new NeatEvolutionAlgorithmParameters(_eaParams);
            //var genomeParams = new NeatGenomeParameters(_neatGenomeParameters);

            var eaParams = _eaParams;
            var genomeParams = _neatGenomeParameters;

            var settings = new Godot.Collections.Dictionary
            {
                ["population"] = _popSize,
                ["species_num"] = eaParams.SpecieCount,
                ["elitism"] = (float)eaParams.ElitismProportion,
                ["selection"] = (float)eaParams.SelectionProportion,
                ["offspring_sexual"] = (float)eaParams.OffspringSexualProportion,
                ["offspring_asexual"] = (float)eaParams.OffspringAsexualProportion,
                ["interspecies_mating"] = (float)eaParams.InterspeciesMatingProportion,
                ["add_node_chance"] = (float)genomeParams.AddNodeMutationProbability,
                ["add_conn_chance"] = (float)genomeParams.AddConnectionMutationProbability,
                ["delete_node_chance"] = (float)genomeParams.DeleteConnectionMutationProbability,
                ["weight_mutation_chance"] = (float)genomeParams.ConnectionWeightMutationProbability,
                ["initial_connection"] = (float)genomeParams.InitialInterconnectionsProportion
            };


            GD.Print("Export check");

            var export = new Godot.Collections.Dictionary<string, Godot.Collections.Dictionary>()
            {
                ["settings"] = settings,
                ["results"] = (Godot.Collections.Dictionary)ResultSet
            };

            string res = JSON.Print(export);
            //GD.Print(res);


            var file = new File();
            file.Open($"res://Results/{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.json", File.ModeFlags.Write);
            file.StoreString(res);
            file.Close();
        }

        private void AddResultSet(NeatAlgorithmStats stats)
        {
            var set = new Godot.Collections.Dictionary<string, float>
            {
                ["max_fitness"] = (int)Math.Round(stats._maxFitness),
                ["mean_fitness"] = (int)stats._meanFitness,
                ["max_complexity"] = (float)stats._maxComplexity,
                ["mean_complexitiy"] = (float)stats._meanComplexity,
                ["sexual_offspring_count"] = stats._sexualOffspringCount,
                ["asexual_offspring_count"] = stats._asexualOffspringCount
            };

            ResultSet[$"{stats._generation}"] = set;

            //GD.Print(JSON.Print(ResultSet));
        }

    }
}
