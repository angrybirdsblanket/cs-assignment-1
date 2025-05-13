using System;
using System.Collections.Generic;
using System.Linq;
using PokemonPocket.Models;
using PokemonPocket.Data;
using Spectre.Console;

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
            Pokemon wild = GenerateRandomPokemon();

            int goldGain;
            bool success = CatchPokemon(wild, out goldGain);
            if (success)
            {
                Player player = this._context.Players
                  .Where(p => p.Id == 1)
                  .First();
                player.Gold += goldGain;
            }

            var levelablePokemon = PokemonService.GetPlayerPokemon(this._context)
              .Where(p => p.Exp >= 100)
              .ToList();

            foreach (Pokemon pokemon in levelablePokemon)
            {
                pokemon.LevelUp();
            }

            this._context.SaveChanges();

            return success ? wild : null;
        }

        private void CalculateExp(Pokemon enemy, Pokemon attacker, int damageDealt)
        {
            const int BaseExp = 50;
            double damageRatio = (double)damageDealt / enemy.MaxHP;
            double rawXp = damageRatio * BaseExp;


            int xpGained = (int)Math.Floor(rawXp);
            if (damageDealt > 0 && xpGained < 1)
                xpGained = 1;

            attacker.Exp += xpGained;
            this._context.SaveChanges();
        }

        private Pokemon GenerateRandomPokemon()
        {
            var pokemonTypes = new List<Func<Pokemon>>
      {
        () => new Pikachu(this._random.Next(180, 301), 0),
        () => new Eevee(this._random.Next(180, 301), 0),
        () => new Charmander(this._random.Next(180, 301), 0)
      };

            Pokemon wild = pokemonTypes[this._random.Next(pokemonTypes.Count)]();
            return wild;
        }

        private bool CatchPokemon(Pokemon wild, out int goldGain)
        {
            goldGain = 0;
            int attempts = 3;
            bool success = false;
            int maxHp = wild.HP;

            AnsiConsole.WriteLine($"\nA wild {wild.Name} with {maxHp} HP appeared!");

            List<Pokemon> pocket = GetAvailablePokemon();


            while (attempts > 0 && !success && wild.HP > 0 && pocket.Count() > 0)
            {
                int selection = SelectPokemon(pocket);
                Pokemon attacker = pocket[selection];

                ExecuteBattleRound(attacker, wild);

                if (HandleFaintedAttacker(pocket, ref selection, attacker))
                    continue;

                success = PromptForCapture(wild, maxHp, ref attempts, out goldGain);

                if (!success && attempts > 0)
                {
                    AnsiConsole.WriteLine($"You have {attempts} attempt(s) left.");
                }
            }

            if (attempts == 0)
                AnsiConsole.WriteLine($"You have run out of attempts, {wild.Name} has fled");
            else if (wild.HP == 0)
                AnsiConsole.WriteLine($"{wild.Name} has fainted");

            return success;
        }

        private List<Pokemon> GetAvailablePokemon()
        {
            return PokemonService.GetPlayerPokemon(this._context)
              .OrderBy(p => p.Id)
              .Where(p => p.HP > 0)
              .ToList();
        }

        private int SelectPokemon(List<Pokemon> pocket)
        {
          var selectedPokemon = AnsiConsole.Prompt(
              new SelectionPrompt<Pokemon>()
              .Title("Choose a Pokémon from your pocket:")
              .PageSize(10)
              .AddChoices(pocket)
              .UseConverter(p => $"{p.Name}, {p.HP} HP, {p.Exp} Exp")
              );

          int selection = pocket.IndexOf(selectedPokemon);

          return selection; 
        }
        private void ExecuteBattleRound(Pokemon attacker, Pokemon wild)
        {
            int damage = attacker.Attack(wild);
            CalculateExp(wild, attacker, damage);
            AnsiConsole.WriteLine($"Your {attacker.Name} attacked the wild {wild.Name} for {damage} damage and left it with {wild.HP} HP!");

            if (wild.HP > 0)
            {
                damage = wild.Attack(attacker);
                AnsiConsole.WriteLine($"The wild {wild.Name} attacked your {attacker.Name} for {damage} damage and left it with {attacker.HP} HP!");
            }
        }

        private bool HandleFaintedAttacker(List<Pokemon> pocket, ref int selection, Pokemon attacker)
        {
            if (attacker.HP <= 0)
            {
                Console.WriteLine($"{attacker.Name} has fainted.");
                pocket.RemoveAt(selection);
                if (pocket.Count == 0)
                {
                    Console.WriteLine("You have no more Pokémon left. The wild Pokémon fled.");
                    return true;
                }
                selection = SelectPokemon(pocket);
                return true;
            }
            return false;
        }

        private bool PromptForCapture(Pokemon wild, int maxHp, ref int attempts, out int goldGain)
        {
            goldGain = 0;

            if (wild.HP == 0) {
              AnsiConsole.WriteLine($"The wild {wild.Name} has already fainted and cannot be caught");
              return false;
            }

            Console.Write("Would you like to try and capture?");
            string input = AnsiConsole.Prompt<String>(
                      new SelectionPrompt<String>()
                        .Title("Would you like to try capture?")
                        .PageSize(10)
                        .AddChoices("Yes", "No")
                        );
            if (input == "Yes")
            {
                if (AttemptCatch(wild, maxHp))
                {
                    double healthPercentage = (double)wild.HP / maxHp;
                    int baseGold = 20;
                    goldGain = (int)(baseGold * healthPercentage);

                    Console.WriteLine($"You successfully caught the {wild.Name} and gained {goldGain} gold!");
                    wild.HP = maxHp;
                    wild.MaxHP = maxHp;
                    return true;
                }

                AnsiConsole.WriteLine("Capture failed.");
                attempts--;

                if (attempts > 0)
                {
                  input = AnsiConsole.Prompt<String>(
                      new SelectionPrompt<String>()
                        .Title("Would you like to try capture?")
                        .PageSize(10)
                        .AddChoices("Yes", "No")
                        );

                    if (!string.IsNullOrEmpty(input) && input[0] == 'y')
                    {
                        if (AttemptCatch(wild, maxHp))
                        {
                            double healthPercentage = (double)wild.HP / maxHp;
                            int baseGold = 20;
                            goldGain = (int)(baseGold * healthPercentage);

                            Console.WriteLine($"You successfully caught the {wild.Name} and gained {goldGain} gold!");
                            wild.HP = maxHp;
                            return true;
                        }
                    }
                }

                attempts--;
            }

            return false;
        }
        private bool AttemptCatch(Pokemon wild, int maxHp)
        {
              int percentageChance = (int)(((double)(maxHp - wild.HP) / maxHp) * 100);
              return this._random.Next(1, 101) <= percentageChance;
        }
    }
}
