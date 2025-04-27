// TODO: Implement the PokemonService class to act as the basic game logic layer
using System;
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
        Console.WriteLine("addPokemon executed");
      }

      private void listPokemon() {
        Console.WriteLine("listPokemon executed");
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
