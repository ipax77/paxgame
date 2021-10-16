using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace paxgame.Shared
{
    public record AiResponse
    {
        public int[] moves { get; init; }
        public double reward { get; init; }
        
        public AiResponse(int[] moves, double reward)
        {
            this.moves = moves;
            this.reward = reward;
        }
    }
}
