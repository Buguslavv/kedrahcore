using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Tibia.Objects;
using Tibia.Constants;
using Kedrah.Objects;
using Kedrah.Constants;
using System.Threading;

namespace Kedrah.Modules
{
    public class Targeting : Module
    {
        #region Variables/Objects

        private Creature creature;
        private Target target;

        public static Dictionary<DamageType, string> MageStrikes = new Dictionary<DamageType, string> { 
            { DamageType.Death, "exori mort" },
            { DamageType.Earth, "exori tera" },
            { DamageType.Energy, "exori vis" },
            { DamageType.Fire, "exori flam" },
            { DamageType.Ice, "exori frigo" }
        };

        public static Dictionary<DamageType, string> PaladinStrikes = new Dictionary<DamageType, string> { 
            { DamageType.Holy, "exori san" },
            { DamageType.Physical, "exori con" }
        };

        public bool AttackedOnly, Reachable, Shootable, IsTargeting;
        public byte Distance = 2;
        public byte OthersMonsters;
        public Dictionary<string, byte> TargetSelection = new Dictionary<string, byte>();
        public List<Target> Targets = new List<Target>();

        #endregion

        #region Constructor/Destructor

        public Targeting(ref Core core)
            : base(ref core)
        {
            Reachable = false;
            Reachable = true;
            Shootable = false;
            OthersMonsters = 0;
            TargetSelection.Add("distance", 0);
            TargetSelection.Add("health", 0);
            TargetSelection.Add("priority", 100);
            TargetSelection.Add("stick", 0);

            #region Timers

            Timers.Add("target", new Tibia.Util.Timer(500, false));
            Timers["target"].Execute += new Tibia.Util.Timer.TimerExecution(Target_OnExecute);
            Timers.Add("action", new Tibia.Util.Timer(2000, false));
            Timers["action"].Execute += new Tibia.Util.Timer.TimerExecution(Action_OnExecute);
            Timers.Add("move", new Tibia.Util.Timer(1000, false));
            Timers["move"].Execute += new Tibia.Util.Timer.TimerExecution(Move_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public long ActionDelay
        {
            get
            {
                return Timers["action"].Interval;
            }
            set
            {
                Timers["action"].Interval = value;
            }
        }

        public bool Attacker
        {
            get
            {
                if (Timers["target"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("target");
                    PlayTimer("action");
                    PlayTimer("move");
                }
                else
                {
                    PauseTimer("target");
                    PauseTimer("action");
                    PauseTimer("move");
                }
            }
        }

        public long AttackDelay
        {
            get
            {
                return Timers["target"].Interval;
            }
            set
            {
                Timers["target"].Interval = value;
            }
        }

        #endregion

        #region Module Functions

        #region Add Targets

        public void AddAllCreatures()
        {
            foreach (var c in CreatureLists.AllCreatures)
            {
                Targets.Add(new Target(c.Value, FightActions.Attack, 0, FightSecurity.Automatic, FightStances.Stand, Attack.FullAttack, Follow.DoNotFollow));
            }
        }

        public void AddAllCreatures(FightActions action, byte priority, FightSecurity security, FightStances stance, Attack attackMode, Follow followMode)
        {
            foreach (var c in CreatureLists.AllCreatures)
            {
                Targets.Add(new Target(c.Value, action, priority, security, stance, attackMode, followMode));
            }
        }

        public void AddTarget(string name)
        {
            CreatureData c = null;

            if (CreatureLists.AllCreatures.ContainsKey(name))
            {
                c = CreatureLists.AllCreatures[name];
            }
            else
            {
                c = new CreatureData(name, 0, 0, 0, 0, 0, false, false, FrontAttack.None, null, null, null, null, null);
            }

            Targets.Add(new Target(c));
        }

        public void AddTarget(string name, FightActions action, byte priority, FightSecurity security, FightStances stance, Attack attackMode, Follow followMode)
        {
            CreatureData c = null;

            if (CreatureLists.AllCreatures.ContainsKey(name))
            {
                c = CreatureLists.AllCreatures[name];
            }
            else
            {
                c = new CreatureData(name, 0, 0, 0, 0, 0, false, false, FrontAttack.None, null, null, null, null, null);
            }

            Targets.Add(new Target(c, action, priority, security, stance, attackMode, followMode));
        }

        #endregion

        #region Get Spells

        public string GetBestMageSpell(CreatureData creatureData)
        {
            return GetBestMageSpell(creatureData, "exori mort", "exori tera", "exori vis", "exori flam", "exori frigo");
        }

        public string GetBestMageSpell(CreatureData creatureData, string deathSpell, string earthSpell, string energySpell, string fireSpell, string iceSpell)
        {
            DamageType damageType = creatureData.GetWeakness(new List<DamageType>() { DamageType.Death, DamageType.Earth, DamageType.Energy, DamageType.Fire, DamageType.Ice });

            switch (damageType)
            {
                case DamageType.Death:
                    return deathSpell;
                case DamageType.Earth:
                    return earthSpell;
                case DamageType.Energy:
                    return energySpell;
                case DamageType.Fire:
                    return fireSpell;
                case DamageType.Ice:
                    return iceSpell;
            }

            return iceSpell;
        }

        public string GetBestPaladinSpell(CreatureData creatureData)
        {
            return GetBestPaladinSpell(creatureData, "exori san", "exori con");
        }

        public string GetBestPaladinSpell(CreatureData creatureData, string holySpell, string physicalSpell)
        {
            DamageType damageType = creatureData.GetWeakness(new List<DamageType>() { DamageType.Holy, DamageType.Physical });

            switch (damageType)
            {
                case DamageType.Holy:
                    return holySpell;
                case DamageType.Physical:
                    return physicalSpell;
            }

            return holySpell;
        }

        #endregion

        public void SelectTarget()
        {
            if (!Core.Client.LoggedIn)
            {
                return;
            }

            Creature selected = null;
            Target selectedTarget = null;
            Dictionary<string, int[]> verifier = new Dictionary<string, int[]>(4);

            verifier.Add("distance", new int[2] { 0, 0 });
            verifier.Add("health", new int[2] { 0, 0 });
            verifier.Add("priority", new int[2] { 0, 0 });
            verifier.Add("stick", new int[2] { 0, 0 });
            List<KeyValuePair<string, byte>> items = TargetSelection.OrderByDescending(s => s.Value).ToList();
            List<Creature> creatures = Core.Client.BattleList.GetCreatures().ToList();
            creatures.RemoveAll(c => c.Z != Core.Player.Z || c.IsSelf() || c.HPBar == 0 || c.Type != CreatureType.NPC);

            foreach (Creature creature in creatures)
            {
                Target target = Targets.Find(delegate(Target t)
                {
                    return (string.Compare(t.Name, "All", false) == 0 || string.Compare(t.Name, creature.Name, true) == 0 && (t.HPRange[0] <= creature.HPBar && t.HPRange[1] >= creature.HPBar));
                });

                if (target == null)
                {
                    continue;
                }

                if (AttackedOnly && !creature.IsAttacking())
                {
                    continue;
                }

                try
                {
                    if (Reachable && !creature.IsReachable())
                    {
                        continue;
                    }
                }
                catch { }

                if (OthersMonsters > 0)
                {
                    var playersAround = Core.Client.BattleList.GetCreatures().ToList().FindAll(delegate(Creature c)
                    {
                        return c.DistanceBetween(creature.Location) <= OthersMonsters && (c.Z == creature.Location.Z) && (c.Type == CreatureType.Player) && (!c.IsSelf());
                    });

                    if (playersAround.Count > 0 && !creature.IsAttacking())
                    {
                        continue;
                    }
                }

                if (selected == null)
                {
                    selected = creature;
                    selectedTarget = target;
                    continue;
                }

                if (selectedTarget.Action != FightActions.Attack && target.Action == FightActions.Attack)
                {
                    selected = creature;
                    selectedTarget = target;
                    continue;
                }

                verifier["distance"][0] = selected.Distance();
                verifier["health"][0] = selected.HPBar;
                verifier["priority"][0] = -selectedTarget.Priority;
                verifier["stick"][0] = (int) selected.Id;
                verifier["distance"][1] = creature.Distance();
                verifier["health"][1] = creature.HPBar;
                verifier["priority"][1] = -target.Priority;
                verifier["stick"][1] = (int) creature.Id;

                foreach (KeyValuePair<string, byte> v in items)
                {
                    if (v.Key == "stick")
                    {
                        if (creature.Id == Core.Player.RedSquare)
                        {
                            selected = creature;
                            selectedTarget = target;
                        }

                        break;
                    }

                    if (verifier[v.Key][0] > verifier[v.Key][1])
                    {
                        selected = creature;
                        selectedTarget = target;
                        break;
                    }
                }
            }

            this.target = selectedTarget;
            this.creature = selected;
        }

        #endregion

        #region Timers

        private void Target_OnExecute()
        {
            if (Core.Modules.Looter.IsLooting)
            {
                return;
            }

            SelectTarget();

            if (target == null || creature == null)
            {
                IsTargeting = false;
                return;
            }

            IsTargeting = true;

            Core.Client.SetModes(target.AttackMode, target.FollowMode);
            Core.Player.RedSquare = creature.Id;

            if (target.Action == FightActions.Attack)
            {
                creature.Attack(Core.Player.RedSquare == creature.Id);
            }
            else if (target.Action == FightActions.Follow)
            {
                creature.Follow(Core.Player.GreenSquare == creature.Id);
            }
        }

        private void Action_OnExecute()
        {
            if (Core.Modules.Looter.IsLooting)
            {
                return;
            }

            if (target == null || creature == null)
            {
                return;
            }

            foreach (FightExtraPair extra in target.Extra)
            {
                extra.Execute(creature, Core.Client.Inventory);
            }
        }

        private void Move_OnExecute()
        {
            if (Core.Modules.Looter.IsLooting)
            {
                return;
            }

            if (target == null || creature == null)
            {
                return;
            }

            List<Location> prohibited = new List<Location>();

            try
            {
                switch (target.Security)
                {
                    case FightSecurity.Both:
                        prohibited = creature.Location.BeamAffectedLocations();
                        prohibited.AddRange(creature.Location.WaveAffectedLocations());
                        break;
                    case FightSecurity.Beam:
                        prohibited = creature.Location.BeamAffectedLocations();
                        break;
                    case FightSecurity.Wave:
                        prohibited = creature.Location.WaveAffectedLocations();
                        break;
                    case FightSecurity.Automatic:
                        switch (target.FrontAttack)
                        {
                            case FrontAttack.Both:
                                prohibited = creature.Location.BeamAffectedLocations();
                                prohibited.AddRange(creature.Location.WaveAffectedLocations());
                                break;
                            case FrontAttack.Beam:
                                prohibited = creature.Location.BeamAffectedLocations();
                                break;
                            case FrontAttack.Wave:
                                prohibited = creature.Location.WaveAffectedLocations();
                                break;
                        }
                        break;
                }

                foreach (Location l in prohibited)
                    Core.Client.Map.GetTile(l).ReplaceGround(923);

                if (!Core.Player.IsWalking)
                {
                    if (prohibited.Count > 0 || target.Stance == FightStances.Distance)
                    {
                        List<Tile> list;
                        Tile tile;
                        IEnumerable<Tile> possible = Core.Client.Map.GetTilesOnSameFloor().Where(t => t.Location.DistanceBetween(creature.Location) <= ((target.Stance == FightStances.Distance) ? Distance : 1));
                        possible = possible.Where(t => !prohibited.Contains(t.Location));
                        try { possible = possible.Where(t => t.Location.IsReachable()); }
                        catch { }
                        list = possible.ToList();
                        list.Sort(new Comparison<Tile>(delegate(Tile t1, Tile t2) { return t1.Location.Distance().CompareTo(t2.Location.Distance()); }));
                        tile = list.FirstOrDefault(t => t.Location.Distance() == Distance);
                        if (tile != null) tile = list.FirstOrDefault();

                        if (tile != null)
                        {
                            Core.Player.GoTo = tile.Location;
                        }
                    }
                    else if (target.Stance == FightStances.ParryFollow || target.Stance == FightStances.ParryStand)
                    {
                        if (Core.Player.Location.MonstersAround() > 2)
                        {
                            List<Tile> list;
                            Tile tile;
                            IEnumerable<Tile> possible = Core.Client.Map.GetTilesOnSameFloor().Where(t => t.Location.MonstersAround() <= 2);
                            try { possible = possible.Where(t => t.Location.IsReachable()); }
                            catch { }
                            list = possible.ToList();
                            list.Sort(new Comparison<Tile>(delegate(Tile t1, Tile t2) { return t1.Location.Distance().CompareTo(t2.Location.Distance()); }));
                            tile = list.FirstOrDefault(t => t.Location.Distance() == Distance);

                            if (tile != null)
                            {
                                Core.Player.GoTo = tile.Location;
                            }
                        }
                    }
                    else if (target.Stance == FightStances.ParryFollow || target.Stance == FightStances.Follow)
                    {
                        creature.Approach();
                    }
                }
            }
            catch { }
        }

        #endregion
    }
}
