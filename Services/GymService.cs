using System;
using System.Collections.Generic;
using PokemonPocket.Data;
using PokemonPocket.Models;
using System.Linq;

namespace PokemonPocket.Services {

  public class GymService {

    private readonly PokemonPocketContext _context;

    public GymService(PokemonPocketContext context) {
      this._context = context;
    }

    private void displayGymMenu() {
      Console.WriteLine("(1) Check Upcoming Gym battles");
      Console.WriteLine("(2) Check next Gym Leader's Team");
      Console.WriteLine("(3) Start Gym fight");
      Console.Write("Please enter [1,2,3] or B to go back: ");
    }

    public bool handleGymMenu() {
      this.displayGymMenu();

      string input;

      input = Console.ReadLine();
      while (string.IsNullOrEmpty(input)) {
        Console.Clear();
        Console.WriteLine("No input was detected, please try again");
        this.displayGymMenu();
        input = Console.ReadLine();
      }

      input = input.ToLower();

      switch(input) {
        case "1":
          listGyms();
          return true;
        case "2":
          getNextFightPokemon();
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

    public void InitialiseGyms()
    {
      // Check if gym leaders have already been added
      var firstLaunchTest = this._context.GymLeaders.FirstOrDefault();
      if (firstLaunchTest == null)
      {
        // Define gym leaders with exactly 3 Pokémon each, using their default skills
        GymLeader leader_1 = new GymLeader
        {
          GymName = "Thunderdome",
          BadgeName = "Electric Badge",
          PokemonTeam = new List<Pokemon>
          {
            new Pikachu { Name = "Pikachu", HP = 200, MaxHP = 200, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 30, Level = 2 },
            new Raichu { Name = "Raichu", HP = 400, MaxHP = 400, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 40, Level = 3 },
            new Eevee { Name = "Eevee", HP = 300, MaxHP = 300, Exp = 0, Skill = "Run Away", SkillDamage = 20, Level = 2 }
          }
        };

        GymLeader leader_2 = new GymLeader
        {
          GymName = "Verdant Grove",
          BadgeName = "Grass Badge",
          PokemonTeam = new List<Pokemon>
          {
            new Bulbasaur { Name = "Bulbasaur", HP = 325, MaxHP = 325, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 25, Level = 3 },
            new Ivysaur { Name = "Ivysaur", HP = 450, MaxHP = 450, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 30, Level = 4 },
            new Eevee { Name = "Eevee", HP = 300, MaxHP = 300, Exp = 0, Skill = "Run Away", SkillDamage = 15, Level = 2 }
          }
        };

        GymLeader leader_3 = new GymLeader
        {
          GymName = "Blazing Inferno",
          BadgeName = "Fire Badge",
          PokemonTeam = new List<Pokemon>
          {
            new Charmander { Name = "Charmander", HP = 350, MaxHP = 350, Exp = 0, Skill = "Solar Power", SkillDamage = 20, Level = 3 },
            new Flareon { Name = "Flareon", HP = 450, MaxHP = 450, Exp = 0, Skill = "Run Away", SkillDamage = 40, Level = 4 },
            new Charmeleon { Name = "Charmeleon", HP = 500, MaxHP = 500, Exp = 0, Skill = "Solar Power", SkillDamage = 35, Level = 5 }
          }
        };

        GymLeader leader_4 = new GymLeader
        {
          GymName = "Mystic Lagoon",
          BadgeName = "Water Badge",
          PokemonTeam = new List<Pokemon>
          {
            new Ivysaur { Name = "Ivysaur", HP = 500, MaxHP = 500, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 35, Level = 5 },
            new Eevee { Name = "Eevee", HP = 525, MaxHP = 525, Exp = 0, Skill = "Run Away", SkillDamage = 25, Level = 4 },
            new Flareon { Name = "Flareon", HP = 450, MaxHP = 450, Exp = 0, Skill = "Run Away", SkillDamage = 30, Level = 5 }
          }
        };

        GymLeader leader_5 = new GymLeader
        {
          GymName = "Dragon's Den",
          BadgeName = "Dragon Badge",
          PokemonTeam = new List<Pokemon>
          {
            new Charmeleon { Name = "Charmeleon", HP = 550, MaxHP = 550, Exp = 0, Skill = "Solar Power", SkillDamage = 40, Level = 6 },
            new Raichu { Name = "Raichu", HP = 600, MaxHP = 600, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 50, Level = 7 },
            new Ivysaur { Name = "Ivysaur", HP = 525, MaxHP = 525, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 45, Level = 6 }
          }
        };

        // Add all gym leaders to the database
        this._context.GymLeaders.AddRange(new[] { leader_1, leader_2, leader_3, leader_4, leader_5 });
        this._context.SaveChanges();
      }
    }
    private void viewGymStatus () {

    }

    private void listGyms() {
      var gyms = this._context.GymLeaders
        .ToList();

      Console.WriteLine("The following gyms are coming up");

      int gym = 1;
      foreach (GymLeader leader in gyms) {
        Console.WriteLine($"{gym}. {leader.GymName}");
        gym++;
      }
    }

    private void getNextFightPokemon() {
      var player = this._context.Players.FirstOrDefault();

      var nextLeader = this._context.GymLeaders
        .Skip(player.Badges.Count())
        .FirstOrDefault();

      if (nextLeader != null) {
        Console.WriteLine($"The {nextLeader.GymName} is next");
        Console.WriteLine("You will be fighting the following pokemon");

        foreach (Pokemon pokemon in nextLeader.PokemonTeam) {
          Console.WriteLine($"{pokemon.Name}: {pokemon.MaxHP} HP, Level {pokemon.Level}");
        }

      }

    }

  }

}
