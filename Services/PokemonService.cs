//Ivan Dochev 241836X

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
                    .Title("[bold yellow]Choose Pokémon:[/]")
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
                new TextPrompt<int>("[bold green]Enter Pokémon's HP:[/]")
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
                new TextPrompt<int>("[bold green]Enter Pokémon's Exp:[/]")
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

            AnsiConsole.MarkupLine("[bold green]Pokémon successfully added![/]");
            this.ContinueToMenu();
        }

        private void ListPokemon()
        {
            var pokemon = PokemonService.GetPlayerPokemon(this._context);

            if (pokemon.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]You currently have no Pokémon in your pocket.[/]");
            }
            else
            {
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Title("[bold underline]Your Pokémon Pocket[/]");
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

            this.ContinueToMenu();
        }

        private void CheckEvolutionStatus()
        {
            var rules = this._context.EvolutionRules.ToList();
            bool eligibleEvolutions = false;

            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title("[bold underline]Evolution Status[/]");
            table.AddColumn("Pokémon");
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
                AnsiConsole.MarkupLine("[red]You currently have no eligible Pokémon for evolution.[/]");
            }
            else
            {
                AnsiConsole.Write(table);
            }

            this.ContinueToMenu();
        }

        private void EvolveEligiblePokemon()
        {
            var rules = this._context.EvolutionRules.ToList();

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
                    var batch = pocket
                        .Skip(g * rule.NoToEvolve)
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
            if (!this.simpleState) this.ContinueToMenu();
        }

        private void PlayerStats()
        {
            Player player = this._context.Players
                .Where(p => p.Id == 1)
                .Include(p => p.Badges)
                .First();

            List<Pokemon> healablePokemon = PokemonService.GetPlayerPokemon(this._context)
                .Where(p => p.HP != p.MaxHP)
                .ToList();

            var panel = new Panel(
                $"[bold yellow]Gold:[/] {player.Gold}\n" +
                $"[bold yellow]Pokémon Needing Healing:[/] {healablePokemon.Count}\n" +
                $"[bold yellow]Badges:[/] {player.Badges.Count}"
            );
            panel.Header = new PanelHeader("Player Statistics", Justify.Center);
            AnsiConsole.Write(panel);

            this.ContinueToMenu();
        }

        private void HealPokemon()
        {
            var pokemonList = PokemonService.GetPlayerPokemon(this._context)
                .OrderBy(p => p.Id)
                .Where(p => p.HP != p.MaxHP)
                .ToList();

            if (pokemonList.Count >= 1)
            {
                int totalHealthMissing = 0;
                foreach (Pokemon pokemon in pokemonList)
                {
                    int healthMissing = pokemon.MaxHP - pokemon.HP;
                    totalHealthMissing += healthMissing;
                }

                int goldRequired = 0;

                if (totalHealthMissing >= 2)
                {
                    while (totalHealthMissing >= 2)
                    {
                        goldRequired++;
                        totalHealthMissing -= 2;
                    }
                }
                else
                {
                    goldRequired = 1;
                }

                var response = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold green]To heal your Pokémon, you need to pay {goldRequired} gold. Would you like to proceed?[/]")
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
                        AnsiConsole.MarkupLine("[bold green]Your Pokémon have been healed![/]");
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
                AnsiConsole.MarkupLine("[green]All your Pokémon are currently at max health![/]");
            }

            this.ContinueToMenu();
        }

        private void PlayerBadges()
        {
            Player player = this._context.Players
                .Where(p => p.Id == 1)
                .Include(p => p.Badges)
                .First();

            Table table = new Table();
            table.Title("[bold yellow underline]Your Badges[/]");
            table.AddColumn("Badge Number");
            table.AddColumn("Badge Name");

            int i = 0;
            foreach (Badge badge in player.Badges)
            {
                i++;
                table.AddRow(i.ToString(), badge.Name);
            }

            AnsiConsole.Write(table);
            this.ContinueToMenu();
        }

        private string DrawMainMenu()
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Welcome to Pokémon Pocket App[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Add Pokémon to my Pocket",
                        "List Pokémon(s) in my Pocket",
                        "Check if I can evolve my Pokémon",
                        "Evolve Pokémon",
                        "Player Menu",
                        "Gym Menu",
                        "PokeSplice Hub",
                        "Toggle Game State",
                        "Exit"
                    })
            );
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
                        "Heal your Pokémon",
                        "Fight and catch a Pokémon",
                        "List your Badges",
                        "Go Back"
                    })
            );
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
                case "Heal your Pokémon":
                    this.HealPokemon();
                    return true;
                case "List your Badges":
                    this.PlayerBadges();
                    return true;
                case "Fight and catch a Pokémon":
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
                        AnsiConsole.MarkupLine("[red]Your Pocket is empty, please come back once you have Pokémon inside.[/]");
                    }
                    return true;
                case "Go Back":
                    return false;
            }
            return true; // unreachable but required by compiler
        }

        public bool GetNextAction()
        {
            switch (this.simpleState)
            {
                case false:
                    string input = this.DrawMainMenu();
                    switch (input)
                    {
                        case "Add Pokémon to my Pocket":
                            this.AddPokemon();
                            break;
                        case "List Pokémon(s) in my Pocket":
                            this.ListPokemon();
                            break;
                        case "Check if I can evolve my Pokémon":
                            this.CheckEvolutionStatus();
                            break;
                        case "Evolve Pokémon":
                            this.EvolveEligiblePokemon();
                            break;
                        case "Player Menu":
                            bool playerNotFinished = true;
                            while (playerNotFinished)
                            {
                                playerNotFinished = this.HandlePlayerMenu();
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
                            this.ToggleGameState();
                            break;
                        case "Exit":
                            return false;
                    }
                    return true;

                case true:
                    char simpleInput = this.DrawSimpleMainMenu();
                    switch (simpleInput)
                    {
                        case '1':
                            this.SimpleAddPokemon();
                            break;
                        case '2':
                            this.SimpleListPokemon();
                            break;
                        case '3':
                            this.SimpleCheckEvolutionStatus();
                            break;
                        case '4':
                            this.SimpleEvolveEligiblePokemon();
                            break;
                        case '5':
                            this.ToggleGameState();
                            break;
                        case 'q':
                            return false;
                    }
                    return true;
            }
        }

        public async Task InitialiseEvoRulesAsync()
        {
            if (!await this._context.EvolutionRules.AnyAsync())
            {
                var rules = new List<PokemonMaster>
                {
                    new PokemonMaster { Name = "Pikachu", NoToEvolve = 2, EvolveTo = "Raichu" },
                    new PokemonMaster { Name = "Charmander", NoToEvolve = 1, EvolveTo = "Charmeleon" },
                    new PokemonMaster { Name = "Eevee", NoToEvolve = 3, EvolveTo = "Flareon" },
                    new PokemonMaster { Name = "Bulbasaur", NoToEvolve = 5, EvolveTo = "Ivysaur" }
                };

                await this._context.EvolutionRules.AddRangeAsync(rules);
            }

            await this._context.SaveChangesAsync();
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
            var pokemonList = new List<Pokemon>
            {
                new Charmander(300, 20),
                new Charmander(300, 20),
                new Charmander(300, 20),
                new Charmander(300, 20),
                new Charmander(300, 20),
                new Ivysaur(300, 20),
                new Ivysaur(300, 20),

                new Charmander(300, 20),
                new Charmander(300, 20),
                new Charmander(300, 20),
                new Charmander(300, 20),
                new Charmander(300, 20),
                new Ivysaur(300, 20),
                new Ivysaur(300, 20),

                new Bulbasaur(300, 20),
                new Bulbasaur(300, 20),
                new Bulbasaur(300, 20),
            };

            this._context.AddRange(pokemonList);
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
                Console.WriteLine("Welcome to Pokémon Pocket App");
                Console.WriteLine(new string('*', 29));
                Console.WriteLine("(1) Add Pokémon to my pocket");
                Console.WriteLine("(2) List Pokémon(s) in my Pocket");
                Console.WriteLine("(3) Check if I can evolve Pokémon");
                Console.WriteLine("(4) Evolve Pokémon");
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
                Console.Write("Enter Pokémon's Name (Pikachu, Charmander, Eevee): ");
                var raw = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(raw)) continue;

                pokemonName = raw.Trim().ToLower();
                if (new[] { "pikachu", "charmander", "eevee" }.Contains(pokemonName))
                    break;

                Console.WriteLine("Please only enter 'Pikachu', 'Charmander', or 'Eevee'.");
            }

            int pokemonHP;
            while (true)
            {
                Console.Write("Enter Pokémon's HP: ");
                var raw = Console.ReadLine();
                if (!int.TryParse(raw, out pokemonHP) || pokemonHP < 0)
                {
                    Console.WriteLine("Invalid HP. Please try again.");
                    continue;
                }
                break;
            }

            int pokemonExp;
            while (true)
            {
                Console.Write("Enter Pokémon's Exp: ");
                var raw = Console.ReadLine();
                if (!int.TryParse(raw, out pokemonExp) || pokemonExp < 0)
                {
                    Console.WriteLine("Invalid Exp. Please try again.");
                    continue;
                }
                break;
            }

            try
            {
                switch (pokemonName)
                {
                    case "pikachu":
                        this._context.Add(new Pikachu(pokemonHP, pokemonExp));
                        break;
                    case "charmander":
                        this._context.Add(new Charmander(pokemonHP, pokemonExp));
                        break;
                    case "eevee":
                        this._context.Add(new Eevee(pokemonHP, pokemonExp));
                        break;
                }
                this._context.SaveChanges();
                Console.WriteLine("Pokémon successfully added!");
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
            var pocket = GetPlayerPokemon(this._context)
                .OrderByDescending(p => p.Exp)
                .ToList();

            if (!pocket.Any())
            {
                Console.WriteLine("You currently have no Pokémon in your pocket.");
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
            var rules = this._context.EvolutionRules.ToList();
            bool eligible = false;

            foreach (var rule in rules)
            {
                var list = GetPlayerPokemon(this._context)
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
                Console.WriteLine("You currently have no eligible Pokémon for evolution.");

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private void SimpleEvolveEligiblePokemon()
        {
            var rules = this._context.EvolutionRules.ToList();
            bool hasEvolved = false;

            foreach (var rule in rules)
            {
                var pocket = GetPlayerPokemon(this._context)
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

                    this._context.RemoveRange(batch);

                    Pokemon evo = rule.EvolveTo.ToLower() switch
                    {
                        "raichu" => new Raichu { Name = "Raichu", HP = 100, MaxHP = maxHp, Exp = maxExp, Level = maxLevel, SkillDamage = maxSkillDamage },
                        "charmeleon" => new Charmeleon { Name = "Charmeleon", HP = 100, MaxHP = maxHp, Exp = maxExp, Level = maxLevel, SkillDamage = maxSkillDamage },
                        "flareon" => new Flareon { Name = "Flareon", HP = 100, MaxHP = maxHp, Exp = maxExp, Level = maxLevel, SkillDamage = maxSkillDamage },
                        _ => throw new InvalidOperationException($"Unknown evolution target: {rule.EvolveTo}")
                    };

                    this._context.Add(evo);
                }
            }

            try
            {
                this._context.SaveChanges();
                if (hasEvolved)
                    Console.WriteLine("Your Pokémon have been evolved successfully!");
                else
                    Console.WriteLine("You currently have no eligible Pokémon for evolution.");
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

