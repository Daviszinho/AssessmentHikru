using System.ComponentModel;
using Lib.Repository.Entities;

namespace Lib.Repository.Services;

public static class BattleExtendedService
{
    /*
 Battle Algorithm
For calculating the battle algorithm, take into account the flow below:
The monster with the highest speed makes the first attack, if both speeds are equal, the monster with the higher attack goes first.
For calculating the damage, subtract the defense from the attack (attack - defense); the difference is the damage; if the attack is equal to or lower than the defense, the damage is 1.
Subtract the damage from the HP (HP = HP - damage).
Monsters will battle in turns until one wins; all turns should be calculated in the same request; for that reason, the battle endpoint should return winner data in just one call.
Who wins the battle is the monster who subtracted the enemy’s HP to zero
*/

    public static Monster CalculateBattle(Monster monsterA, Monster monsterB)
    {
        var winner = monsterA; //default winner
        while (monsterA.Hp > 0 && monsterB.Hp > 0) 
        {

            if (monsterA.Speed > monsterB.Speed) //The monster with the highest speed makes the first attack
            {
                monsterB.Hp -= monsterB.Defense - monsterA.Attack<monsterB.Defense ?1:monsterA.Attack;
            }
            else if (monsterA.Speed < monsterB.Speed) //The monster with the highest speed makes the first attack
            {
                monsterA.Hp -= monsterA.Defense - monsterB.Attack < monsterA.Defense ? 1 : monsterA.Attack;
            }
            else //if both speeds are equal
            {
                if (monsterA.Attack > monsterB.Attack) //the monster with the higher attack goes first.
                {
                    monsterB.Hp -= monsterB.Defense - monsterA.Attack < monsterB.Defense ? 1 : monsterA.Attack;
                }
                else if (monsterA.Attack < monsterB.Attack)
                {
                    monsterA.Hp -= monsterA.Defense - monsterB.Attack < monsterA.Defense ? 1 : monsterA.Attack;
                }
            }
            //Who wins the battle is the monster who subtracted the enemy’s HP to zero
            if (monsterA.Hp <= 0)
            {
                winner = monsterB;
            }
            else if (monsterB.Hp <= 0)
            {
                winner = monsterA;
            }
            else {
                (monsterA, monsterB) = (monsterB, monsterA); // Swap monsters for the next turn
            }
        }


        return winner;
    }
}
