using System;
using PokemonPocket.Data;
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

