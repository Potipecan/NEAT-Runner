﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_NEAT_arena.Game
{
    interface IGameController
    {
        void BeginGame();

        void Pause();

        void Resume();

        void OnRoundEnded();

        void Process();
    }
}