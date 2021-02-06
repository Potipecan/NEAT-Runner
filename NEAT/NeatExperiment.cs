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

namespace A_NEAT_arena.NEAT
{
    class NeatExperiment
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

        private GameScene testEnv;
        private IGenomeFactory<NeatGenome> genomeFactory;
        private ISpeciationStrategy<NeatGenome> speciationStrategy;

        private NeatEvolutionAlgorithm<NeatGenome> network;
        public NeatEvolutionAlgorithm<NeatGenome> Network { get => network; set { network = value; } }

        public NeatExperiment(GameScene env, NeatEvolutionAlgorithmParameters eaparams, int inputs = 18, int outputs = 2)
        {
            testEnv = env;
            _eaParams = eaparams;
            genomeFactory = new NeatGenomeFactory(inputs, outputs);
            speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(new ManhattanDistanceMetric());
            Network = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, new NullComplexityRegulationStrategy());


        }

        public void InitializeNetwork()
        {
        }
    }
}
