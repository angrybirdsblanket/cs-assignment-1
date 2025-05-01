using System;
using System.Linq;
using PokemonPocket.Data;
using PokemonPocket.Models;
using PokemonPocket.Services;

namespace PokemonPocket
{
  class Program
  {
    static void Main(string[] args)
    {

      using PokemonPocketContext context = new PokemonPocketContext();
      BattleService battles = new BattleService(context);
      PokemonService service = new PokemonService(context, battles);

      service.InitialiseEvoRules();

      /* there will always be one player
         if no player is found, the below if statement will generate one and add to the database*/

      if (context.Players.FirstOrDefault() == null) {
        Player player = new Player();
        context.Add(player);
        context.SaveChanges();
      }

      bool running = true;

      // main game loop
      while (running)
      {
        running = service.GetNextAction();
        Console.WriteLine();
      }
      Environment.Exit(0);

    }
  }
}

