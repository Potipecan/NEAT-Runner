using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_NEAT_arena.Game
{
    class PlayerGameController : IGameController
    {
        private GameScene _env;

        public PlayerGameController(GameScene env)
        {
            _env = env;
        }

        #region IGameController implementation
        public void BeginGame()
        {
            throw new NotImplementedException();
        }

        public void OnRoundEnded()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Process()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
