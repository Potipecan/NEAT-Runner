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
        private delegate void RunStarter(List<BaseRunner> runners);
        private event RunStarter StartRun;

        private ulong evaluationCount;
        public ulong EvaluationCount => 0;

        private bool stopConditionSatisfied;
        public bool StopConditionSatisfied => stopConditionSatisfied;

        private uint batchSize;
        public uint BatchSize { get => batchSize; set { batchSize = value; } }

        public ulong Seed { get; set; }

        private int batch;
        public int Batch { get => batch; }

        private GameScene _testEnv;
        private IGenomeDecoder<NeatGenome, IBlackBox> _genomeDecoder;
        private List<BaseRunner> _runnersBatch;
        private List<BaseRunner> _newcache;
        private SemaphoreSlim batchWaiter;
        private int listIndex;

        public RunnerGenomeListEvaluator(IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder, GameScene testenv)
        {
            _testEnv = testenv;
            _testEnv.PlayArea.GameOverEvent += OnBatchEnded;
            //StartRun += _testEnv.StartRun;

            _genomeDecoder = genomeDecoder;
            _newcache = new List<BaseRunner>();
            batchWaiter = new SemaphoreSlim(0, 1);
            evaluationCount = 0;
        }

        public void Evaluate(IList<NeatGenome> genomeList)
        {
            //GD.Print($"Genome count: {genomeList.Count}, Batch size: {BatchSize}");
            var taskList = new List<Task>();
            batch = 0;
            //var t = Task.Run(() => { /*System.Threading.Thread.Sleep(500);*/ });

            while (PrepBatch(genomeList))
            {
                batch++;
                Task.Run(() =>
                {
                    var cache = new List<BaseRunner>(_newcache);
                    int i = listIndex;
                    //GD.Print($"{i} enters semaphore.");

                    _testEnv.Tree.CreateTimer(0f).Connect("timeout", _testEnv, nameof(GameScene.StartRun), new Godot.Collections.Array() { cache, Seed });

                    batchWaiter.Wait();

                }).Wait();

            }

            listIndex = 0;
        }

        private void OnBatchEnded()
        {
            do
            {
                if (batchWaiter.CurrentCount < 1)
                {
                    batchWaiter.Release();
                    break;
                }
                else System.Threading.Thread.Sleep(500);

            } while (batchWaiter.CurrentCount > 0);        }

        private bool PrepBatch(IList<NeatGenome> genomeList)
        {
            // checks if end of list has been reached
            if (listIndex >= genomeList.Count) return false;

            int batchsize = (int)BatchSize;

            // make a runner for each of the genomes in batch
            _newcache.Clear();
            for (int c = 0; listIndex < genomeList.Count && c < batchsize; listIndex++, c++)
            {
                IBlackBox phenome = _genomeDecoder.Decode(genomeList[listIndex]);
                var runner = Preloads.ANNRunner.Instance() as ANNRunner;
                runner.Init(genomeList[listIndex], phenome);
                _newcache.Add(runner);
            }

            evaluationCount += (ulong)batchsize;

            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
