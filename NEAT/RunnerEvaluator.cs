using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using A_NEAT_arena.Game;

namespace A_NEAT_arena.NEAT
{
    class RunnerEvaluator : Godot.Object, IPhenomeEvaluator<IBlackBox>
    {
        public ulong EvaluationCount => 0;

        public bool StopConditionSatisfied => testEnv.Stop;

        private GameScene testEnv;

        public RunnerEvaluator(GameScene env)
        {
            testEnv = env;
        }

        public FitnessInfo Evaluate(IBlackBox phenome)
        {
            
            var runner = testEnv.AddANNRunner(phenome);
            Task.Run(async () => await ToSignal(runner, nameof(BaseRunner.Died))).Wait();
            FitnessInfo res = new FitnessInfo(runner.Score, runner.Score);

            return res;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
