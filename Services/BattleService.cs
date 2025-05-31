// Ivan Dochev 241836X
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

        private Pokemon GenerateRandomPokemon()
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
            Pokemon wild = this.GenerateRandomPokemon();
            int goldGain;
            bool success = this.CatchPokemon(wild, out goldGain);

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
                AnsiConsole.MarkupLine($"[bold green]Your {pokemon.Name} leveled up to {pokemon.Level}![/]");
            }

            this._context.SaveChanges();

            this.PauseAndClear();
            return success ? wild : null;
        }

        private bool CatchPokemon(Pokemon wild, out int goldGain)
        {
            goldGain = 0;
            int attempts = 3; 
            bool success = false;
            int maxHp = wild.HP;

            AnsiConsole.MarkupLine($"[bold yellow]\nA wild {wild.Name} with {maxHp} HP appeared![/]");

            List<Pokemon> pocket = this.GetAvailablePokemon();

            while (attempts > 0 && wild.HP > 0 && pocket.Count > 0)
            {
                int selection = this.SelectPokemon(pocket);
                Pokemon attacker = pocket[selection];

                this.ExecuteBattleRound(attacker, wild);

                this.DisplayBattleStatus(attacker, wild);

                if (this.HandleFaintedAttacker(pocket, ref selection, attacker))
                    continue;

                for (int i = 0; i < 3 && attempts > 0; i++)
                {
                    bool userWantsToCapture;
                    success = this.PromptForCapture(wild, maxHp, ref attempts, out goldGain, out userWantsToCapture);

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
            attacker.Exp += attacker.CalculateExp(wild, damage);
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
                selection = this.SelectPokemon(pocket);
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

            userWantsToCapture = (input == "Yes");

            if (!userWantsToCapture)
                return false;

            if (this.AttemptCatch(wild, maxHp))
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
            attempts--; 

            return false;
        }

        private bool AttemptCatch(Pokemon wild, int maxHp)
        {
            int percentageChance = (int)(((double)(maxHp - wild.HP) / maxHp) * 100);
            return this._random.Next(1, 101) <= percentageChance;
        }

        private void PauseAndClear()
        {
            AnsiConsole.MarkupLine("[grey]Press [underline]Enter[/] to continue...[/]");
            Console.ReadLine();
            AnsiConsole.Clear();
        }
    }
}

