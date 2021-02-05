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
    class RunnerEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        public ulong EvaluationCount => throw new NotImplementedException();

        public bool StopConditionSatisfied => throw new NotImplementedException();

        public FitnessInfo Evaluate(IBlackBox phenome)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
