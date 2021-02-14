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

namespace A_NEAT_arena.NEAT
{
    class NeatController : IGameController
    {
        private NeatEvolutionAlgorithmParameters _eaParams;
        private NeatGenomeParameters _neatGenomeParameters;
        private string _name;
        private int _popSize;
        private int _specieCount;
        private NetworkActivationScheme _activationScheme;
        private string _complexityRegulationStr;
        private int? _complexityTreshold;
        private string _description;
        private ParallelOptions _parallelOptions;
        private int _batchSize;

        private GameScene testEnv;
        private IGenomeFactory<NeatGenome> _genomeFactory;
        private IGenomeDecoder<NeatGenome, IBlackBox> _genomeDecoder;
        private ISpeciationStrategy<NeatGenome> _speciationStrategy;
        private RunnerGenomeListEvaluator _listEvaluator;

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

        /// <summary>
        /// Initializes the NEAT evolution algorithm
        /// </summary>
        /// <param name="eaparams">User evoution algorithm parameters</param>
        public void InitializeNetwork(NeatEvolutionAlgorithmParameters eaparams, int pop, int batchSize, int inputs = 18, int outputs = 2)
        {
            _eaParams = eaparams;
            BatchSize = batchSize;

            _popSize = pop;
            _genomeFactory = new NeatGenomeFactory(inputs, outputs);

            // instance new NeatEvolutionAlgorithm
            network = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, _speciationStrategy, new NullComplexityRegulationStrategy());

            // set update scheme and connect update event
            network.UpdateScheme = new UpdateScheme(1);
            network.UpdateEvent += OnNetworkUpdate;

            // initialize objects needed for the network
            _listEvaluator = new RunnerGenomeListEvaluator(_genomeDecoder, testEnv);
            _listEvaluator.BatchSize = (uint)_batchSize;
            var genomeList = _genomeFactory.CreateGenomeList(_popSize, 0);

            // initialize network
            network.Initialize(_listEvaluator, _genomeFactory, genomeList);
            network.RequestPause();
        }

        private void OnNetworkUpdate(object sender, EventArgs e)
        {
            GD.Print($"Generation:{network.CurrentGeneration}\n" +
                $"Max fitness: {network.Statistics._maxFitness}\n" +
                $"Mean fitness: {network.Statistics._meanFitness}");

            // TODO: set game pausing when generation ends
        }

        #region IGameController implementation
        public void BeginGame()
        {
            if(network != null) network.StartContinue();
        }

        public void Pause()
        {
            if (network != null) network.RequestPause();
        }

        public void Resume()
        {
            if (network != null) network.StartContinue();
        }

        public void OnRoundEnded()
        {
            throw new NotImplementedException();
        }

        public void Process()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
