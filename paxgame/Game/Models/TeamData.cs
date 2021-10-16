using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Models
{
    public class TeamData
    {
        public List<BattleUnit> Units { get; set; } = new List<BattleUnit>();
        private List<BattleUnit>? HealableUnits { get; set; }
        public object lockobject = new object();

        public void Set(List<BattleUnit> units)
        {
            Units = units;
            HealableUnits = null;
        }

        public List<BattleUnit> GetHealableUnits()
        {
            lock (lockobject)
            {
                if (HealableUnits == null)
                {
                    HealableUnits = Units.Where(x =>
                        x.Unit.Attributes.Contains(UnitAttribute.Biological)
                        && x.Unit.Defence.Healthpoints > 50
                        && x.BattleDefence.BattleHealthpoints < x.Unit.Defence.Healthpoints - 50)
                    .ToList();
                }
            }
            return HealableUnits;
        }

        public void RemoveHealableUnit(BattleUnit unit)
        {
            if (HealableUnits != null && HealableUnits.Contains(unit))
            {
                HealableUnits.Remove(unit);
            }
        }
    }
}
