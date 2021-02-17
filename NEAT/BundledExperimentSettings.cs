using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;

namespace A_NEAT_arena.NEAT
{
    public class BundledExperimentSettings
    {
        public int Population { get; private set; }
        public int BatchSize { get; private set; }
        public NeatEvolutionAlgorithmParameters EAParams { get; private set; }
        public NeatGenomeParameters GenomeParams { get; private set; }

        public BundledExperimentSettings(int population, int batchSize, NeatEvolutionAlgorithmParameters eaParams, NeatGenomeParameters genomeParams)
        {
            Population = population;
            BatchSize = batchSize;
            EAParams = eaParams;
            GenomeParams = genomeParams;
        }
    }
}
