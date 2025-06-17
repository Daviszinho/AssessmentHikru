using System.Threading;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Lib.Repository.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


public class BattleExtendedController : BaseApiController
{
    private readonly IBattleOfMonstersRepository _repository;

    public BattleExtendedController(IBattleOfMonstersRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Add([FromBody] Battle battle)
    {
        //TBD
        if (battle == null)
        {
            return BadRequest("Battle cannot be null.");
        }

        if (battle.Id==null || battle.MonsterA == null || battle.MonsterB == null || battle.Winner == null)
        {
            //return BadRequest("Battle properties MonsterA, MonsterB, and Winner cannot be null.");
            return BadRequest("Missing ID");
        }

        await _repository.Battles.AddAsync(battle);
        await _repository.Save();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Remove(int id)
    {
        var existingBattle = await _repository.Battles.FindAsync(id);

        if (existingBattle == null)
        {
            //return NotFound($"The battle with ID = {id} not found.");
            return NotFound( );
        }

        await _repository.Battles.RemoveAsync(id);
        await _repository.Save();
        return Ok();
    }
    /*
     Battle Algorithm
For calculating the battle algorithm, take into account the flow below:
The monster with the highest speed makes the first attack, if both speeds are equal, the monster with the higher attack goes first.
For calculating the damage, subtract the defense from the attack (attack - defense); the difference is the damage; if the attack is equal to or lower than the defense, the damage is 1.
Subtract the damage from the HP (HP = HP - damage).
Monsters will battle in turns until one wins; all turns should be calculated in the same request; for that reason, the battle endpoint should return winner data in just one call.
Who wins the battle is the monster who subtracted the enemy’s HP to zero
    */
    public async Task<ActionResult> StartBattle([FromBody] Battle battle)
    {

        if (battle == null)
        {
            return BadRequest("Battle cannot be null.");
        }
        if (battle.MonsterA == null || battle.MonsterB == null)
        {
            return BadRequest("MonsterA and MonsterB cannot be null.");
        }
        var monsterA = await _repository.Monsters.FindAsync(battle.MonsterA.Value);
        var monsterB = await _repository.Monsters.FindAsync(battle.MonsterB.Value);
        if (monsterA == null || monsterB == null)
        {
            return NotFound("One or both monsters not found.");
        }

        // Calculate the battle
        var winner = BattleExtendedService.CalculateBattle(monsterA, monsterB);

        battle.Winner = winner.Id;
        battle.WinnerRelation = winner;
        return Ok(battle);
    }

}
