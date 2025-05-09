using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using PokemonPocket.Data;
using PokemonPocket.Models;
using Microsoft.EntityFrameworkCore;
namespace PokemonPocket.Services

{
  public class PokemonService
  {
    private readonly PokemonPocketContext _context;
    private readonly BattleService _battles;
    private readonly GymService _gyms;

    public PokemonService(PokemonPocketContext context, BattleService battles, GymService gyms)
    {
      this._context = context;
      this._battles = battles;
      this._gyms = gyms;
    }

    private void addPokemon()
    {

      Console.Write("Enter Pokemon's Name: ");
      string name = Console.ReadLine();

      if (!new[] { "pikachu", "eevee", "charmander", "bulbasaur"}.Contains(name.ToLower()))
      {
        Console.WriteLine("Pokemon not recognised.");
        return;
      }

      Console.Write("Enter Pokemon's HP: ");
      int hp = Int32.Parse(Console.ReadLine());

      Console.Write("Enter Pokemon's Exp: ");
      int exp = Int32.Parse(Console.ReadLine());

      switch (name.ToLower())
      {

        case "pikachu":

          Pikachu pikachu = new Pikachu(hp, exp);

          this._context.Add(pikachu);
          break;

        case "charmander":
          Charmander charmander = new Charmander(hp, exp);

          this._context.Add(charmander);

          break;

        case "eevee":
          Eevee eevee = new Eevee(hp, exp);

          this._context.Add(eevee);

          break;
      }

      this._context.SaveChanges();
    }

    private void listPokemon()
    {
      var pokemon = PokemonService.GetPlayerPokemon(this._context);

      if (pokemon.Count == 0)
      {
        Console.WriteLine("Your Pocket is empty");
      }

      foreach (var p in pokemon)
      {
        Console.WriteLine($"Name: {p.Name}");
        Console.WriteLine($"HP: {p.HP}");
        Console.WriteLine($"Exp: {p.Exp}");
        Console.WriteLine($"Skill Name: {p.Skill}");
        Console.WriteLine($"Current Level: {p.Level}");
        Console.WriteLine($"------------------------------");
      }
    }

    private void checkEvolutionStatus()
    {
      List<PokemonMaster> rules = this._context.EvolutionRules
        .ToList();
      bool eligibleEvolutions = false;
      foreach (PokemonMaster rule in rules)
      {
        var list = PokemonService.GetPlayerPokemon(this._context)
          .Where(p => p.Name == rule.Name)
          .ToList();

        if (list.Count >= rule.NoToEvolve)
        {
          eligibleEvolutions = true;
          int eligibleCount = list.Count / rule.NoToEvolve * rule.NoToEvolve;

          Console.WriteLine($"{eligibleCount} {rule.Name} ---> {eligibleCount / rule.NoToEvolve} {rule.EvolveTo}");
        }
      }
      if (!eligibleEvolutions) {
        Console.WriteLine("You currently have no eligible pokemon for evolution");
      }
    }

    private void evolveEligiblePokemon()
    {
      var rules = _context.EvolutionRules.ToList();

      foreach (var rule in rules)
      {
        var pocket = PokemonService.GetPlayerPokemon(this._context)
          .Where(p => p.Name == rule.Name)
          .OrderByDescending(p => p.MaxHP)
          .ThenByDescending(p => p.Exp)
          .ToList();

        int fullGroups = pocket.Count / rule.NoToEvolve;

        for (int g = 0; g < fullGroups; g++)
        {
          var batch = pocket.Skip(g * rule.NoToEvolve)
            .Take(rule.NoToEvolve)
            .ToList();

          int maxHp = batch.Max(p => p.MaxHP);
          int maxExp = batch.Max(p => p.Exp);
          int maxLevel = batch.Max(p => p.Level); 
          int maxSkillDamage = batch.Max(p => p.SkillDamage);

          this._context.RemoveRange(batch);

          Pokemon evolved = rule.EvolveTo.ToLower() switch
          {
            "raichu" => new Raichu { HP = maxHp, Exp = maxExp, Name = "Raichu", Level = maxLevel, SkillDamage = maxSkillDamage, MaxHP = maxHp },
            "charmeleon" => new Charmeleon { HP = maxHp, Exp = maxExp, Name = "Charmeleon", Level = maxLevel, SkillDamage = maxSkillDamage, MaxHP = maxHp },
            "flareon" => new Flareon { HP = maxHp, Exp = maxExp, Name = "Flareon", Level = maxLevel, SkillDamage = maxSkillDamage, MaxHP = maxHp },
            "ivysaur" => new Ivysaur { HP = maxHp, Exp = maxExp, Name = "Ivysaur", Level = maxLevel, SkillDamage = maxSkillDamage, MaxHP = maxHp },
            _ => throw new InvalidOperationException($"Unknown evolution target: {rule.EvolveTo}")
          };

        }
      }

      this._context.SaveChanges();
    }

    private void playerStats() {
      Player player = this._context.Players
        .Where(p => p.Id == 1)
        .First();

      List<Pokemon> healablePokemon = PokemonService.GetPlayerPokemon(this._context)
        .Where(p => p.HP != p.MaxHP)
        .ToList();

      Console.WriteLine($"You currently have {player.Gold} gold, and {healablePokemon.Count()} of your pokemon currently require healing");
    }

    private void healPokemon() {
      var pokemonList = PokemonService.GetPlayerPokemon(this._context)
        .OrderBy(p => p.Id)
        .Where(p => p.HP != p.MaxHP)
        .ToList();

      int totalHealthPercentMissing = 0;
      foreach (Pokemon pokemon in pokemonList) {
        int healthPercentMissing = (int)(((double)(pokemon.MaxHP - pokemon.HP) / pokemon.MaxHP) * 100);            
        totalHealthPercentMissing += healthPercentMissing;
      }

      int goldRequired = Max(totalHealthPercentMissing / 10, 1);

      Console.Write($"To heal your Pokemon, you need to pay {goldRequired} gold. Would you like to proceed? (y/n): ");
      string response = Console.ReadLine();
      response.ToLower();

      while (string.IsNullOrEmpty(response) || (response[0] != 'y' && response[0] != 'n')) {
        Console.Write($"Your response is not recognised, please try again: ");
        response = Console.ReadLine();
        response.ToLower();
      }

      if (response[0] == 'y') {
        Player player = this._context.Players.First();
        if (player.Gold >= goldRequired) {
          player.Gold -= goldRequired;
          foreach (Pokemon pokemon in pokemonList) {
            pokemon.Heal();
          }
          Console.WriteLine("Your Pokemon have been healed!");
          this._context.SaveChanges();
        }
        else Console.WriteLine("You do not have enough gold, please come back when you gather more");
      }

    }

    private void drawMainMenu()
    {
      Console.WriteLine("*****************************");
      Console.WriteLine("Welcome to Pokemon Pocket App");
      Console.WriteLine("*****************************");
      Console.WriteLine("(1). Add Pokemon to my Pocket");
      Console.WriteLine("(2). List Pokemon(s) in my Pocket");
      Console.WriteLine("(3). Check if I can evolve my Pokemon");
      Console.WriteLine("(4). Evolve Pokemon");
      Console.WriteLine("(5). Player Menu");
      Console.WriteLine("(6). Gym Menu");
      Console.Write("Please enter [1,2,3,4,5,6] or Q to quit: ");
    }

    private void drawPlayerMenu() {
      Console.WriteLine("(1). Check your player Statistics");
      Console.WriteLine("(2). Heal your Pokemon");
      Console.WriteLine("(3). Fight and catch a Pokemon");
      Console.Write("Please enter [1,2,3] or B to go back: ");
    }

    private bool handlePlayerMenu() {
      this.drawPlayerMenu();
      string input;

      input = Console.ReadLine();
      while (string.IsNullOrEmpty(input)) {
        Console.WriteLine("No input was detected, please try again");
        input = Console.ReadLine();
      }
      

      switch (input) {
        case "1":
          this.playerStats();
          return true;
        case "2": 
          this.healPokemon();
          return true;
        case "3":

          var playerPokemon = PokemonService.GetPlayerPokemon(this._context);

          if (playerPokemon.Count() > 0)
          {
            Pokemon capturedPokemon = this._battles.Capture();

            if (capturedPokemon is Pokemon)
            {
              this._context.Add(capturedPokemon);
              this._context.SaveChanges();
            }

          }
          else
          {
            Console.WriteLine("Your Pocket is empty, please come back once you have pokemon inside.");
          }

          return true;

        case "b":
          return true;
        default:
          Console.Clear();
          Console.WriteLine("An invalid character was detected, please try again");
          Console.WriteLine();
          return false;
      }
    }


    public bool GetNextAction()
    {
      this.drawMainMenu();

      string input = Console.ReadLine();

      if (string.IsNullOrEmpty(input))
      {
        Console.WriteLine("Invalid input, please try again");
        return true;
      }


      Console.Clear();
      switch (input)
      {
        case "1":
          addPokemon();
          break;
        case "2":
          listPokemon();
          break;
        case "3":
          checkEvolutionStatus();
          break;
        case "4":
          evolveEligiblePokemon();
          break;
        case "5":
          bool playerFinished = false;
          while (!playerFinished) {
            playerFinished = handlePlayerMenu();
          }
          break;
        case "6":
          bool gymFinished = false;
            while (!gymFinished) {
              gymFinished = this._gyms.HandleGymMenu();
            }
          break;
        case "7":
          testPokemon();
          break;
        case "q":
        case "Q":
          return false;
        default:
          Console.WriteLine("An Invalid character was detected, please try again");
          return true;
      }
      return true;
    }

    public void InitialiseEvoRules()
    {

      var pikachu_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Pikachu");
      if (!(pikachu_query is PokemonMaster))
      {
        PokemonMaster pikachu = new PokemonMaster()
        {
          Name = "Pikachu",
          NoToEvolve = 2,
          EvolveTo = "Raichu"
        };
        this._context.EvolutionRules.Add(pikachu);
      }

      var eevee_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Eevee");
      if (!(eevee_query is PokemonMaster))
      {
        PokemonMaster eevee = new PokemonMaster()
        {
          Name = "Eevee",
          NoToEvolve = 3,
          EvolveTo = "Flareon"
        };
        this._context.EvolutionRules.Add(eevee);
      }

      var charmander_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Charmander");
      if (!(charmander_query is PokemonMaster))
      {
        PokemonMaster charmander = new PokemonMaster()
        {
          Name = "Charmander",
          NoToEvolve = 1,
          EvolveTo = "Charmeleon"
        };
        this._context.EvolutionRules.Add(charmander);


      }

      var bulbasaur_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Bulbasaur");
      if(!(bulbasaur_query is PokemonMaster)) {
        PokemonMaster bulbasaur = new PokemonMaster() {
          Name = "Bulbasaur",
          NoToEvolve = 5,
          EvolveTo = "Ivysaur"
        };
        this._context.Add(bulbasaur);
      }


      this._context.SaveChanges();
    }

    public static List<Pokemon> GetPlayerPokemon(PokemonPocketContext context)
    {
      return context.Pokemon
        .Where(p => EF.Property<int?>(p, "GymLeaderId") == null) 
        .OrderByDescending(p => p.Exp)  
        .ToList(); 
    }

    private void testPokemon() {
      var pikachu = new Pikachu() {
        Name = "Pikachu",
        HP = 1000,
        MaxHP = 1000,
        Exp = 0,
        Skill = "Lightning Bolt",
        SkillDamage = 100,
        Level = 40
      };

      var bulbasaur = new Bulbasaur() {
        Name = "Bulbasaur",
        HP = 1000,
        MaxHP = 1000,
        Exp = 0,
        Skill = "Verdant Spiral",
        SkillDamage = 100,
        Level = 40
      };

      var eevee = new Eevee() {
        Name = "Eevee",
        HP = 1000,
        MaxHP = 1000,
        Exp = 0,
        Skill = "Run Away",
        SkillDamage = 100,
        Level = 40
      };

      this._context.AddRange(pikachu, eevee, bulbasaur);
      this._context.SaveChanges();

    }

  }
}

