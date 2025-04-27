// TODO: Implement the PokemonService class to act as the basic game logic layer
using System;
using System.Linq;
using PokemonPocket.Data;
using PokemonPocket.Models;
namespace PokemonPocket.Services
{
    public class PokemonService     
    {
      private readonly PokemonPocketContext _context;

      public PokemonService(PokemonPocketContext context) {
        this._context = context;
      }

      private void addPokemon() {

        Console.Write ("Enter Pokemon's Name: ");
        string name = Console.ReadLine();

        Console.Write ("Enter Pokemon's HP: ");
        string hp = Console.ReadLine();

        Console.Write ("Enter Pokemon's Exp: ");
        string exp = Console.ReadLine();

        switch (name.ToLower()) {

          case "pikachu":
            Pikachu pikachu = new Pikachu(){
              HP = int.Parse(hp),
              Name = "Pikachu",
              Exp = int.Parse(exp)
            };

            this._context.Add(pikachu);
            break;

          case "charmander":
            Charmander charmander = new Charmander(){
              HP = int.Parse(hp),
              Name = "Charmander",
              Exp = int.Parse(exp)
            };

            this._context.Add(charmander);

            break;

          case "eevee":
            Eevee eevee = new Eevee(){
              HP = int.Parse(hp),
              Name = "Eevee",
              Exp = int.Parse(exp)
            };

            this._context.Add(eevee);

            break;

          default:
            Console.WriteLine("Pokemon not recognized.");
            break;
        }
        
        this._context.SaveChanges();
      }

      private void listPokemon() {
        var pokemon = this._context.Pokemon
          .OrderByDescending(p => p.Exp)
          .ToList();
        
        foreach (var p in pokemon) {
          Console.WriteLine($"Name: {p.Name}");
          Console.WriteLine($"HP: {p.HP}");
          Console.WriteLine($"Name: {p.Exp}");
          Console.WriteLine($"Name: {p.Skill}");
          Console.WriteLine($"------------------------------");
        }
      }

      private void checkEvolutionStatus() {
        Console.WriteLine("checkEvolutionStatus executed");
      }

      private void evolveEligiblePokemon() {
        Console.WriteLine("evolveEligiblePokemon executed");
      }

      private void drawMenu() {
        Console.WriteLine("*****************************");
        Console.WriteLine("Welcome to Pokemon Pocket App");
        Console.WriteLine("*****************************");
        Console.WriteLine("(1). Add Pokemon to my Pocket");
        Console.WriteLine("(2). List Pokemon(s) in my Pocket");
        Console.WriteLine("(3). Check if I can evolve my Pokemon");
        Console.WriteLine("(4). Evolve Pokemon");
        Console.Write("Please enter [1,2,3,4] or Q to quit: ");
      }

      public bool GetNextAction() {
        this.drawMenu();

        string input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) return true;  // Handle invalid or empty input
        char response = input[0];

        switch (response) {
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
          case 'q':
          case 'Q':
            return false;
          default:
            return true;
        }
        // this is unreachable code, but it is here to satisfy the compiler
        return true;
      }


    }
}
