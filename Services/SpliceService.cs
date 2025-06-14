//Ivan Dochev 241836X
using System.Threading.Tasks;


namespace PokemonPocket.Services
{
    public class SpliceService
    {
        private readonly PokemonPocketContext _context;

        public SpliceService(PokemonPocketContext context)
        {
            this._context = context;
        }

        public async Task InitialiseSplicingRulesAsync()
        {
            var rules = this._context.SplicingRules.ToList();
            if (rules.Count() == 0)
            {
                var seeds = new[]
                {
                    new SplicingRule("Eevee", 2, "Pikachu", 3, "Eeveechu"),
                    new SplicingRule("Eevee", 1, "Flareon", 3, "Eeveeon"),
                    new SplicingRule("Charmander", 5, "Ivysaur", 2, "Charvysaur"),
                    new SplicingRule("Pikachu", 3, "Bulbasaur", 2, "Pikasaur"),
                    new SplicingRule("Charmander", 4, "Raichu", 3, "Charaichu"),
                    new SplicingRule("Flareon", 2, "Eeveechu", 1, "Flareeveechu"),
                    new SplicingRule("Bulbasaur", 3, "Charvysaur", 2, "Charvysaurion")
                };
                await this._context.AddRangeAsync(seeds);
            }

            await this._context.SaveChangesAsync();
        }

        public bool HandleSpliceMenu()
        {
            string input = this.DrawSpliceMenu();

            switch (input)
            {
                case "List Splicing Recipes":
                    this.DisplaySplicingRecipes();
                    return false;
                case "Display Current Eligible Recipes":
                    this.DisplayEligibleSpliceRecipes();
                    return false;
                case "Splice Eligible Pokemon":
                    this.SplicePokemon();
                    return false;
                case "Go Back":
                    return true;
            }

            return true;
        }

        private string DrawSpliceMenu()
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]==== The PokeSplicer ====[/]")
                    .PageSize(10)
                    .AddChoices(new[]
                    {
                        "List Splicing Recipes",
                        "Display Current Eligible Recipes",
                        "Splice Eligible Pokemon",
                        "Go Back"
                    })
            );

            return selection;
        }

        private void DisplaySplicingRecipes()
        {
            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title("[bold underline]All Splicing Recipes[/]");
            table.AddColumn("Sample A");
            table.AddColumn("A count");
            table.AddColumn("Sample B");
            table.AddColumn("B count");
            table.AddColumn("Outcome");

            List<SplicingRule> rules = this._context.SplicingRules.ToList();

            foreach (SplicingRule rule in rules)
            {
                table.AddRow(
                    rule.parentAName,
                    rule.parentACount.ToString(),
                    rule.parentBName,
                    rule.parentBCount.ToString(),
                    rule.childName
                );
            }

            AnsiConsole.Write(table);
            this.ContinueToMenu();
        }

        private void DisplayEligibleSpliceRecipes()
        {
            List<SplicingRule> rules = this._context.SplicingRules.ToList();

            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title("[bold underline]Eligible Splices[/]");
            table.AddColumn("Pokémon A");
            table.AddColumn("Pokémon B");
            table.AddColumn("Outcome");

            bool eligibleSplices = false;

            foreach (SplicingRule rule in rules)
            {
                List<Pokemon> currPokemon = PokemonService
                    .GetPlayerPokemon(this._context)
                    .Where(p => p.Name == rule.parentAName || p.Name == rule.parentBName)
                    .ToList();

                List<Pokemon> parentAPokemon = currPokemon
                    .Where(p => p.Name == rule.parentAName)
                    .ToList();
                List<Pokemon> parentBPokemon = currPokemon
                    .Where(p => p.Name == rule.parentBName)
                    .ToList();

                int childCount = System.Math.Min(
                    parentAPokemon.Count() / rule.parentACount,
                    parentBPokemon.Count() / rule.parentBCount
                );

                if (childCount >= 1)
                {
                    eligibleSplices = true;
                    table.AddRow(
                        $"{rule.parentACount * childCount} {rule.parentAName}",
                        $"{rule.parentBCount * childCount} {rule.parentBName}",
                        $"{childCount} {rule.childName}"
                    );
                }
            }

            if (eligibleSplices)
            {
                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]You have no Pokémon eligible for splicing.[/]");
            }

            this.ContinueToMenu();
        }

        private void SplicePokemon()
        {
            bool splicing = true;

            while (splicing)
            {
                var eligible = new List<(SplicingRule rule, int count)>();
                var rules = this._context.SplicingRules.ToList();
                var pocket = PokemonService.GetPlayerPokemon(this._context);

                foreach (var rule in rules)
                {
                    int haveA = pocket.Count(p => p.Name == rule.parentAName);
                    int haveB = pocket.Count(p => p.Name == rule.parentBName);
                    int possible = System.Math.Min(
                        haveA / rule.parentACount,
                        haveB / rule.parentBCount
                    );
                    if (possible > 0)
                        eligible.Add((rule, possible));
                }

                if (!eligible.Any())
                {
                    AnsiConsole.MarkupLine("[red]No splices available right now.[/]");
                    splicing = false;
                    break;
                }

                var choices = eligible
                    .Select((e, idx) =>
                        $"{idx + 1}. {e.count}× {e.rule.childName} " +
                        $"({e.rule.parentACount}×{e.rule.parentAName} + {e.rule.parentBCount}×{e.rule.parentBName})"
                    )
                    .Append("Go Back")
                    .ToList();

                var pick = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Select a splice to perform[/]")
                        .PageSize(10)
                        .AddChoices(choices)
                );

                if (pick == "Go Back")
                {
                    splicing = false;
                    break;
                }

                int index = int.Parse(pick.Split('.')[0]) - 1;
                var (ruleToUse, _) = eligible[index];

                var removeA = pocket
                    .Where(p => p.Name == ruleToUse.parentAName)
                    .Take(ruleToUse.parentACount)
                    .ToList();
                var removeB = pocket
                    .Where(p => p.Name == ruleToUse.parentBName)
                    .Take(ruleToUse.parentBCount)
                    .ToList();

                this._context.RemoveRange(removeA);
                this._context.RemoveRange(removeB);

                var batch = removeA.Concat(removeB).ToList();
                int maxHp = batch.Max(p => p.MaxHP);
                int maxExp = batch.Max(p => p.Exp);
                int maxLevel = batch.Max(p => p.Level);
                int maxSkillDamage = batch.Max(p => p.SkillDamage);

                Pokemon child = ruleToUse.childName switch
                {
                    "Eeveechu" => new Eeveechu
                    {
                        Name = ruleToUse.childName,
                        HP = maxHp,
                        MaxHP = maxHp,
                        Exp = maxExp,
                        Level = maxLevel,
                        SkillDamage = maxSkillDamage + 60
                    },
                    "Eeveeon" => new Eeveeon
                    {
                        Name = ruleToUse.childName,
                        HP = maxHp,
                        MaxHP = maxHp,
                        Exp = maxExp,
                        Level = maxLevel,
                        SkillDamage = maxSkillDamage + 65
                    },
                    "Charvysaur" => new Charvysaur
                    {
                        Name = ruleToUse.childName,
                        HP = maxHp,
                        MaxHP = maxHp,
                        Exp = maxExp,
                        Level = maxLevel,
                        SkillDamage = maxSkillDamage + 50
                    },
                    "Pikasaur" => new Pikasaur
                    {
                        Name = ruleToUse.childName,
                        HP = maxHp,
                        MaxHP = maxHp,
                        Exp = maxExp,
                        Level = maxLevel,
                        SkillDamage = maxSkillDamage + 45
                    },
                    "Charaichu" => new Charaichu
                    {
                        Name = ruleToUse.childName,
                        HP = maxHp,
                        MaxHP = maxHp,
                        Exp = maxExp,
                        Level = maxLevel,
                        SkillDamage = maxSkillDamage + 40
                    },
                    "Flareeveechu" => new Flareeveechu
                    {
                        Name = ruleToUse.childName,
                        HP = maxHp,
                        MaxHP = maxHp,
                        Exp = maxExp,
                        Level = maxLevel,
                        SkillDamage = maxSkillDamage + 75
                    },
                    "Charvysaurion" => new Charvysaurion
                    {
                        Name = ruleToUse.childName,
                        HP = maxHp,
                        MaxHP = maxHp,
                        Exp = maxExp,
                        Level = maxLevel,
                        SkillDamage = maxSkillDamage + 60
                    },
                    _ => throw new InvalidOperationException("Unknown splice")
                };

                this._context.Add(child);
                this._context.SaveChanges();
                AnsiConsole.MarkupLine($"[green]Successfully spliced 1× {ruleToUse.childName}![/]");
            }

            this.ContinueToMenu();
        }

        private void ContinueToMenu()
        {
            AnsiConsole.Prompt(
                new TextPrompt<string>("[grey]Press enter to continue...[/]")
                    .AllowEmpty()
            );
            AnsiConsole.Clear();
        }
    }
}

