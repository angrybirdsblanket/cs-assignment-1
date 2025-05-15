using System;
using System.Collections.Generic;
using System.Linq;
using PokemonPocket.Data;
using PokemonPocket.Models;
using Spectre.Console;
using Microsoft.EntityFrameworkCore;

namespace PokemonPocket.Services
{
    public class GymService
    {
        private readonly PokemonPocketContext _context;
        private readonly Random _random = new Random();

        public GymService(PokemonPocketContext context)
        {
            this._context = context;
        }

        private string displayGymMenu()
        {
            string selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]==== Pokemon Gym ====[/]")
                    .AddChoices(new[]
                    {
                        "Check Upcoming Gym battles",
                        "Check next Gym Leader's Team",
                        "Start Gym fight",
                        "Go Back"
                    })
            );
            return selection;
        }

        public bool HandleGymMenu()
        {
            string input = displayGymMenu();
            
            switch (input)
            {
                case "Check Upcoming Gym battles":
                    listGyms();
                    return false;
                case "Check next Gym Leader's Team":
                    getNextFightPokemon();
                    return false;
                case "Start Gym fight":
                    startGymFight();
                    return false;
                case "Go Back":
                    return true;
                default:
                    return true; // unreachable but required by compiler
            }
        }

        public void InitialiseGyms()
        {
            var firstLaunchTest = this._context.GymLeaders.FirstOrDefault();
            if (firstLaunchTest == null)
            {
                GymLeader leader_1 = new GymLeader
                {
                    GymName = "Thunderdome",
                    BadgeName = "Electric Badge",
                    PokemonTeam = new List<Pokemon>
                    {
                        new Pikachu { Name = "Pikachu", HP = 200, MaxHP = 200, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 30, Level = 2 },
                        new Raichu { Name = "Raichu", HP = 400, MaxHP = 400, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 40, Level = 3 },
                        new Eevee { Name = "Eevee", HP = 300, MaxHP = 300, Exp = 0, Skill = "Run Away", SkillDamage = 20, Level = 2 }
                    },
                    Defeated = false,
                    TrainerName = "Arden"
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
                    },
                    Defeated = false,
                    TrainerName = "Kael"
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
                    },
                    Defeated = false,
                    TrainerName = "Liora"
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
                    },
                    Defeated = false,
                    TrainerName = "Darius"
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
                    },
                    Defeated = false,
                    TrainerName = "Sylas"
                };

                this._context.GymLeaders.AddRange(new[] { leader_1, leader_2, leader_3, leader_4, leader_5 });
                this._context.SaveChanges();
            }
        }

        private void listGyms()
        {
            var gyms = this._context.GymLeaders.ToList();

            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title("[bold underline]Upcoming Gyms[/]");
            table.AddColumn("No.");
            table.AddColumn("Gym Name");
            table.AddColumn("Trainer Name");
            table.AddColumn("Status");

            int gym = 1;
            foreach (GymLeader leader in gyms)
            {
                string status = leader.Defeated ? "[red]DEFEATED[/]" : "[green]CHALLENGE AVAILABLE[/]";
                table.AddRow(gym.ToString(), leader.GymName, leader.TrainerName, status);
                gym++;
            }

            AnsiConsole.Write(table);
            continueToMenu();
        }

        private void getNextFightPokemon()
        {
            var nextLeader = this._context.GymLeaders
              .Where(l => l.Defeated == false)
              .Include(l => l.PokemonTeam)
              .FirstOrDefault();

            if (nextLeader != null)
            {
                AnsiConsole.MarkupLine($"\n[bold yellow]The {nextLeader.GymName} is next[/]");
                AnsiConsole.MarkupLine($"Gym Leader: [underline]{nextLeader.TrainerName}[/]");
                AnsiConsole.MarkupLine($"Badge: [italic]{nextLeader.BadgeName}[/]");
                AnsiConsole.MarkupLine("\n[bold]You will be fighting the following Pokémon:[/]");
                AnsiConsole.MarkupLine("[grey]----------------------------------------[/]");

                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.AddColumn("Name");
                table.AddColumn("Max HP");
                table.AddColumn("Level");
                table.AddColumn("Skill");

                foreach (Pokemon pokemon in nextLeader.PokemonTeam)
                {
                    table.AddRow(
                        pokemon.Name,
                        pokemon.MaxHP.ToString(),
                        pokemon.Level.ToString(),
                        $"{pokemon.Skill} ({pokemon.SkillDamage} dmg)"
                    );
                }

                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.MarkupLine("\n[green]Congratulations! You have defeated all gym leaders![/]");
            }
            continueToMenu();
        }

        private void startGymFight()
        {
            var leader = this._context.GymLeaders
              .Include(l => l.PokemonTeam)
              .Where(l => l.Defeated == false)
              .FirstOrDefault();

            if (leader == null)
            {
                AnsiConsole.MarkupLine("\n[green]You have defeated all leaders! Congratulations on becoming a Pokémon Master![/]");
                continueToMenu();
                return;
            }

            List<Pokemon> leaderTeam = leader.PokemonTeam.Where(p => p.HP > 0).ToList();
            List<Pokemon> pocket = PokemonService.GetPlayerPokemon(this._context)
              .Where(p => p.HP > 0)
              .ToList();

            if (pocket.Count == 0)
            {
                AnsiConsole.MarkupLine("\n[red]You do not have any Pokémon available for battle.[/]");
                AnsiConsole.MarkupLine("[yellow]Please heal your Pokémon at the Pokémon Center before challenging a gym.[/]");
                continueToMenu();
                return;
            }

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold underline]=== GYM BATTLE: {leader.GymName.ToUpper()} ===[/]");
            AnsiConsole.MarkupLine($"\nGym Leader [underline]{leader.TrainerName}[/] challenges you to a battle!");
            AnsiConsole.MarkupLine($"The [italic]{leader.BadgeName}[/] is at stake!");

            bool playerWon = true;

            int pokemonSelection = selectPokemon(pocket);
            Pokemon playerPokemon = pocket[pokemonSelection];

            int currentLeaderPokemonIndex = 0;
            Pokemon leaderCurrentPokemon = leaderTeam[currentLeaderPokemonIndex];
            AnsiConsole.MarkupLine($"\nGym Leader [bold]{leader.TrainerName}[/] sends out [green]{leaderCurrentPokemon.Name}[/]!");
            AnsiConsole.MarkupLine($"You send out [green]{playerPokemon.Name}[/]!");

            while (pocket.Count > 0 && leaderTeam.Count > 0)
            {
                string actionChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\nWhat would you like to do?")
                        .AddChoices(new[] { "Attack", "Switch Pokémon" })
                );

                if (actionChoice == "Switch Pokémon")
                {
                    pokemonSelection = selectPokemon(pocket);
                    playerPokemon = pocket[pokemonSelection];
                    AnsiConsole.MarkupLine($"You switch to [green]{playerPokemon.Name}[/]!");

                    int damage = leaderCurrentPokemon.Attack(playerPokemon);
                    AnsiConsole.MarkupLine($"The trainer's [green]{leaderCurrentPokemon.Name}[/] attacked your [green]{playerPokemon.Name}[/] for [bold]{damage}[/] damage and left it with [italic]{playerPokemon.HP}[/] HP!");

                    if (playerPokemon.HP <= 0)
                    {
                        AnsiConsole.MarkupLine($"[red]{playerPokemon.Name} has fainted.[/]");
                        pocket.RemoveAt(pokemonSelection);

                        if (pocket.Count == 0)
                        {
                            playerWon = false;
                            break;
                        }

                        pokemonSelection = selectPokemon(pocket);
                        playerPokemon = pocket[pokemonSelection];
                        AnsiConsole.MarkupLine($"You send out [green]{playerPokemon.Name}[/]!");
                    }
                    continue;
                }

                executeBattleRound(playerPokemon, leaderCurrentPokemon);

                if (leaderCurrentPokemon.HP <= 0)
                {
                    AnsiConsole.MarkupLine($"[red]Gym Leader's {leaderCurrentPokemon.Name} has fainted![/]");

                    leaderTeam.RemoveAt(currentLeaderPokemonIndex);

                    if (leaderTeam.Count == 0)
                    {
                        AnsiConsole.MarkupLine($"[bold green]You've defeated all of Gym Leader {leader.TrainerName}'s Pokémon![/]");
                        break;
                    }

                    currentLeaderPokemonIndex = 0;
                    leaderCurrentPokemon = leaderTeam[currentLeaderPokemonIndex];
                    AnsiConsole.MarkupLine($"Gym Leader [bold]{leader.TrainerName}[/] sends out [green]{leaderCurrentPokemon.Name}[/]!");
                }

                if (playerPokemon.HP <= 0)
                {
                    AnsiConsole.MarkupLine($"[red]{playerPokemon.Name} has fainted.[/]");
                    pocket.RemoveAt(pokemonSelection);

                    if (pocket.Count == 0)
                    {
                        playerWon = false;
                        break;
                    }

                    pokemonSelection = selectPokemon(pocket);
                    playerPokemon = pocket[pokemonSelection];
                    AnsiConsole.MarkupLine($"You send out [green]{playerPokemon.Name}[/]!");
                }
            }

            if (playerWon)
            {
                AnsiConsole.MarkupLine($"\n[bold green]Congratulations! You've defeated Gym Leader {leader.TrainerName}![/]");
                AnsiConsole.MarkupLine($"You've earned the [italic]{leader.BadgeName}[/]!");

                //reset the defeated leader's Pokémon
                var dbLeader = this._context.GymLeaders.Find(leader.Id);
                if (dbLeader != null)
                {
                    dbLeader.Defeated = true;
                    foreach (var pokemon in dbLeader.PokemonTeam)
                    {
                        pokemon.HP = pokemon.MaxHP;
                    }
                }

                var badge = new Badge
                {
                    Name = leader.BadgeName,
                    PlayerId = 1,
                    Type = leader.GymName
                };

                Player player = this._context.Players.Where(p => p.Id == 1).First();
                int goldReward = 100 * (leader.PokemonTeam.Count);
                player.Gold += goldReward;
                AnsiConsole.MarkupLine($"You received [bold yellow]{goldReward}[/] gold as a reward!");

                var levelablePokemon = PokemonService.GetPlayerPokemon(this._context)
                  .Where(p => p.Exp >= 100)
                  .ToList();

                foreach (Pokemon pokemon in levelablePokemon)
                {
                    pokemon.LevelUp();
                    AnsiConsole.MarkupLine($"Your [green]{pokemon.Name}[/] leveled up to level [bold]{pokemon.Level}[/]!");
                }

                this._context.Badges.Add(badge);
                this._context.SaveChanges();
            }
            else
            {
                AnsiConsole.MarkupLine($"\n[red]You were defeated by Gym Leader {leader.TrainerName}.[/]");
                AnsiConsole.MarkupLine("[yellow]Train harder and try again![/]");
            }
            
            this._context.SaveChanges();
            continueToMenu();
        }

        private void calculateExp(Pokemon enemy, Pokemon attacker, int damageDealt)
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

        private List<Pokemon> getAvailablePokemon()
        {
            return PokemonService.GetPlayerPokemon(this._context)
              .OrderBy(p => p.Id)
              .Where(p => p.HP > 0)
              .ToList();
        }

        private int selectPokemon(List<Pokemon> pocket)
        {
            var choices = pocket.Select((p, i) => $"{i}: {p.Name}, HP: {p.HP}/{p.MaxHP}, Level: {p.Level}, Exp: {p.Exp}/100").ToList();
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose a Pokémon from your pocket:")
                    .AddChoices(choices)
            );
            int index = int.Parse(choice.Split(':')[0]);
            return index;
        }

        private void executeBattleRound(Pokemon attacker, Pokemon leaderPokemon)
        {
            int damage = attacker.Attack(leaderPokemon);
            AnsiConsole.MarkupLine($"Your [green]{attacker.Name}[/] attacked the trainer's [green]{leaderPokemon.Name}[/] for [bold]{damage}[/] damage and left it with [italic]{leaderPokemon.HP}[/] HP!");
            calculateExp(leaderPokemon, attacker, damage);

            if (leaderPokemon.HP > 0)
            {
                damage = leaderPokemon.Attack(attacker);
                AnsiConsole.MarkupLine($"The trainer's [green]{leaderPokemon.Name}[/] attacked your [green]{attacker.Name}[/] for [bold]{damage}[/] damage and left it with [italic]{attacker.HP}[/] HP!");
            }
        }
        
        // Helper method to pause and clear the console.
        private void continueToMenu()
        {
            // Using a simple prompt so user can press Enter to continue.
            AnsiConsole.MarkupLine("[grey]Press [underline]Enter[/] to continue...[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }
    }
}
