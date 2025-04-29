using System;
using System.Collections.Generic;
using System.Linq;
using PokemonPocket.Models;
using PokemonPocket.Data;

namespace PokemonPocket.Services
{
  public class BattleService
  {
    private readonly PokemonPocketContext _context;
    private readonly Random _random = new Random();

    public BattleService(PokemonPocketContext context)
    {
      this._context = context;
    }

    public Pokemon Capture()
    {
      Pokemon pokemon = GenerateRandomPokemon();

      bool success = catchPokemon(pokemon);
      return success ? pokemon : null;
    }

    private Pokemon GenerateRandomPokemon()
    {
      var pokemonTypes = new List<Func<Pokemon>>
      {
        () => new Pikachu(),
        () => new Eevee(),
        () => new Charmander()
      };

      Pokemon pokemon = pokemonTypes[_random.Next(pokemonTypes.Count)]();
      pokemon.HP = _random.Next(180, 301);
      pokemon.Exp = 0;
      pokemon.Name = pokemon.GetType().Name;

      return pokemon;
    }

    private bool catchPokemon(Pokemon pokemon)
    {
      int attempts = 3;
      bool success = false;
      int maxHp = pokemon.HP;

      Console.WriteLine($"\nA wild {pokemon.Name} appeared!");

      List<Pokemon> pocket = _context.Pokemon.OrderBy(p => p.Id).ToList();

      int selection = selectPokemon(pocket);

      while (attempts > 0 && !success && pokemon.HP > 0)
      {
        Pokemon attacker = pocket[selection];
        attacker.Attack(pokemon);

        Console.WriteLine($"{attacker.Name} attacked {pokemon.Name} and left it with {pokemon.HP} HP!");

        Console.Write("Would you like to try and capture? (y/n): ");
        string input = Console.ReadLine();

        if (!string.IsNullOrEmpty(input) && input[0] == 'y')
        {
          success = attemptCatch(pokemon, maxHp);
          if (success)
          {
            Console.WriteLine($"You successfully caught the {pokemon.Name}!");
            break;
          }
          else
          {
            Console.WriteLine("Capture failed.");
            attempts--;

            Console.WriteLine("Attempt to catch again? (y/n): ");
            input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input) && input[0] == 'y')
            {
              success = attemptCatch(pokemon, maxHp);
              if (success)
              {
                Console.WriteLine($"You successfully caught the {pokemon.Name}!");
                pokemon.HP = maxHp;
                break;
              }
            }

          }
          attempts--;
        }


        if (attempts > 0 && !success)
        {
          Console.WriteLine($"You have {attempts} attempt(s) left.");
          selection = selectPokemon(pocket);
        }
      }
      if (attempts == 0) {
        Console.WriteLine($"You have run out of attepmts, {pokemon.Name} has fled");
      } 
      else if (pokemon.HP == 0) {
        Console.WriteLine($"{pokemon.Name} has fainted");
      }

      return success;
    }

    private int selectPokemon(List<Pokemon> pocket)
    {
      int selection = -1;

      while (selection < 0 || selection >= pocket.Count)
      {
        Console.WriteLine("Choose a Pokémon from your pocket:");
        for (int i = 0; i < pocket.Count; i++)
        {
          Pokemon p = pocket[i];
          Console.WriteLine($"{i}: --> {p.Name}, {p.HP} HP, {p.Exp} Exp");
        }

        Console.Write("Your choice: ");
        string input = Console.ReadLine();

        if (!int.TryParse(input, out selection) || selection < 0 || selection >= pocket.Count)
        {
          Console.WriteLine("Invalid selection. Please try again.");
          selection = -1;
        }
      }

      return selection;
    }

    private bool attemptCatch(Pokemon pokemon, int maxHp)
    {
      int percentageChance = (int)(((double)(maxHp - pokemon.HP) / maxHp) * 100);
      return _random.Next(1, 101) <= percentageChance;
    }
  }
}

