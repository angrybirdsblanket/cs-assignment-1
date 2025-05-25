using System.Threading.Tasks;


namespace PokemonPocket.Services {
  public class SpliceService {
    private readonly PokemonPocketContext _context;

    public SpliceService(PokemonPocketContext context) {
      this._context = context;
    }

    public async Task InitialiseSplicingRulesAsync()
    {
      var rules = this._context.SplicingRules.ToList();
      if (rules.Count() == 0)
      {
        var seeds = new[]
        {
          new SplicingRule {
            parentAName  = "Eevee",
            parentACount = 2,
            parentBName  = "Pikachu",
            parentBCount = 3,
            childName    = "Eeveechu"
          },
          new SplicingRule {
            parentAName  = "Eevee",
            parentACount = 1,
            parentBName  = "Flareon",
            parentBCount = 3,
            childName    = "Eeveeon"
          },
          new SplicingRule {
            parentAName  = "Charmander",
            parentACount = 5,
            parentBName  = "Ivysaur",
            parentBCount = 2,
            childName    = "Charvysaur"
          },
          new SplicingRule {
            parentAName  = "Pikachu",
            parentACount = 3,
            parentBName  = "Bulbasaur",
            parentBCount = 2,
            childName    = "Pikasaur"
          },
          new SplicingRule {
            parentAName  = "Charmander",
            parentACount = 4,
            parentBName  = "Raichu",
            parentBCount = 3,
            childName    = "Charaichu"
          },
          new SplicingRule {
            parentAName  = "Flareon",
            parentACount = 2,
            parentBName  = "Eeveechu",
            parentBCount = 1,
            childName    = "Flareeveechu"
          },
          new SplicingRule {
            parentAName  = "Bulbasaur",
            parentACount = 3,
            parentBName  = "Charvysaur",
            parentBCount = 2,
            childName    = "Charvysaurion"
          }
        };

        await _context.AddRangeAsync(seeds);
        await _context.SaveChangesAsync();
      }
    }

    public bool HandleSpliceMenu() {
      string input = this.drawSpliceMenu();

      switch (input) {
        case "List Splicing Recipes":
          displaySplicingRecipes();
          return false;
        case "Display Current Eligible Recipes":
          displayEligibleSpliceRecipes();
          return false;
        case "Splice Eligible Pokemon":
          return false;
        case "Go Back":
          return true;
      }
      return true;
    }

    private string drawSpliceMenu() {
      var selection = AnsiConsole.Prompt(
          new SelectionPrompt<string>()
          .Title("[bold yellow]==== The PokeSplicer ====[/]")
          .PageSize(10)
          .AddChoices(new[] {
            "List Splicing Recipes",
            "Display Current Eligible Recipes",
            "Splice Eligible Pokemon",
            "Go Back"
            }));

      return selection;
    }

    private void displaySplicingRecipes() {
      var table = new Table();
      table.Border = TableBorder.Rounded;
      table.Title("[bold underline]All Splicing Recipes[/]");
      table.AddColumn("Sample A");
      table.AddColumn("A count");
      table.AddColumn("Sample B");
      table.AddColumn("B count");
      table.AddColumn("Outcome");

      List<SplicingRule> rules = this._context.SplicingRules.ToList();

      foreach (SplicingRule rule in rules) {
        table.AddRow(rule.parentAName, rule.parentACount.ToString(), rule.parentBName, rule.parentBCount.ToString(), rule.childName);
      }

      AnsiConsole.Write(table);
      continueToMenu();
    }

    private void displayEligibleSpliceRecipes() {
      List<SplicingRule> rules = this._context.SplicingRules.ToList();

      var table = new Table();
      table.Border = TableBorder.Rounded;
      table.Title("[bold underline]Eligible Splices[/]");
      table.AddColumn("Pokemon A");
      table.AddColumn("Pokemon B");
      table.AddColumn("Outcome");

      bool eligibleSplices = false;
      
      foreach (SplicingRule rule in rules) {
        List<Pokemon> currPokemon = PokemonService.GetPlayerPokemon(this._context)
          .Where(p => p.Name == rule.parentAName || p.Name == rule.parentBName)
          .ToList();

        List<Pokemon> parentAPokemon = currPokemon.Where(p => p.Name == rule.parentAName).ToList();
        List<Pokemon> parentBPokemon = currPokemon.Where(p => p.Name == rule.parentBName).ToList();

        int childCount = System.Math.Min(parentAPokemon.Count() / rule.parentACount, parentBPokemon.Count() / rule.parentBCount);
        if (childCount >= 1) {
          eligibleSplices = true;
          table.AddRow($"{rule.parentACount * childCount} {rule.parentAName}", $"{rule.parentBCount * childCount} {rule.parentBName}", $"{childCount} {rule.childName}");
        }
      }
      if (eligibleSplices) {
        AnsiConsole.Write(table);
      } else {
        AnsiConsole.WriteLine("[red]You have no pokemon eligible for splicing.[/]");
      }
      continueToMenu();
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
