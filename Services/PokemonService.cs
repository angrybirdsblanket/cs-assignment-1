using System;
using Spectre.Console;
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
            var name = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Choose Pokemon:[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Pikachu",
                        "Eevee",
                        "Charmander",
                        "Bulbasaur",
                        "Go Back"
                    }));

            if (name == "Go Back")
                return;

            int hp = AnsiConsole.Prompt(
                new TextPrompt<int>("[bold green]Enter Pokemon's HP:[/]")
                    .ValidationErrorMessage("[red]HP must be between 1 and 300[/]")
                    .Validate(value =>
                    {
                        if (value <= 0)
                            return ValidationResult.Error("[red]HP must be greater than 0[/]");
                        if (value > 300)
                            return ValidationResult.Error("[red]HP cannot exceed 300[/]");
                        return ValidationResult.Success();
                    })
            );

            int exp = AnsiConsole.Prompt(
                new TextPrompt<int>("[bold green]Enter Pokemon's Exp:[/]")
                    .ValidationErrorMessage("[red]Experience must be between 0 and 50[/]")
                    .Validate(value =>
                    {
                        if (value < 0)
                            return ValidationResult.Error("[red]Experience cannot be negative[/]");
                        if (value > 50)
                            return ValidationResult.Error("[red]Experience cannot exceed 50[/]");
                        return ValidationResult.Success();
                    })
            );

            switch (name.ToLower())
            {
                case "pikachu":
                    var pikachu = new Pikachu(hp, exp);
                    this._context.Add(pikachu);
                    break;
                case "charmander":
                    var charmander = new Charmander(hp, exp);
                    this._context.Add(charmander);
                    break;
                case "eevee":
                    var eevee = new Eevee(hp, exp);
                    this._context.Add(eevee);
                    break;
                case "bulbasaur":
                    var bulbasaur = new Bulbasaur(hp, exp);
                    this._context.Add(bulbasaur);
                    break;
            }

            this._context.SaveChanges();

            AnsiConsole.MarkupLine("[bold green]Pokemon successfully added![/]");
            continueToMenu();
        }

        private void listPokemon()
        {
            var pokemon = PokemonService.GetPlayerPokemon(this._context);

            if (pokemon.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]You currently have no Pokemon in your pocket.[/]");
            }
            else
            {
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Title("[bold underline]Your Pokemon Pocket[/]");
                table.AddColumn("Name");
                table.AddColumn("HP");
                table.AddColumn("Exp");
                table.AddColumn("Skill Name");
                table.AddColumn("Current Level");

                foreach (var p in pokemon)
                {
                    table.AddRow(p.Name, p.HP.ToString(), p.Exp.ToString(), p.Skill, p.Level.ToString());
                }
                AnsiConsole.Write(table);
            }
            continueToMenu();
        }

        private void checkEvolutionStatus()
        {
            var rules = this._context.EvolutionRules.ToList();
            bool eligibleEvolutions = false;

            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title("[bold underline]Evolution Status[/]");
            table.AddColumn("Pokemon");
            table.AddColumn("Eligible Count");
            table.AddColumn("Evolves To");

            foreach (var rule in rules)
            {
                var list = PokemonService.GetPlayerPokemon(this._context)
                    .Where(p => p.Name == rule.Name)
                    .ToList();

                if (list.Count >= rule.NoToEvolve)
                {
                    eligibleEvolutions = true;
                    int eligibleCount = list.Count / rule.NoToEvolve * rule.NoToEvolve;
                    table.AddRow(rule.Name, eligibleCount.ToString(), rule.EvolveTo);
                }
            }
            if (!eligibleEvolutions)
            {
                AnsiConsole.MarkupLine("[red]You currently have no eligible Pokemon for evolution.[/]");
            }
            else
            {
                AnsiConsole.Write(table);
            }
            continueToMenu();
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
                        "raichu" => new Raichu { HP = 100, Exp = 0, Name = "Raichu", Level = maxLevel, SkillDamage = maxSkillDamage, MaxHP = maxHp },
                        "charmeleon" => new Charmeleon { HP = 100, Exp = 0, Name = "Charmeleon", Level = maxLevel, SkillDamage = maxSkillDamage, MaxHP = maxHp },
                        "flareon" => new Flareon { HP = 100, Exp = 0, Name = "Flareon", Level = maxLevel, SkillDamage = maxSkillDamage, MaxHP = maxHp },
                        "ivysaur" => new Ivysaur { HP = 100, Exp = 0, Name = "Ivysaur", Level = maxLevel, SkillDamage = maxSkillDamage, MaxHP = maxHp },
                        _ => throw new InvalidOperationException($"Unknown evolution target: {rule.EvolveTo}")
                    };
                    this._context.Add(evolved);

                    AnsiConsole.MarkupLine($"[bold green]{rule.Name} evolved into {rule.EvolveTo}![/]");
                }
            }

            this._context.SaveChanges();
            continueToMenu();
        }

        private void playerStats()
        {
            Player player = this._context.Players
                .Where(p => p.Id == 1)
                .First();

            List<Pokemon> healablePokemon = PokemonService.GetPlayerPokemon(this._context)
                .Where(p => p.HP != p.MaxHP)
                .ToList();

            int badgeCount = this._context.Badges
                .Where(b => b.PlayerId == player.Id)
                .Count();

            var panel = new Panel($"[bold yellow]Gold:[/] {player.Gold}\n[bold yellow]Pokemon Needing Healing:[/] {healablePokemon.Count}\n[bold yellow]Badges:[/] {badgeCount}");
            panel.Header = new PanelHeader("Player Statistics", Justify.Center);
            AnsiConsole.Write(panel);

            continueToMenu();
        }

        private void healPokemon()
        {
            var pokemonList = PokemonService.GetPlayerPokemon(this._context)
                .OrderBy(p => p.Id)
                .Where(p => p.HP != p.MaxHP)
                .ToList();

            if (pokemonList.Count >= 1)
            {
                int totalHealthPercentMissing = 0;
                foreach (Pokemon pokemon in pokemonList)
                {
                    int healthPercentMissing = (int)(((double)(pokemon.MaxHP - pokemon.HP) / pokemon.MaxHP) * 100);
                    totalHealthPercentMissing += healthPercentMissing;
                }

                int goldRequired = Max(totalHealthPercentMissing / 10, 1);

                var response = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold green]To heal your Pokemon, you need to pay {goldRequired} gold. Would you like to proceed?[/]")
                        .PageSize(10)
                        .AddChoices(new[] { "y", "n" })
                );

                if (response == "y")
                {
                    Player player = this._context.Players.First();
                    if (player.Gold >= goldRequired)
                    {
                        player.Gold -= goldRequired;
                        foreach (Pokemon pokemon in pokemonList)
                        {
                            pokemon.Heal();
                        }
                        AnsiConsole.MarkupLine("[bold green]Your Pokemon have been healed![/]");
                        this._context.SaveChanges();
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]You do not have enough gold, please come back when you gather more.[/]");
                    }
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[green]All your Pokemon are currently at max health![/]");
            }

            continueToMenu();
        }

        private string drawMainMenu()
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Welcome to Pokemon Pocket App[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Add Pokemon to my Pocket",
                        "List Pokemon(s) in my Pocket",
                        "Check if I can evolve my Pokemon",
                        "Evolve Pokemon",
                        "Player Menu",
                        "Gym Menu",
                        "Exit"
                    }));
            return selection;
        }

        private string drawPlayerMenu()
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Player Menu[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Check your player Statistics",
                        "Heal your Pokemon",
                        "Fight and catch a Pokemon",
                        "Go Back"
                    }));
            return selection;
        }

        private bool handlePlayerMenu()
        {
            string input = this.drawPlayerMenu();

            switch (input)
            {
                case "Check your player Statistics":
                    this.playerStats();
                    return true;
                case "Heal your Pokemon":
                    this.healPokemon();
                    return true;
                case "Fight and catch a Pokemon":
                    var playerPokemon = PokemonService.GetPlayerPokemon(this._context);
                    if (playerPokemon.Count > 0)
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
                        AnsiConsole.MarkupLine("[red]Your Pocket is empty, please come back once you have Pokemon inside.[/]");
                    }
                    return true;
                case "Go Back":
                    return false;
            }
            return true; // unreachable but required by compiler
        }

        public bool GetNextAction()
        {
            string input = this.drawMainMenu();
            switch (input)
            {
                case "Add Pokemon to my Pocket":
                    addPokemon();
                    break;
                case "List Pokemon(s) in my Pocket":
                    listPokemon();
                    break;
                case "Check if I can evolve my Pokemon":
                    checkEvolutionStatus();
                    break;
                case "Evolve Pokemon":
                    evolveEligiblePokemon();
                    break;
                case "Player Menu":
                    bool playerNotFinished = true;
                    while (playerNotFinished)
                    {
                        playerNotFinished = handlePlayerMenu();
                    }
                    break;
                case "Gym Menu":
                    bool gymNotFinished = false;
                    while (!gymNotFinished)
                    {
                        gymNotFinished = this._gyms.HandleGymMenu();
                    }
                    break;
                case "Exit":
                    return false;
            }
            return true;
        }

        public void InitialiseEvoRules()
        {
            var pikachu_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Pikachu");
            if (!(pikachu_query is PokemonMaster))
            {
                var pikachu = new PokemonMaster()
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
                var eevee = new PokemonMaster()
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
                var charmander = new PokemonMaster()
                {
                    Name = "Charmander",
                    NoToEvolve = 1,
                    EvolveTo = "Charmeleon"
                };
                this._context.EvolutionRules.Add(charmander);
            }

            var bulbasaur_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Bulbasaur");
            if (!(bulbasaur_query is PokemonMaster))
            {
                var bulbasaur = new PokemonMaster()
                {
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

        public void testPokemon()
        {
            var pikachu = new Pikachu()
            {
                Name = "Pikachu",
                HP = 1000,
                MaxHP = 1000,
                Exp = 0,
                Skill = "Lightning Bolt",
                SkillDamage = 100,
                Level = 40
            };

            var bulbasaur = new Bulbasaur()
            {
                Name = "Bulbasaur",
                HP = 1000,
                MaxHP = 1000,
                Exp = 0,
                Skill = "Verdant Spiral",
                SkillDamage = 100,
                Level = 40
            };

            var eevee = new Eevee()
            {
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

        private void continueToMenu()
        {
            AnsiConsole.Prompt(
                new TextPrompt<string>("[grey]Press enter to continue...[/]")
                    .AllowEmpty());
            AnsiConsole.Clear();
        }
    }
}
