using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Kedrah.Constants;
using Tibia.Constants;

namespace Kedrah.Objects
{
    public class Target : CreatureData
    {
        public FightActions Action;
        public byte Priority;
        public byte[] HPRange = { 0, 100 };
        public FightSecurity Security;
        public FightStances Stance;
        public Attack AttackMode;
        public Follow FollowMode;
        public List<FightExtraPair> Extra;

        public Target(CreatureData c)
            : this(c, FightActions.None, 0, FightSecurity.Automatic, FightStances.Stand, Attack.FullAttack, Follow.DoNotFollow)
        {
        }

        public Target(CreatureData c, FightActions action, byte priority, FightSecurity security, FightStances stance, Attack attackMode, Follow followMode)
            : base(c.Name, c.HitPoints, c.ExperiencePoints, c.SummonMana, c.ConvinceMana, c.MaxDamage, c.CanIllusion, c.CanSeeInvisible, c.FrontAttack, c.Immunities, c.Strengths, c.Weaknesses, c.Sounds, c.Loot)
        {
            this.Action = action;
            this.Priority = priority;
            this.Security = security;
            this.Stance = stance;
            this.AttackMode = attackMode;
            this.FollowMode = followMode;
            this.Extra = new List<FightExtraPair>();
        }
    }
}
