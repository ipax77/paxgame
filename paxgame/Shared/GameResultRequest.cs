using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace paxgame.Shared
{
    public sealed record GameResultRequest
    {
        public Guid guid { get; init; }
        public int[][] moves { get; init; }
        public double x { get; init; }
        public double y { get; init; }
        public int duration { get; init; }
        
        [JsonConstructor]
        public GameResultRequest(Guid guid, int[][] moves, double x, double y, int duration)
        {
            this.guid = guid;
            this.moves = moves;
            this.x = x;
            this.y = y;
            this.duration = duration;
        }

        public GameResultRequest(Guid guid, HashSet<int> moves, HashSet<int> oppmoves, double boardX, double boardY, int duration)
        {
            this.guid = guid;
            this.moves = new int[2][] { moves.ToArray(), oppmoves.ToArray() };
            x = boardX;
            y = boardY;
            this.duration = duration;
        }
    }
}
