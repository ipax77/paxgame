using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace paxgame.Shared.Services
{
    public static class StyleService
    {
        public const double boardX = 1000;
        public const double boardY = 600;

        public static string GenStyle(GameResult game, double boardX, double boardY, TimeSpan duration)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var unit in game.Units)
            {
                (var styles, var delays) = StyleService.GetUnitPath(unit, game.Step);
                var svg = StyleService.GetSvg(unit, styles, boardX, boardY);

                var perTotal = Math.Round(unit.Gameloop * 100.0 / (double)game.Loop, 2);

                sb.Append($".u{unit.Id}c{{");
                sb.Append($"offset-path:path('{svg}');");
                sb.Append($"animation:u{unit.Id}a {duration.TotalSeconds.ToString(CultureInfo.InvariantCulture)}s linear 0s forwards");
                sb.Append($",uo {(duration.TotalSeconds / 10).ToString(CultureInfo.InvariantCulture)}s linear {(duration * (perTotal / 100)).TotalSeconds.ToString(CultureInfo.InvariantCulture)}s forwards");
                sb.Append(";}");

                //sb.Append($".u{unit.Id}t{{");
                //sb.Append($"offset-path:path('{svg}');");
                //sb.Append(";}");

                sb.Append($"@keyframes u{unit.Id}a");
                sb.Append("{from{offset-distance:0%;}");
                for (int i = 0; i < delays.Count; i++)
                {
                    var perFrom = Math.Round(delays[i].FromGameloop * 100.0 / (double)game.Loop, 2);
                    var perTo = Math.Round(delays[i].ToGameloop * 100.0 / (double)game.Loop, 2);
                    if (perTo > 0)
                    {
                        sb.Append($"{perFrom.ToString(CultureInfo.InvariantCulture)}%{{offset-distance:{unit.GetOffset(delays[i].ToGameloop).ToString(CultureInfo.InvariantCulture)}%;}}");
                        sb.Append($"{perTo.ToString(CultureInfo.InvariantCulture)}%{{offset-distance:{unit.GetOffset(delays[i].ToGameloop).ToString(CultureInfo.InvariantCulture)}%;}}");
                    }
                    else
                    {
                        sb.Append($"{perFrom.ToString(CultureInfo.InvariantCulture)}%{{offset-distance:{unit.GetOffset(delays[i].FromGameloop).ToString(CultureInfo.InvariantCulture)}%;}}");
                    }
                }
                var perPathTotal = Math.Round(unit.Path.Last().Gameloop * 100.0 / (double)game.Loop, 2);
                sb.Append($"{perPathTotal.ToString(CultureInfo.InvariantCulture)}%{{offset-distance:100%;}}");
                sb.Append("to{offset-distance:100%;}}");

                sb.Append(GetAbilityAnimations(unit, duration, game.Loop));

                unit.Path = new List<StyleInfo>();
            }
            sb.Append("@keyframes uo{from{opacity:1;}to{opacity:0;}}"); // death anim
            sb.Append("@keyframes aa{from{opacity:0;}10%{opacity: 1;}to{opacity:0;}}"); // ability anim
            return sb.ToString();
        }

        public static string GetAbilityAnimations(UnitResult unit, TimeSpan duration, double gameLoop)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < unit.AbilityPath.Count; i++)
            {
                var perFrom = unit.AbilityPath[i].Gameloop / gameLoop;
                var sec = Math.Round((duration * perFrom).TotalSeconds, 2);
                sb.Append($".u{unit.Id}_{i}a{{");
                sb.Append($"animation:aa {1}s linear {sec.ToString(CultureInfo.InvariantCulture)}s forwards");
                sb.Append(";}");
            }
            return sb.ToString();
        }

        public static string GetSvg(UnitResult unit, List<StyleInfo> styles, double boardX, double boardY)
        {
            StringBuilder sb = new StringBuilder();
            if (StyleService.boardX == boardX && StyleService.boardY == boardY)
            {
                sb.Append($"M{unit.BuildPosition.X},{unit.BuildPosition.Y} ");
                for (int i = 0; i < styles.Count; i++)
                {
                    sb.Append($"L{MathF.Round(styles[i].Pos.X, 2).ToString(CultureInfo.InvariantCulture)},{MathF.Round(styles[i].Pos.Y, 2).ToString(CultureInfo.InvariantCulture)} ");
                }
            } else
            {
                var modX = boardX / StyleService.boardX;
                var modY = boardY / StyleService.boardY;
                sb.Append($"M{Math.Round(unit.BuildPosition.X * modX, 2).ToString(CultureInfo.InvariantCulture)},{Math.Round(unit.BuildPosition.Y * modY, 2).ToString(CultureInfo.InvariantCulture)} ");
                for (int i = 0; i < styles.Count; i++)
                {
                    sb.Append($"L{Math.Round(styles[i].Pos.X * modX, 2).ToString(CultureInfo.InvariantCulture)},{Math.Round(styles[i].Pos.Y * modY, 2).ToString(CultureInfo.InvariantCulture)} ");
                }
            }
            sb.Length--;
            return sb.ToString();
        }

        public static (List<StyleInfo>, List<DelayInfo>) GetUnitPath(UnitResult unit, int step)
        {
            var lastDirection = unit.Team == 1 ? new Vector2(0, 1) : new Vector2(0, -1);
            var lastPos = unit.BuildPosition;
            List<StyleInfo> styles = new List<StyleInfo>();
            List<DelayInfo> delays = new List<DelayInfo>();
            if (unit.Path.Any())
            {
                styles.Add(unit.Path[0]);
            }
            else
            {
                return (styles, delays);
            }

            int lastLoop = 0;

            for (int i = 1; i < unit.Path.Count; i++)
            {
                var direction = new Vector2(MathF.Round(unit.Path[i].Direction.X, 2), MathF.Round(unit.Path[i].Direction.Y, 2));

                if (direction != lastDirection && unit.Path[i].Distance > 1)
                {
                    styles.Add(unit.Path[i]);
                }

                if (unit.Path[i].Gameloop - lastLoop > step)
                {
                    delays.Add(new DelayInfo()
                    {
                        FromGameloop = lastLoop,
                        ToGameloop = unit.Path[i].Gameloop,
                    });
                }
                else if (unit.Path[i - 1].Speed != unit.Path[i].Speed)
                {
                    delays.Add(new DelayInfo()
                    {
                        FromGameloop = unit.Path[i].Gameloop,
                        ToGameloop = 0,
                    });
                }

                lastDirection = direction;
                lastPos = unit.Path[i].Pos;
                lastLoop = unit.Path[i].Gameloop;
            }
            styles.Add(unit.Path.Last());
            return (styles, delays);
        }

        private static Regex modRx = new Regex(@"path\('([ML\d,\s\.]*)'\);");
        private static Regex numRx = new Regex(@"([\d\.]+)");
        public static string ModStyle(string style, double x, double y)
        {
            var modX = x / boardX;
            var modY = y / boardY;

            var matches = modRx.Matches(style);
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                var mindex = matches[i].Index;
                sb.Append(style.Substring(index, mindex - index));
                string value = matches[i].Groups[1].Value;
                var nmatches = numRx.Matches(value);
                StringBuilder svg = new StringBuilder();
                if (nmatches.Count > 1)
                {
                    svg.Append($"path('M{Math.Round(Convert.ToDouble(nmatches[0].Groups[1].Value) * modX, 2).ToString(CultureInfo.InvariantCulture)},{Math.Round(Convert.ToDouble(nmatches[1].Groups[1].Value) * modY, 2).ToString(CultureInfo.InvariantCulture)} ");

                    for (int j = 2; j < nmatches.Count; j++)
                    {
                        var mvalue = Convert.ToDouble(nmatches[j].Groups[1].Value, CultureInfo.InvariantCulture);
                        
                        if (j % 2 == 0)
                        {
                            svg.Append($"L{Math.Round(mvalue * modX, 2).ToString(CultureInfo.InvariantCulture)}");
                        }
                        else
                        {
                            svg.Append($",{Math.Round(mvalue * modY, 2).ToString(CultureInfo.InvariantCulture)} ");
                        }
                    }
                    svg.Length--;
                    svg.Append("');");
                    sb.Append(svg.ToString());
                }
                index = mindex + matches[i].Length;
            }
            if (index < style.Length)
            {
                sb.Append(style.Substring(index));
            }
            return sb.ToString();
        }
    }
}
