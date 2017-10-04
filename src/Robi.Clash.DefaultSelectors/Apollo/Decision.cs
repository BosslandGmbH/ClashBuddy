﻿using Robi.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robi.Clash.DefaultSelectors.Apollo
{
    class Decision
    {
        public static readonly ILogger Logger = LogProvider.CreateLogger<Decision>();

        public static bool CanWaitDecision(Playfield p, FightState currentSituation)
        {
            if (p.noEnemiesOnMySide())
            {
                switch (currentSituation)
                {
                    //case FightState.DLPT:
                    //case FightState.DRPT:
                    //    break;
                    //case FightState.DKT:
                    //    break;
                    case FightState.APTL1:
                        {
                            if (p.BattleTime.TotalSeconds < 10)
                                return false;
                            else if (p.enemyPrincessTower1.HP < 300 && p.enemyPrincessTower1.HP > 0)
                                return false;
                            break;
                        }
                    case FightState.APTL2:
                        {
                            if (p.BattleTime.TotalSeconds < 10)
                                return false;
                            else if (p.enemyPrincessTower2.HP < 300 && p.enemyPrincessTower2.HP > 0)
                                return false;
                            break;
                        }
                    case FightState.AKT:
                        {
                            if (p.BattleTime.TotalSeconds < 10)
                                return false;
                            else if (p.enemyKingsTower.HP < 300 && p.enemyKingsTower.HP > 0)
                                return false;
                            break;
                        }
                }
            }
            else
            {
                if (p.BattleTime.TotalSeconds < 15)
                    return false;
                if (p.ownKingsTower.HP < 500)
                    return false;
                if (Helper.IsAnEnemyObjectInArea(p, p.ownKingsTower.Position, 4000, boardObjType.MOB))
                    return false; // ToDo: Find better condition, if the handcard can´t attack the minions, we should wait
            }

            return true;
        }

        public static FightState DefenseDecision(Playfield p)
        {
            // There a no dangerous or own important minions on the playfield
            // This is why we are deciding independent of the minions on the field

            if (p.ownTowers.Count < 3)
                return FightState.DKT;

            BoardObj princessTower = p.enemyPrincessTowers.OrderBy(n => n.HP).FirstOrDefault(); // Because they are going to attack this tower

            if (princessTower.Line == 2)
                return FightState.DPTL2;
            else
                return FightState.DPTL1;
        }

        public static FightState AttackDecision(Playfield p)
        {
            if (p.enemyTowers.Count < 3)
                return FightState.AKT;


            BoardObj princessTower = p.enemyPrincessTowers.OrderBy(n => n.HP).FirstOrDefault();

            if (princessTower.Line == 2)
                return FightState.APTL2;
            else
                return FightState.APTL1;
        }

        public static FightState DangerousSituationDecision(Playfield p, int line)
        {
            if (p.ownTowers.Count > 2)
            {
                if (line == 2)
                    return FightState.UAPTL2;
                else if (line == 1)
                    return FightState.UAPTL1;
                else
                    return FightState.UAKT;
            }
            else
            {
                return FightState.UAKT;
            }
        }

        public static FightState GoodAttackChanceDecision(Playfield p, int line)
        {
            if (p.enemyTowers.Count < 3)
                return FightState.AKT;

            if (line == 2)
                return FightState.APTL2;
            else
                return FightState.APTL1;
        }

        public static FightState GameBeginningDecision(Playfield p, out bool gameBeginning)
        {
            bool StartFirstAttack = true;
            gameBeginning = true;

            // Debugging: try - catch is just for debugging
            try
            {
                StartFirstAttack = (p.ownMana < Setting.ManaTillFirstAttack);
            }
            catch (Exception) { }

            if (StartFirstAttack)
            {
                if (!p.noEnemiesOnMySide())
                    gameBeginning = false;

                return FightState.START;
            }
            else
            {
                gameBeginning = false;
                BoardObj obj = Helper.GetNearestEnemy(p);

                if (obj.Line == 2)
                    return FightState.DPTL2;
                else
                    return FightState.DPTL1;
            }
        }

        public static bool SupportDeployment(Playfield p, int line)
        {
            // If own characters already attacking and you are deploying as support
            // The chars should be deployed behind the own chars
            IEnumerable<BoardObj> attackingChars = p.ownMinions.Where(n => n.Line == line && Helper.IsObjectAtOtherSide(p, n));

            // Maybe check also which card type: Tank deployed in front, Ranger behinde ...

            if (attackingChars.Count() > 0)
                return true;
            else
                return false;
        }

        public static int DangerOrBestAttackingLine(Playfield p) // Good chance for an attack?
        {
            Line[] lines = PlayfieldAnalyse.lines;
            // comparison
            // ToDo: Use line danger and chance analyses

            if (lines[0].ComparisionHP == 0 && lines[1].ComparisionHP == 0)
                return 0;


            if (lines[0].Danger > lines[0].Chance)
            {
                if(lines[1].Danger > lines[1].Chance)
                {
                    if (lines[0].Danger > lines[1].Danger)
                        return -1;
                    else
                        return -2;
                }
                else
                {
                    if (lines[0].Danger > lines[1].Chance)
                        return -1;
                    else
                        return 2;
                }
            }
            else
            {
                if (lines[1].Danger > lines[1].Chance)
                {
                    if (lines[0].Chance > lines[1].Danger)
                        return 1;
                    else
                        return -2;
                }
                else
                {
                    if (lines[0].Chance > lines[1].Chance)
                        return 1;
                    else
                        return 2;
                }
            }




            #region check if buildings can attack own towers
            List<BoardObj> enemyBuildings = p.enemyBuildings;

            if (enemyBuildings?.Count() > 0)
            {
                BoardObj bKT = enemyBuildings.Where(n => n.IsPositionInArea(p, p.ownKingsTower.Position)).FirstOrDefault();
                BoardObj bPT1 = enemyBuildings.Where(n => n.IsPositionInArea(p, p.ownPrincessTower1.Position)).FirstOrDefault();
                BoardObj bPT2 = enemyBuildings.Where(n => n.IsPositionInArea(p, p.ownPrincessTower2.Position)).FirstOrDefault();

                if (bKT != null)
                    return 3;

                if (bPT1 != null)
                    return 1;

                if (bPT2 != null)
                    return 2;
            }
            #endregion


            return 0;

            #region just as comments (.attacker is not implemented atm)
            // Check if building attacks Tower (.attacker is not implemented atm)
            //if (p.ownKingsTower?.attacker?.type == boardObjType.BUILDING)
            //    return 3;
            //if (p.ownPrincessTower1?.attacker?.type == boardObjType.BUILDING)
            //    return 1;
            //if (p.ownPrincessTower2?.attacker?.type == boardObjType.BUILDING)
            //    return 2;
            #endregion
        }

        public static BoardObj GetBestDefender(Playfield p)
        {
            // TODO: Find better condition
            BoardObj enemy = Helper.EnemyCharacterWithTheMostEnemiesAround(p, out int count, transportType.NONE);

            if (enemy == null)
                return p.ownKingsTower;

            if (enemy.Line == 2)
                return p.ownPrincessTower2;
            else if (enemy.Line == 1)
                return p.ownPrincessTower1;
            else
                return p.ownKingsTower;

        }

        #region Not used in balanced FightMode
        public static FightState EnemyHasCharsOnTheFieldDecision(Playfield p)
        {
            if (p.ownTowers.Count > 2)
            {
                //BoardObj obj = GetNearestEnemy(p);

                // ToDo: Get most dangeroust group
                group mostDangeroustGroup = p.getGroup(false, 200, boPriority.byTotalBuildingsDPS, 3000);

                if (mostDangeroustGroup == null)
                {
                    Logger.Debug("mostDangeroustGroup = null");
                    return FightState.DKT;
                }
                int line = mostDangeroustGroup.Position.X > 8700 ? 2 : 1;
                Logger.Debug("mostDangeroustGroup.Position.X = {0} ; line = {1}", mostDangeroustGroup?.Position?.X, line);


                if (line == 2)
                    return FightState.DPTL2;
                else
                    return FightState.DPTL1;
            }
            else
            {
                return FightState.DKT;
            }
        }
        public static FightState EnemyIsOnOurSideDecision(Playfield p)
        {
            Logger.Debug("Enemy is on our Side!!");
            if (p.ownTowers.Count > 2)
            {
                BoardObj obj = Helper.GetNearestEnemy(p);

                if (obj != null && obj.Line == 2)
                    return FightState.UAPTL2;
                else
                    return FightState.UAPTL1;
            }
            else
            {
                return FightState.UAKT;
            }
        }
        #endregion
    }
}
