using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using A_NEAT_arena.Game;
using SharpNeat.Phenomes;
using System.Threading;
using System.Diagnostics;
using Godot;

namespace A_NEAT_arena.NEAT
{
    class RunnerGenomeListEvaluator : IGenomeListEvaluator<NeatGenome>
    {
        public ulong EvaluationCount => 0;

        public bool StopConditionSatisfied => false;

        private uint batchSize;
        public uint BatchSize { get => batchSize; set { batchSize = value; } }

        private GameScene _testEnv;
        private IGenomeDecoder<NeatGenome, IBlackBox> _genomeDecoder;
        private List<BaseRunner> _runnersBatch;
        private SemaphoreSlim batchWaiter;
        private int listIndex;

        public RunnerGenomeListEvaluator(IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder, GameScene testenv)
        {
            _testEnv = testenv;
            _testEnv.PlayArea.GameOverEvent += OnBatchEnded;
            _genomeDecoder = genomeDecoder;
            _runnersBatch = new List<BaseRunner>();
            batchWaiter = new SemaphoreSlim(1, 1);
        }

        public void Evaluate(IList<NeatGenome> genomeList)
        {
            //batchWaiter.Release();
            GD.Print($"current count: {batchWaiter.CurrentCount}");
            for (int i = 0, bsize = (int)BatchSize; i < genomeList.Count; i += bsize, bsize = (int)BatchSize)
            {
                _runnersBatch = PrepBatch(genomeList, i, bsize);
                batchWaiter.Wait();
                _testEnv.Runners = _runnersBatch;
                _testEnv.StartRun();
            }
        }

        private void OnBatchEnded()
        {
            _runnersBatch.Clear();
            batchWaiter.Release();
        }

        private List<BaseRunner> PrepBatch(IList<NeatGenome> genomeList, int index, int batchsize)
        {
            var res = new List<BaseRunner>();
            GD.Print($"Number of genomes: {genomeList.Count} Index: {index} Batch size: {batchsize}");
            var genomebatch = genomeList.ToList().GetRange(index, batchsize);

            foreach (var genome in genomebatch)
            {
                IBlackBox phenome = _genomeDecoder.Decode(genome);
                var runner = Preloads.ANNRunner.Instance() as ANNRunner;
                runner.Init(genome, phenome);
                res.Add(runner);
            }

            GD.Print($"{index}");
            return res;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
