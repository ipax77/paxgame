using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace paxgame.Shared
{
    public record AiRequest
    {
        public Guid guid { get; init; }
        public int[][] moves { get; init; }

        public AiRequest(Guid guid, int[][] moves)
        {
            this.guid = guid;
            this.moves = moves;
        }
    }
}
