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


        private Pokemon generateRandomPokemon()
        {
            var pokemonTypes = new List<Func<Pokemon>>
            {
                () => new Pikachu(this._random.Next(180, 301), 0),
                () => new Eevee(this._random.Next(180, 301), 0),
                () => new Charmander(this._random.Next(180, 301), 0),
                () => new Bulbasaur(this._random.Next(180, 301), 0)
            };

            Pokemon wild = pokemonTypes[this._random.Next(pokemonTypes.Count)]();
            return wild;
        }

        public Pokemon Capture()
        {
            // Generate a wild Pokémon and announce its appearance.
            Pokemon wild = generateRandomPokemon();
            int goldGain;
            bool success = CatchPokemon(wild, out goldGain);

            if (success)
            {
                Player player = this._context.Players
                    .Where(p => p.Id == 1)
                    .First();
                player.Gold += goldGain;
            }

            // Check for level ups after the combat and capture sequence.
            var levelablePokemon = PokemonService.GetPlayerPokemon(this._context)
                .Where(p => p.Exp >= 100)
                .ToList();

            foreach (Pokemon pokemon in levelablePokemon)
            {
                pokemon.LevelUp();
                AnsiConsole.MarkupLine($"[bold green]Your {pokemon.Name} leveled up to {pokemon.Level}![/]");
            }

            this._context.SaveChanges();


            PauseAndClear();
            return success ? wild : null;
        }

        private void calculateExp(Pokemon enemy, Pokemon attacker, int damageDealt)
        {
            const int maxExp = 50;
            int xpGained;


            if (damageDealt >= enemy.MaxHP)
            {
                xpGained = maxExp;
            }
            else
            {
                double damageRatio = (double)damageDealt / enemy.MaxHP;
                xpGained = (int)Math.Floor(damageRatio * maxExp);
                if (damageDealt > 0 && xpGained < 1)
                {
                    xpGained = 1;
                }
            }

            attacker.Exp += xpGained;
            this._context.SaveChanges();
        }

        private bool CatchPokemon(Pokemon wild, out int goldGain)
        {
            goldGain = 0;
            int attempts = 3; // Total capture attempts available
            bool success = false;
            int maxHp = wild.HP;

            AnsiConsole.MarkupLine($"[bold yellow]\nA wild {wild.Name} with {maxHp} HP appeared![/]");

            // Get the player's available Pokémon.
            List<Pokemon> pocket = GetAvailablePokemon();

            while (attempts > 0 && wild.HP > 0 && pocket.Count > 0)
            {
                // Player selects a Pokémon to battle.
                int selection = SelectPokemon(pocket);
                Pokemon attacker = pocket[selection];

                // Execute one battle round.
                ExecuteBattleRound(attacker, wild);

                // Display current battle status in a table for clarity.
                DisplayBattleStatus(attacker, wild);

                // If user's attacking Pokémon fainted, handle it and continue.
                if (HandleFaintedAttacker(pocket, ref selection, attacker))
                    continue;

                // Allow up to 3 capture attempts after each attack.
                for (int i = 0; i < 3 && attempts > 0; i++)
                {
                    bool userWantsToCapture;
                    success = PromptForCapture(wild, maxHp, ref attempts, out goldGain, out userWantsToCapture);

                    if (success)
                        break;

                    if (!userWantsToCapture)
                        break;
                }

                if (success)
                    break;
            }

            if (attempts == 0 && !success)
                AnsiConsole.MarkupLine($"[red]You have run out of attempts. The wild {wild.Name} has fled![/]");
            else if (wild.HP == 0 && !success)
                AnsiConsole.MarkupLine($"[red]{wild.Name} has fainted and cannot be caught![/]");

            // Removed the PauseAndClear from here so that only one prompt is used in Capture().
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
            calculateExp(wild, attacker, damage);
            AnsiConsole.MarkupLine($"[cyan]Your {attacker.Name} attacked the wild {wild.Name} for {damage} damage, leaving it with {wild.HP} HP.[/]");

            if (wild.HP > 0)
            {
                damage = wild.Attack(attacker);
                AnsiConsole.MarkupLine($"[red]The wild {wild.Name} attacked your {attacker.Name} for {damage} damage, leaving it with {attacker.HP} HP.[/]");
            }
        }

        private void DisplayBattleStatus(Pokemon attacker, Pokemon wild)
        {
            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title("[bold underline]Battle Status[/]");
            table.AddColumn("Fighter");
            table.AddColumn("Current HP");
            table.AddColumn("Max HP");
            table.AddColumn("Exp");

            table.AddRow(attacker.Name, attacker.HP.ToString(), attacker.MaxHP.ToString(), attacker.Exp.ToString());
            table.AddRow(wild.Name, wild.HP.ToString(), wild.MaxHP.ToString(), "N/A");

            AnsiConsole.Write(table);
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
            userWantsToCapture = false;

            if (wild.HP == 0)
            {
                AnsiConsole.MarkupLine($"[red]The wild {wild.Name} has already fainted and cannot be caught.[/]");
                return false;
            }

            var input = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Would you like to try to capture it?[/]")
                    .AddChoices("Yes", "No")
            );

            userWantsToCapture = input == "Yes";

            if (!userWantsToCapture)
                return false;

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
            return _random.Next(1, 101) <= percentageChance;
        }

        // Helper method to pause the console until the user presses Enter, then clear the screen.
        private void PauseAndClear()
        {
            AnsiConsole.MarkupLine("[grey]Press [underline]Enter[/] to continue...[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }
    }
}
