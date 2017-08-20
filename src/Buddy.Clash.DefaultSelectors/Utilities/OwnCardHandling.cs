﻿using Buddy.Clash.Engine;
using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using System.Linq;
using Buddy.Common;
using Buddy.Clash.Engine.NativeObjects.LogicData;

namespace Buddy.Clash.DefaultSelectors.Utilities
{
    enum TroopType
    {
        Tank,
        Damager,
        AirAttacker,
        AOE
    };

    enum CardType
    {
        Troop,
        DamagingSpell,
        Defense,
        All,
        NONE
    };

    class OwnCardHandling
    {
        private static readonly ILogger Logger = LogProvider.CreateLogger<EarlyCycleSelector>();

        public static IOrderedEnumerable<Spell> Damaging
        {
            get
            {
                var spells = ClashEngine.Instance.AvailableSpells;
                var DamagingSpells = spells.Where(s => s != null && s.IsValid && String.IsNullOrEmpty(s.SummonCharacter.Name.Value)).OrderBy(s => s.ManaCost);
                return DamagingSpells;
            }
        }

        public static IOrderedEnumerable<Spell> Troop
        {
            get
            {
                var spells = ClashEngine.Instance.AvailableSpells;
                var TroopSpell = spells.Where(s => s != null && s.IsValid && !String.IsNullOrEmpty(s.SummonCharacter.Name.Value)).OrderBy(s => s.ManaCost);
                
                foreach(var s in TroopSpell)
                {
                    Log.Debug("TroopSpell: Name {0} EffektName {1} PushBack {2} ProjectileAOEtoAir {3} " +
                        "ProjectileAOEtoGround {4} Projectile {5} AttacksAir {6} AttacksGround {7} LifeTime {8} SummonCharacterType {9} SummonCharacter {10}",
                                s.Name.Value, s.Effect.Name.Value, s.Pushback, 
                                s.Projectile.AoeToAir, s.Projectile.AoeToGround, s.Projectile.Name.Value
                                , s.SummonCharacter.AttacksAir, s.SummonCharacter.AttacksGround, s.SummonCharacter.LifeTime, s.SummonCharacter.GetType(), s.SummonCharacter.Name.Value);
                }

                return TroopSpell;
            }
        }

        public static IEnumerable<Spell> AirAttack
        {
            get
            {
                return Troop.Where(n => n.SummonCharacter.AttacksAir == 1);
            }
        }

        public static IEnumerable<Spell> GroundAttack
        {
            get
            {
                return Troop.Where(n => n.SummonCharacter.AttacksGround == 1);
            }
        }

        public static IEnumerable<Spell> Flying
        {
            get
            {
                return Troop.Where(n => n.SummonCharacter.FlyingHeight > 0);
            }
        }

        public static IOrderedEnumerable<Spell> CycleCards
        {
            get
            {
                var spells = ClashEngine.Instance.AvailableSpells;
                return spells.Where(s => s != null && s.IsValid && s.ManaCost <= 3).OrderBy(s => s.ManaCost);
            }
        }

        public static IOrderedEnumerable<Spell> PowerCards
        {
            get
            {
                var spells = ClashEngine.Instance.AvailableSpells;
                return spells.Where(s => s != null && s.IsValid && s.ManaCost > 3).OrderByDescending(s => s.ManaCost);
            }
        }

        public static IOrderedEnumerable<Spell> TroopCycleCards
        {
            get
            {
                var spells = Troop;
                return spells.Where(s => s != null && s.IsValid && s.ManaCost <= 3).OrderBy(s => s.ManaCost);
            }
        }

        public static IOrderedEnumerable<Spell> TroopPowerCards
        {
            get
            {
                var spells = Troop;
                return spells.Where(s => s != null && s.IsValid && s.ManaCost > 3).OrderByDescending(s => s.ManaCost);
            }
        }
    }
}
