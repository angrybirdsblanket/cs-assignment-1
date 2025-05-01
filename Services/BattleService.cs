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
            Pokemon wild = GenerateRandomPokemon();

            bool success = CatchPokemon(wild);
            return success ? wild : null;
        }

        private Pokemon GenerateRandomPokemon()
        {
            var pokemonTypes = new List<Func<Pokemon>>
            {
                () => new Pikachu(),
                () => new Eevee(),
                () => new Charmander()
            };

            Pokemon wild = pokemonTypes[_random.Next(pokemonTypes.Count)]();
            wild.HP = _random.Next(180, 301);
            wild.Exp = 0;
            wild.Name = wild.GetType().Name;

            return wild;
        }

        private bool CatchPokemon(Pokemon wild)
        {
            int attempts = 3;
            bool success = false;
            int maxHp = wild.HP;

            Console.WriteLine($"\nA wild {wild.Name} with {maxHp} HP appeared!");

            List<Pokemon> pocket = GetAvailablePokemon();


            while (attempts > 0 && !success && wild.HP > 0 && pocket.Count() > 0)
            {
                int selection = SelectPokemon(pocket);
                Pokemon attacker = pocket[selection];

                ExecuteBattleRound(attacker, wild);

                if (HandleFaintedAttacker(pocket, ref selection, attacker))
                    continue;

                success = PromptForCapture(wild, maxHp, ref attempts);

                if (!success && attempts > 0)
                {
                    Console.WriteLine($"You have {attempts} attempt(s) left.");
                }
            }

            if (attempts == 0)
                Console.WriteLine($"You have run out of attempts, {wild.Name} has fled");
            else if (wild.HP == 0)
                Console.WriteLine($"{wild.Name} has fainted");

            return success;
        }

        private List<Pokemon> GetAvailablePokemon()
        {
            return _context.Pokemon
                .OrderBy(p => p.Id)
                .Where(p => p.HP > 0)
                .ToList();
        }

        private int SelectPokemon(List<Pokemon> pocket)
        {
            int selection = -1;

            while (selection < 0 || selection >= pocket.Count)
            {
                Console.WriteLine("Choose a Pokémon from your pocket:");
                for (int i = 0; i < pocket.Count; i++)
                {
                    var p = pocket[i];
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

        private void ExecuteBattleRound(Pokemon attacker, Pokemon wild)
        {
            attacker.Attack(wild);
            Console.WriteLine($"{attacker.Name} attacked {wild.Name} and left it with {wild.HP} HP!");

            if (wild.HP > 0)
            {
                wild.Attack(attacker);
                Console.WriteLine($"{wild.Name} attacked {attacker.Name} and left it with {attacker.HP} HP!");
            }
        }

        private bool HandleFaintedAttacker(List<Pokemon> pocket, ref int selection, Pokemon attacker)
        {
            if (attacker.HP <= 0)
            {
                Console.WriteLine($"{attacker.Name} has fainted and is removed from your pocket.");
                pocket.RemoveAt(selection);
                if (pocket.Count == 0)
                {
                    Console.WriteLine("You have no more Pokémon left. The wild Pokémon fled.");
                    return true; // end battle
                }
                selection = SelectPokemon(pocket);
                return true; // skip rest of loop
            }
            return false;
        }

        private bool PromptForCapture(Pokemon wild, int maxHp, ref int attempts)
        {
            Console.Write("Would you like to try and capture? (y/n): ");
            string input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input) && input[0] == 'y')
            {
                if (AttemptCatch(wild, maxHp))
                {
                    Console.WriteLine($"You successfully caught the {wild.Name}!");
                    wild.HP = maxHp;
                    return true;
                }
                else
                {
                    Console.WriteLine("Capture failed.");
                    attempts--;

                    if (attempts > 0)
                    {
                        Console.WriteLine("Attempt to catch again? (y/n): ");
                        input = Console.ReadLine();

                        if (!string.IsNullOrEmpty(input) && input[0] == 'y')
                        {
                            if (AttemptCatch(wild, maxHp))
                            {
                                Console.WriteLine($"You successfully caught the {wild.Name}!");
                                wild.HP = maxHp;
                                return true;
                            }
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
            return _random.Next(1, 101) <= percentageChance;
        }
    }
}

