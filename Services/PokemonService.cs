using System;
using System.Collections.Generic;
using System.Linq;
using PokemonPocket.Data;
using PokemonPocket.Models;
namespace PokemonPocket.Services

{
    public class PokemonService
    {
        private readonly PokemonPocketContext _context;
        private readonly BattleService _battles;

        public PokemonService(PokemonPocketContext context, BattleService battles)
        {
            this._context = context;
            this._battles = battles;
        }

        private void addPokemon()
        {

            Console.Write("Enter Pokemon's Name: ");
            string name = Console.ReadLine();

            if (!new[] { "pikachu", "eevee", "charmander" }.Contains(name.ToLower()))
            {
                Console.WriteLine("Pokemon not recognised.");
                return;
            }

            Console.Write("Enter Pokemon's HP: ");
            int hp = Int32.Parse(Console.ReadLine());

            Console.Write("Enter Pokemon's Exp: ");
            int exp = Int32.Parse(Console.ReadLine());

            switch (name.ToLower())
            {

                case "pikachu":

                    Pikachu pikachu = new Pikachu(hp, exp);

                    this._context.Add(pikachu);
                    break;

                case "charmander":
                    Charmander charmander = new Charmander(hp, exp);

                    this._context.Add(charmander);

                    break;

                case "eevee":
                    Eevee eevee = new Eevee(hp, exp);

                    this._context.Add(eevee);

                    break;
            }

            this._context.SaveChanges();
        }

        private void listPokemon()
        {
            var pokemon = this._context.Pokemon
              .OrderByDescending(p => p.Exp)
              .ToList();

            if (pokemon.Count == 0)
            {
                Console.WriteLine("Your Pocket is empty");
            }

            foreach (var p in pokemon)
            {
                Console.WriteLine($"Name: {p.Name}");
                Console.WriteLine($"HP: {p.HP}");
                Console.WriteLine($"Exp: {p.Exp}");
                Console.WriteLine($"Name: {p.Skill}");
                Console.WriteLine($"------------------------------");
            }
        }

        private void checkEvolutionStatus()
        {
            List<PokemonMaster> rules = this._context.EvolutionRules
              .ToList();
            foreach (PokemonMaster rule in rules)
            {
                var list = this._context.Pokemon
                  .Where(p => p.Name == rule.Name)
                  .ToList();

                if (list.Count >= rule.NoToEvolve)
                {
                    int eligibleCount = list.Count / rule.NoToEvolve * rule.NoToEvolve;

                    Console.WriteLine($"{eligibleCount} {rule.Name} ---> {eligibleCount / rule.NoToEvolve} {rule.EvolveTo}");
                }
            }
        }

        private void evolveEligiblePokemon()
        {
            // Load all the evolution rules
            var rules = _context.EvolutionRules
              .ToList();

            foreach (var rule in rules)
            {
                // Get all Pokémon matching the base name, highest Exp first
                var pocket = _context.Pokemon
                  .Where(p => p.Name == rule.Name)
                  .OrderByDescending(p => p.Exp)
                  .ToList();

                // How many full groups of NoToEvolve we have
                int fullGroups = pocket.Count / rule.NoToEvolve;

                for (int g = 0; g < fullGroups; g++)
                {
                    // Take exactly rule.NoToEvolve from the top
                    var batch = pocket
                      .Skip(g * rule.NoToEvolve)
                      .Take(rule.NoToEvolve)
                      .ToList();

                    // Compute the stats for the new evolved form
                    int maxHp = batch.Max(p => p.HP);
                    int maxExp = batch.Max(p => p.Exp);

                    // Remove all originals in one go
                    _context.Pokemon.RemoveRange(batch);

                    // Create the right evolved type
                    Pokemon evolved = rule.EvolveTo.ToLower() switch
                    {
                        "raichu" => new Raichu { HP = maxHp, Exp = maxExp, Name = "Raichu" },
                        "charmeleon" => new Charmeleon { HP = maxHp, Exp = maxExp, Name = "Charmeleon" },
                        "flareon" => new Flareon { HP = maxHp, Exp = maxExp, Name = "Flareon" },
                        _ => throw new InvalidOperationException($"Unknown evolution target: {rule.EvolveTo}")
                    };

                    // Add the evolved Pokémon
                    _context.Pokemon.Add(evolved);
                }
            }

            // Persist *all* the removals + adds in a single transaction
            _context.SaveChanges();
        }

        private void playerStats() {
          Player player = this._context.Players
            .Where(p => p.Id == 1)
            .First();
  
          List<Pokemon> healablePokemon = this._context.Pokemon
            .Where(p => p.MaxHP != p.HP)
            .ToList();

          Console.WriteLine($"You currently have {player.Gold} gold, and {healablePokemon.Count()} of your pokemon currently require healing");
        }

        private void healPokemon() {
          var pokemonList = this._context.Pokemon
            .OrderBy(p => p.Id)
            .Where(p => p.HP != p.MaxHP)
            .ToList();

          int totalHealthPercentMissing = 0;
          foreach (Pokemon pokemon in pokemonList) {
            int healthPercentMissing = (pokemon.MaxHP - pokemon.HP) * 100 / pokemon.MaxHP;
            totalHealthPercentMissing += healthPercentMissing;
          }

          int goldRequired = totalHealthPercentMissing / 10;

          Console.WriteLine($"To heal your Pokemon, you need to pay {goldRequired} gold.");

        }

        private void drawMenu()
        {
            Console.WriteLine("*****************************");
            Console.WriteLine("Welcome to Pokemon Pocket App");
            Console.WriteLine("*****************************");
            Console.WriteLine("(1). Add Pokemon to my Pocket");
            Console.WriteLine("(2). List Pokemon(s) in my Pocket");
            Console.WriteLine("(3). Check if I can evolve my Pokemon");
            Console.WriteLine("(4). Evolve Pokemon");
            Console.WriteLine("(5). Check your player Statistics");
            Console.WriteLine("(6). Heal your Pokemon");
            Console.WriteLine("(7). Fight and catch a Pokemon");
            Console.Write("Please enter [1,2,3,4,5,6,7] or Q to quit: ");
        }

        public bool GetNextAction()
        {
            this.drawMenu();

            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Invalid input, please try again");
                return true;
            }

            char response = input[0];

            switch (response)
            {
                case '1':
                    addPokemon();
                    break;
                case '2':
                    listPokemon();
                    break;
                case '3':
                    checkEvolutionStatus();
                    break;
                case '4':
                    evolveEligiblePokemon();
                    break;
                case '5':
                    playerStats();
                    break;
                case '6':
                    healPokemon();
                    break;
                case '7':
                    if (this._context.Pokemon.Count() > 0)
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
                        Console.WriteLine("Your Pocket is empty, please come back once you have pokemon inside.");
                    }
                    break;
                case 'q':
                case 'Q':
                    return false;
                default:
                    return true;
            }
            return true;
        }

        public void InitialiseEvoRules()
        {

            var pikachu_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Pikachu");
            if (!(pikachu_query is PokemonMaster))
            {
                PokemonMaster pikachu = new PokemonMaster()
                {
                    Name = "Pikachu",
                    NoToEvolve = 2,
                    EvolveTo = "Raichu"
                };
                this._context.EvolutionRules.Add(pikachu);
                this._context.SaveChanges();
            }

            var eevee_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Eevee");
            if (!(eevee_query is PokemonMaster))
            {
                PokemonMaster eevee = new PokemonMaster()
                {
                    Name = "Eevee",
                    NoToEvolve = 3,
                    EvolveTo = "Flareon"
                };
                this._context.EvolutionRules.Add(eevee);
                this._context.SaveChanges();
            }

            var charmander_query = this._context.EvolutionRules.FirstOrDefault(p => p.Name == "Charmander");
            if (!(charmander_query is PokemonMaster))
            {
                PokemonMaster charmander = new PokemonMaster()
                {
                    Name = "Charmander",
                    NoToEvolve = 1,
                    EvolveTo = "Charmeleon"
                };
                this._context.EvolutionRules.Add(charmander);
                this._context.SaveChanges();
            }

        }

    }
}

