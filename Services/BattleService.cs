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
            int attempts = 3; // Total capture attempts available
            bool success = false;
            int maxHp = wild.HP;

            AnsiConsole.MarkupLine($"[bold yellow]\nA wild {wild.Name} with {maxHp} HP appeared![/]");

            List<Pokemon> pocket = GetAvailablePokemon();

            while (attempts > 0 && wild.HP > 0 && pocket.Count > 0)
            {
                // Player selects a Pokémon to battle
                int selection = SelectPokemon(pocket);
                Pokemon attacker = pocket[selection];

                // Execute one battle round
                ExecuteBattleRound(attacker, wild);

                // Handle fainted Pokémon in the player's team
                if (HandleFaintedAttacker(pocket, ref selection, attacker))
                    continue;

                // Allow up to 3 capture attempts after each attack
                for (int i = 0; i < 3 && attempts > 0; i++)
                {
                    bool userWantsToCapture; // Track if the user chose "Yes"
                    success = PromptForCapture(wild, maxHp, ref attempts, out goldGain, out userWantsToCapture);

                    // Exit the loop if capture is successful
                    if (success)
                        break;

                    // Exit the loop if the user does not want to capture further
                    if (!userWantsToCapture)
                        break;
                }

                // Exit the outer loop if capture is successful
                if (success)
                    break;
            }

            if (attempts == 0 && !success)
                AnsiConsole.MarkupLine($"[red]You have run out of attempts. The wild {wild.Name} has fled![/]");
            else if (wild.HP == 0 && !success)
                AnsiConsole.MarkupLine($"[red]{wild.Name} has fainted and cannot be caught![/]");

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
            return AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("[bold green]Choose a Pokémon from your pocket:[/]")
                    .PageSize(10)
                    .AddChoices(pocket.Select((p, index) => index).ToList())
                    .UseConverter(i =>
                    {
                        var p = pocket[i];
                        return $"{p.Name}, HP: {p.HP}/{p.MaxHP}, Level: {p.Level}, Exp: {p.Exp}/100";
                    })
            );
        }

        private void ExecuteBattleRound(Pokemon attacker, Pokemon wild)
        {
            int damage = attacker.Attack(wild);
            CalculateExp(wild, attacker, damage);
            AnsiConsole.MarkupLine($"[cyan]Your {attacker.Name} attacked the wild {wild.Name} for {damage} damage and left it with {wild.HP} HP![/]");

            if (wild.HP > 0)
            {
                damage = wild.Attack(attacker);
                AnsiConsole.MarkupLine($"[red]The wild {wild.Name} attacked your {attacker.Name} for {damage} damage and left it with {attacker.HP} HP![/]");
            }
        }

        private bool HandleFaintedAttacker(List<Pokemon> pocket, ref int selection, Pokemon attacker)
        {
            if (attacker.HP <= 0)
            {
                AnsiConsole.MarkupLine($"[red]{attacker.Name} has fainted.[/]");
                pocket.RemoveAt(selection);
                if (pocket.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]You have no more Pokémon left. The wild Pokémon fled.[/]");
                    return true;
                }
                selection = SelectPokemon(pocket);
                return true;
            }
            return false;
        }

        private bool PromptForCapture(Pokemon wild, int maxHp, ref int attempts, out int goldGain, out bool userWantsToCapture)
        {
            goldGain = 0;
            userWantsToCapture = false; // Default to "No"

            if (wild.HP == 0)
            {
                AnsiConsole.MarkupLine($"[red]The wild {wild.Name} has already fainted and cannot be caught.[/]");
                return false;
            }

            var input = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Would you like to try to capture?[/]")
                    .AddChoices("Yes", "No")
            );

            // Update the user's choice
            userWantsToCapture = input == "Yes";

            // Exit early if the player chooses "No"
            if (!userWantsToCapture)
            {
                return false;
            }

            // Attempt to catch the Pokémon
            if (AttemptCatch(wild, maxHp))
            {
                double healthPercentage = (double)wild.HP / maxHp;
                int baseGold = 20;
                goldGain = (int)(baseGold * healthPercentage);

                AnsiConsole.MarkupLine($"[bold green]You successfully caught the {wild.Name} and gained {goldGain} gold![/]");
                wild.HP = maxHp;
                wild.MaxHP = maxHp;
                return true;
            }

            AnsiConsole.MarkupLine("[red]Capture failed.[/]");
            attempts--; // Decrease attempts if capture fails

            return false;
        }

        private bool AttemptCatch(Pokemon wild, int maxHp)
        {
            int percentageChance = (int)(((double)(maxHp - wild.HP) / maxHp) * 100);
            return this._random.Next(1, 101) <= percentageChance;
        }
    }
}
