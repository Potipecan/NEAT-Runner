using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace A_NEAT_arena.NEAT
{
    class RandomEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        public ulong EvaluationCount => 0;

        public bool StopConditionSatisfied => false;

        private Random Rand;

        public RandomEvaluator()
        {
            Rand = new Random();
        }

        public FitnessInfo Evaluate(IBlackBox phenome)
        {
            int fit = Rand.Next() % 10000;
            return new FitnessInfo(fit, fit);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
