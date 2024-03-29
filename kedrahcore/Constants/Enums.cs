﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah.Constants
{
    #region Cavebot

    public enum WaypointType
    {
        Action,
        Approach,
        Node,
        OpenBody,
        Pick,
        Rope,
        Shovel,
        Stand,
        Use,
        Walk
    }

    #endregion

    #region Loot

    public enum LootContainer
    {
        Any = 0,
        Ground = 255
    }

    public enum OpenBodyRule
    {
        None,
        Filtered,
        Allowed,
        All
    }

    #endregion

    #region Targeting

    public enum FightActions
    {
        Attack,
        Follow,
        None
    }

    public enum FightExtra
    {
        Spell,
        ItemEquip,
        ItemUse,
        AutoSpell
    }

    public enum FightStances
    {
        Stand,
        Follow,
        Distance,
        ParryStand,
        ParryFollow
    }

    public enum FightSecurity
    {
        Wave,
        Beam,
        Both,
        Automatic,
        None
    }

    #endregion
}
