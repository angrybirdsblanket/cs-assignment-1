// Ivan Dochev 241836X
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

        private string DisplayGymMenu()
        {
            string selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]==== Pokémon Gym ====[/]")
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
            string input = this.DisplayGymMenu();

            switch (input)
            {
                case "Check Upcoming Gym battles":
                    this.ListGyms();
                    return false;
                case "Check next Gym Leader's Team":
                    this.GetNextFightPokemon();
                    return false;
                case "Start Gym fight":
                    this.StartGymFight();
                    return false;
                case "Go Back":
                    return true;
                default:
                    return true; // unreachable but required by compiler
            }
        }

        public async Task InitialiseGymsAsync()
        {
            var firstLeader = await this._context.GymLeaders.FirstOrDefaultAsync();

            if (firstLeader == null)
            {
                GymLeader[] leaders = new GymLeader[]
                {
                    new GymLeader
                    {
                        GymName   = "Thunderdome",
                        BadgeName = "Electric Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Charvysaur { Name = "Pikachu", HP = 200, MaxHP = 200, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 30, Level = 2 },
                            new Raichu    { Name = "Raichu",   HP = 400, MaxHP = 400, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 40, Level = 3 },
                            new Eevee     { Name = "Eevee",    HP = 300, MaxHP = 300, Exp = 0, Skill = "Run Away",       SkillDamage = 20, Level = 2 }
                        },
                        Defeated    = false,
                        TrainerName = "Arden"
                    },
                    new GymLeader
                    {
                        GymName   = "Verdant Grove",
                        BadgeName = "Grass Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Bulbasaur { Name = "Bulbasaur", HP = 325, MaxHP = 325, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 25, Level = 3 },
                            new Ivysaur   { Name = "Ivysaur",   HP = 450, MaxHP = 450, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 30, Level = 4 },
                            new Eevee     { Name = "Eevee",     HP = 300, MaxHP = 300, Exp = 0, Skill = "Run Away",       SkillDamage = 15, Level = 2 }
                        },
                        Defeated    = false,
                        TrainerName = "Kael"
                    },
                    new GymLeader
                    {
                        GymName   = "Blazing Inferno",
                        BadgeName = "Fire Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Charmander { Name = "Charmander", HP = 350, MaxHP = 350, Exp = 0, Skill = "Solar Power", SkillDamage = 20, Level = 3 },
                            new Flareon    { Name = "Flareon",    HP = 450, MaxHP = 450, Exp = 0, Skill = "Run Away",   SkillDamage = 40, Level = 4 },
                            new Charmeleon { Name = "Charmeleon", HP = 500, MaxHP = 500, Exp = 0, Skill = "Solar Power", SkillDamage = 35, Level = 5 }
                        },
                        Defeated    = false,
                        TrainerName = "Liora"
                    },
                    new GymLeader
                    {
                        GymName   = "Mystic Lagoon",
                        BadgeName = "Water Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Ivysaur { Name = "Ivysaur", HP = 500, MaxHP = 500, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 35, Level = 5 },
                            new Eevee   { Name = "Eevee",   HP = 525, MaxHP = 525, Exp = 0, Skill = "Run Away",       SkillDamage = 25, Level = 4 },
                            new Flareon { Name = "Flareon", HP = 450, MaxHP = 450, Exp = 0, Skill = "Run Away",       SkillDamage = 30, Level = 5 }
                        },
                        Defeated    = false,
                        TrainerName = "Darius"
                    },
                    new GymLeader
                    {
                        GymName   = "Dragon's Den",
                        BadgeName = "Dragon Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Charmeleon{ Name = "Charmeleon", HP = 550, MaxHP = 550, Exp = 0, Skill = "Solar Power", SkillDamage = 40, Level = 6 },
                            new Raichu    { Name = "Raichu",     HP = 600, MaxHP = 600, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 50, Level = 7 },
                            new Ivysaur   { Name = "Ivysaur",    HP = 525, MaxHP = 525, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 45, Level = 6 }
                        },
                        Defeated    = false,
                        TrainerName = "Sylas"
                    },
                    new GymLeader
                    {
                        GymName   = "Sky Fortress",
                        BadgeName = "Flying Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Eeveechu   { Name = "Eeveechu", HP = 1300, MaxHP = 1300, Exp = 0, Skill = "Volt Dash",     SkillDamage = 90,  Level = 6 },
                            new Eeveeon    { Name = "Eeveeon",  HP = 1400, MaxHP = 1400, Exp = 0, Skill = "Flame Storm",   SkillDamage = 130, Level = 6 },
                            new Charvysaur { Name = "Charvysaur", HP = 1500, MaxHP = 1500, Exp = 0, Skill = "Flame Vine",   SkillDamage = 100, Level = 7 }
                        },
                        Defeated    = false,
                        TrainerName = "Zephyr"
                    },
                    new GymLeader
                    {
                        GymName   = "Shadow Realm",
                        BadgeName = "Shadow Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Pikasaur  { Name = "Pikasaur", HP = 2000, MaxHP = 2000, Exp = 0, Skill = "Thunder Seed", SkillDamage = 150, Level = 8 },
                            new Charmachu { Name = "Charmachu", HP = 2200, MaxHP = 2200, Exp = 0, Skill = "Electric Claw", SkillDamage = 180, Level = 9 },
                            new Eeveeon   { Name = "Eeveeon",   HP = 2400, MaxHP = 2400, Exp = 0, Skill = "Flare Storm",   SkillDamage = 200, Level = 10 }
                        },
                        Defeated    = false,
                        TrainerName = "Nyx"
                    },
                    new GymLeader
                    {
                        GymName   = "Crystal Cavern",
                        BadgeName = "Crystal Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Pikasaur   { Name = "Pikasaur",   HP = 1800, MaxHP = 1800, Exp = 0, Skill = "Thunder Seed", SkillDamage = 150, Level = 7 },
                            new Charmachu  { Name = "Charmachu",  HP = 2000, MaxHP = 2000, Exp = 0, Skill = "Electric Claw", SkillDamage = 170, Level = 8 },
                            new Charvysaur { Name = "Charvysaur", HP = 2200, MaxHP = 2200, Exp = 0, Skill = "Flame Vine",   SkillDamage = 180, Level = 9 }
                        },
                        Defeated    = false,
                        TrainerName = "Luna"
                    },
                    new GymLeader
                    {
                        GymName   = "Cosmic Nexus",
                        BadgeName = "Cosmic Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Eeveeon       { Name = "Eeveeon",     HP = 2500, MaxHP = 2500, Exp = 0, Skill = "Flare Storm",     SkillDamage = 250, Level = 12 },
                            new Charvysaurion { Name = "Charvysaur",  HP = 2750, MaxHP = 2750, Exp = 0, Skill = "Tri-Force Slash", SkillDamage = 200, Level = 10 },
                            new Flareeveechu  { Name = "Eeveechu",    HP = 3000, MaxHP = 3000, Exp = 0, Skill = "Elemental Charge", SkillDamage = 220, Level = 11 }
                        },
                        Defeated    = false,
                        TrainerName = "Astrid"
                    },
                    new GymLeader
                    {
                        GymName   = "Elemental Citadel",
                        BadgeName = "Elemental Badge",
                        PokemonTeam = new List<Pokemon>
                        {
                            new Charvysaurion { Name = "Charvysaurion", HP = 3500, MaxHP = 3500, Exp = 0, Skill = "Tri-Force Slash", SkillDamage = 300, Level = 15 },
                            new Pikasaur       { Name = "Pikasaur",      HP = 3200, MaxHP = 3200, Exp = 0, Skill = "Thunder Seed",    SkillDamage = 280, Level = 14 },
                            new Charmachu      { Name = "Charmachu",     HP = 3300, MaxHP = 3300, Exp = 0, Skill = "Electric Claw",   SkillDamage = 290, Level = 13 }
                        },
                        Defeated    = false,
                        TrainerName = "Elysia"
                    }
                };

                await this._context.GymLeaders.AddRangeAsync(leaders);
                await this._context.SaveChangesAsync();
            }
        }

        private void ListGyms()
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
            this.ContinueToMenu();
        }

        private void GetNextFightPokemon()
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

            this.ContinueToMenu();
        }

        private void StartGymFight()
        {
            var leader = this._context.GymLeaders
                .Include(l => l.PokemonTeam)
                .Where(l => l.Defeated == false)
                .FirstOrDefault();

            if (leader == null)
            {
                AnsiConsole.MarkupLine("\n[green]You have defeated all leaders! Congratulations on becoming a Pokémon Master![/]");
                this.ContinueToMenu();
                return;
            }

            List<Pokemon> leaderTeam = leader.PokemonTeam.Where(p => p.HP > 0).ToList();
            List<Pokemon> pocket = PokemonService
                .GetPlayerPokemon(this._context)
                .Where(p => p.HP > 0)
                .ToList();

            if (pocket.Count == 0)
            {
                AnsiConsole.MarkupLine("\n[red]You do not have any Pokémon available for battle.[/]");
                AnsiConsole.MarkupLine("[yellow]Please heal your Pokémon at the Pokémon Center before challenging a gym.[/]");
                this.ContinueToMenu();
                return;
            }

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold underline]=== GYM BATTLE: {leader.GymName.ToUpper()} ===[/]");
            AnsiConsole.MarkupLine($"\nGym Leader [underline]{leader.TrainerName}[/] challenges you to a battle!");
            AnsiConsole.MarkupLine($"The [italic]{leader.BadgeName}[/] is at stake!");

            bool playerWon = true;

            int pokemonSelection = this.SelectPokemon(pocket);
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
                    pokemonSelection = this.SelectPokemon(pocket);
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

                        pokemonSelection = this.SelectPokemon(pocket);
                        playerPokemon = pocket[pokemonSelection];
                        AnsiConsole.MarkupLine($"You send out [green]{playerPokemon.Name}[/]!");
                    }
                    continue;
                }

                this.ExecuteBattleRound(playerPokemon, leaderCurrentPokemon);

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

                    pokemonSelection = this.SelectPokemon(pocket);
                    playerPokemon = pocket[pokemonSelection];
                    AnsiConsole.MarkupLine($"You send out [green]{playerPokemon.Name}[/]!");
                }
            }

            if (playerWon)
            {
                AnsiConsole.MarkupLine($"\n[bold green]Congratulations! You've defeated Gym Leader {leader.TrainerName}![/]");
                AnsiConsole.MarkupLine($"You've earned the [italic]{leader.BadgeName}[/]!");

                Player player = this._context.Players
                    .Where(p => p.Id == 1)
                    .Include(p => p.Badges)
                    .First();

                // reset the defeated leader's Pokémon
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

                int goldReward = 100 * (leader.PokemonTeam.Count);
                player.Gold += goldReward;
                player.Badges.Add(badge);
                AnsiConsole.MarkupLine($"You received [bold yellow]{goldReward}[/] gold as a reward!");

                var levelablePokemon = PokemonService.GetPlayerPokemon(this._context)
                    .Where(p => p.Exp >= 100)
                    .ToList();

                foreach (Pokemon pokemon in levelablePokemon)
                {
                    pokemon.LevelUp();
                    AnsiConsole.MarkupLine($"Your [green]{pokemon.Name}[/] leveled up to level [bold]{pokemon.Level}[/]!");
                }

                this._context.SaveChanges();
            }
            else
            {
                AnsiConsole.MarkupLine($"\n[red]You were defeated by Gym Leader {leader.TrainerName}.[/]");
                AnsiConsole.MarkupLine("[yellow]Train harder and try again![/]");
            }

            this._context.SaveChanges();
            this.ContinueToMenu();
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
            var choices = pocket
                .Select((p, i) => $"{i}: {p.Name}, HP: {p.HP}/{p.MaxHP}, Level: {p.Level}, Exp: {p.Exp}/100")
                .ToList();

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose a Pokémon from your pocket:")
                    .AddChoices(choices)
            );
            int index = int.Parse(choice.Split(':')[0]);
            return index;
        }

        private void ExecuteBattleRound(Pokemon attacker, Pokemon leaderPokemon)
        {
            int damage = attacker.Attack(leaderPokemon);
            AnsiConsole.MarkupLine($"Your [green]{attacker.Name}[/] attacked the trainer's [green]{leaderPokemon.Name}[/] for [bold]{damage}[/] damage and left it with [italic]{leaderPokemon.HP}[/] HP!");
            attacker.Exp += attacker.CalculateExp(leaderPokemon, damage);

            if (leaderPokemon.HP > 0)
            {
                damage = leaderPokemon.Attack(attacker);
                AnsiConsole.MarkupLine($"The trainer's [green]{leaderPokemon.Name}[/] attacked your [green]{attacker.Name}[/] for [bold]{damage}[/] damage and left it with [italic]{attacker.HP}[/] HP!");
            }
        }

        private void ContinueToMenu()
        {
            AnsiConsole.MarkupLine("[grey]Press [underline]Enter[/] to continue...[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }
    }
}

