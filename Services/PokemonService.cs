//Ivan Dochev 241836X
using static System.Math;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PokemonPocket.Services
{
    public class PokemonService
    {
        private readonly PokemonPocketContext _context;
        private readonly BattleService _battles;
        private readonly GymService _gyms;
        private readonly SpliceService _splice;
        private bool simpleState;

        public PokemonService(PokemonPocketContext context, BattleService battles, GymService gyms, SpliceService splice)
        {
            this._context = context;
            this._battles = battles;
            this._gyms = gyms;
            this._splice = splice;
            this.simpleState = true;
        }

        private void AddPokemon()
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
            ContinueToMenu();
        }

        private void ListPokemon()
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
            ContinueToMenu();
        }

        private void CheckEvolutionStatus()
        {
            var rules = this._context.EvolutionRules.ToList();
            bool eligibleEvolutions = false;

            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title("[bold underline]Evolution Status[/]");
            table.AddColumn("Pokemon");
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
                    table.AddRow($"{eligibleCount} {rule.Name}", $"{list.Count / rule.NoToEvolve} {rule.EvolveTo}");
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
            ContinueToMenu();
        }

        private void EvolveEligiblePokemon()
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

                }
            }

            this._context.SaveChanges();
            if (!simpleState) ContinueToMenu();
        }

        private void PlayerStats()
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

            ContinueToMenu();
        }

        private void HealPokemon()
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

            ContinueToMenu();
        }

        private string DrawMainMenu()
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
            "PokeSplice Hub",
            "Toggle Game State",
            "Exit"
                  }));
            return selection;
        }

        private string DrawPlayerMenu()
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[bold yellow]==== Player Menu ====[/]")
                .PageSize(10)
                .AddChoices(new[] {
            "Check your player Statistics",
            "Heal your Pokemon",
            "Fight and catch a Pokemon",
            "Go Back"
                  }));
            return selection;
        }

        private bool HandlePlayerMenu()
        {
            string input = this.DrawPlayerMenu();

            switch (input)
            {
                case "Check your player Statistics":
                    this.PlayerStats();
                    return true;
                case "Heal your Pokemon":
                    this.HealPokemon();
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
            switch (simpleState)
            {

                case false:
                    string input = this.DrawMainMenu();
                    switch (input)
                    {
                        case "Add Pokemon to my Pocket":
                            AddPokemon();
                            break;
                        case "List Pokemon(s) in my Pocket":
                            ListPokemon();
                            break;
                        case "Check if I can evolve my Pokemon":
                            CheckEvolutionStatus();
                            break;
                        case "Evolve Pokemon":
                            EvolveEligiblePokemon();
                            break;
                        case "Player Menu":
                            bool playerNotFinished = true;
                            while (playerNotFinished)
                            {
                                playerNotFinished = HandlePlayerMenu();
                            }
                            break;
                        case "Gym Menu":
                            bool gymNotFinished = false;
                            while (!gymNotFinished)
                            {
                                gymNotFinished = this._gyms.HandleGymMenu();
                            }
                            break;
                        case "PokeSplice Hub":
                            bool spliceNotFinished = false;
                            while (!spliceNotFinished)
                            {
                                spliceNotFinished = this._splice.HandleSpliceMenu();
                            }
                            break;
                        case "Toggle Game State":
                            ToggleGameState();
                            break;
                        case "Exit":
                            return false;
                    }
                    return true;

                case true:
                    char simpleInput = DrawSimpleMainMenu();
                    switch (simpleInput)
                    {
                        case '1':
                            SimpleAddPokemon();
                            break;
                        case '2':
                            SimpleListPokemon();
                            break;
                        case '3':
                            SimpleCheckEvolutionStatus();
                            break;
                        case '4':
                            SimpleEvolveEligiblePokemon();
                            break;
                        case '5':
                            ToggleGameState();
                            break;
                        case 'q':
                            return false;
                    }

                    return true;
            }
        }

        public async Task InitialiseEvoRulesAsync()
        {
            if (!await _context.EvolutionRules.AnyAsync())
            {
                var rules = new List<PokemonMaster>
        {
          new PokemonMaster { Name = "Pikachu", NoToEvolve = 2, EvolveTo = "Raichu" },
          new PokemonMaster { Name = "Charmander", NoToEvolve = 1, EvolveTo = "Charmeleon" },
          new PokemonMaster { Name = "Eevee", NoToEvolve = 3, EvolveTo = "Flareon" },
          new PokemonMaster { Name = "Bulbasaur", NoToEvolve = 5, EvolveTo = "Ivysaur" }
        };

                await _context.EvolutionRules.AddRangeAsync(rules);
            }

            await _context.SaveChangesAsync();
        }

        public static List<Pokemon> GetPlayerPokemon(PokemonPocketContext context)
        {
            return context.Pokemon
              .Where(p => EF.Property<int?>(p, "GymLeaderId") == null)
              .OrderByDescending(p => p.Exp)
              .ToList();
        }

        public void TestPokemon()
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

            var pikachu2 = new Pikachu()
            {
                Name = "Pikachu",
                HP = 1000,
                MaxHP = 1000,
                Exp = 0,
                Skill = "Lightning Bolt",
                SkillDamage = 100,
                Level = 40
            };

            var pikachu3 = new Pikachu()
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

            var eevee2 = new Eevee()
            {
                Name = "Eevee",
                HP = 1000,
                MaxHP = 1000,
                Exp = 0,
                Skill = "Run Away",
                SkillDamage = 100,
                Level = 40
            };

            var flareon = new Flareon()
            {
                Name = "Flareon",
                HP = 1000,
                MaxHP = 1000,
                Exp = 0,
                Skill = "Run Away",
                SkillDamage = 100,
                Level = 40
            };

            this._context.AddRange(pikachu, pikachu2, pikachu3, eevee, eevee2, bulbasaur, flareon);
            this._context.SaveChanges();
        }

        private void ContinueToMenu()
        {
            AnsiConsole.Prompt(
                new TextPrompt<string>("[grey]Press enter to continue...[/]")
                .AllowEmpty());
            AnsiConsole.Clear();
        }

        private char DrawSimpleMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(new string('*', 29));
                Console.WriteLine("Welcome to Pokemon Pocket App");
                Console.WriteLine(new string('*', 29));
                Console.WriteLine("(1) Add Pokemon to my pocket");
                Console.WriteLine("(2) List Pokemon(s) in my Pocket");
                Console.WriteLine("(3) Check if I can evolve pokemon");
                Console.WriteLine("(4) Evolve pokemon");
                Console.WriteLine("(5) Toggle Game State");
                Console.Write("Please enter [1-5] or press Q to quit: ");

                var raw = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    Console.WriteLine("Input cannot be empty. Press Enter to retry.");
                    Console.ReadLine();
                    continue;
                }

                var key = raw.Trim().ToLower()[0];
                if (key == 'q' || (key >= '1' && key <= '5'))
                    return key;

                Console.WriteLine("Invalid entry. Press Enter to retry.");
                Console.ReadLine();
            }
        }

        private void SimpleAddPokemon()
        {
            string pokemonName;
            while (true)
            {
                Console.Write("Enter Pokemon's Name (Pikachu, Charmander, Eevee): ");
                var raw = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(raw)) continue;

                pokemonName = raw.Trim().ToLower();
                if (new[] { "pikachu", "charmander", "eevee" }.Contains(pokemonName))
                    break;

                Console.WriteLine("Please only enter 'Pikachu', 'Charmander', or 'Eevee'.");
            }

            // HP entry
            int pokemonHP;
            while (true)
            {
                Console.Write("Enter Pokemon's HP: ");
                var raw = Console.ReadLine();
                if (!int.TryParse(raw, out pokemonHP) || pokemonHP < 0)
                {
                    Console.WriteLine("Invalid HP. Please try again.");
                    continue;
                }
                break;
            }

            // Exp entry
            int pokemonExp;
            while (true)
            {
                Console.Write("Enter Pokemon's Exp: ");
                var raw = Console.ReadLine();
                if (!int.TryParse(raw, out pokemonExp) || pokemonExp < 0)
                {
                    Console.WriteLine("Invalid Exp. Please try again.");
                    continue;
                }
                break;
            }

            // Instantiate & save
            try
            {
                switch (pokemonName)
                {
                    case "pikachu":
                        _context.Add(new Pikachu(pokemonHP, pokemonExp));
                        break;
                    case "charmander":
                        _context.Add(new Charmander(pokemonHP, pokemonExp));
                        break;
                    case "eevee":
                        _context.Add(new Eevee(pokemonHP, pokemonExp));
                        break;
                }
                _context.SaveChanges();
                Console.WriteLine("Pokemon successfully added!");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error saving to database: {ex.InnerException?.Message ?? ex.Message}");
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private void SimpleListPokemon()
        {
            var pocket = GetPlayerPokemon(_context)
              .OrderByDescending(p => p.Exp)
              .ToList();

            if (!pocket.Any())
            {
                Console.WriteLine("You currently have no Pokemon in your pocket.");
            }
            else
            {
                foreach (var p in pocket)
                {
                    Console.WriteLine($"Name : {p.Name}");
                    Console.WriteLine($"HP   : {p.HP}/{p.MaxHP}");
                    Console.WriteLine($"Exp  : {p.Exp}");
                    Console.WriteLine($"Skill: {p.Skill}");
                    Console.WriteLine(new string('-', 30));
                }
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private void SimpleCheckEvolutionStatus()
        {
            var rules = _context.EvolutionRules.ToList();
            bool eligible = false;

            foreach (var rule in rules)
            {
                var list = GetPlayerPokemon(_context)
                  .Where(p => p.Name == rule.Name)
                  .ToList();

                if (list.Count >= rule.NoToEvolve)
                {
                    eligible = true;
                    int groups = list.Count / rule.NoToEvolve;
                    int used = groups * rule.NoToEvolve;
                    Console.WriteLine($"{used} × {rule.Name} → {groups} × {rule.EvolveTo}");
                }
            }

            if (!eligible)
                Console.WriteLine("You currently have no eligible Pokemon for evolution.");

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private void SimpleEvolveEligiblePokemon()
        {
            var rules = _context.EvolutionRules.ToList();
            bool hasEvolved = false;

            foreach (var rule in rules)
            {
                var pocket = GetPlayerPokemon(_context)
                  .Where(p => p.Name == rule.Name)
                  .OrderByDescending(p => p.MaxHP)
                  .ThenByDescending(p => p.Exp)
                  .ToList();

                int fullGroups = pocket.Count / rule.NoToEvolve;
                if (fullGroups == 0) continue;

                hasEvolved = true;
                for (int g = 0; g < fullGroups; g++)
                {
                    var batch = pocket
                      .Skip(g * rule.NoToEvolve)
                      .Take(rule.NoToEvolve)
                      .ToList();

                    int maxHp = batch.Max(p => p.MaxHP);
                    int maxExp = batch.Max(p => p.Exp);
                    int maxLevel = batch.Max(p => p.Level);
                    int maxSkillDamage = batch.Max(p => p.SkillDamage);

                    _context.RemoveRange(batch);

                    Pokemon evo = rule.EvolveTo.ToLower() switch
                    {
                        "raichu" => new Raichu { Name = "Raichu", HP = 100, MaxHP = maxHp, Exp = maxExp, Level = maxLevel, SkillDamage = maxSkillDamage },
                        "charmeleon" => new Charmeleon { Name = "Charmeleon", HP = 100, MaxHP = maxHp, Exp = maxExp, Level = maxLevel, SkillDamage = maxSkillDamage },
                        "flareon" => new Flareon { Name = "Flareon", HP = 100, MaxHP = maxHp, Exp = maxExp, Level = maxLevel, SkillDamage = maxSkillDamage },
                        _ => throw new InvalidOperationException($"Unknown evolution target: {rule.EvolveTo}")
                    };

                    _context.Add(evo);
                }
            }

            try
            {
                _context.SaveChanges();
                if (hasEvolved)
                    Console.WriteLine("Your Pokemon have been evolved successfully!");
                else
                    Console.WriteLine("You currently have no eligible Pokemon for evolution.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error saving evolutions: {ex.InnerException?.Message ?? ex.Message}");
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private void ToggleGameState()
        {
            switch (this.simpleState)
            {
                case true:
                    Console.Clear();
                    this.simpleState = false;
                    break;

                case false:
                    AnsiConsole.Clear();
                    this.simpleState = true;
                    break;
            }
        }
    }
}
