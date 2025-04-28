using System;
using System.Linq;
using PokemonPocket.Models;
using PokemonPocket.Data;
using PokemonPocket.Services;

namespace PokemonPocket
{
    class Program
    {
        static void Main(string[] args)
        {

            using PokemonPocketContext context = new PokemonPocketContext();
            PokemonService service = new PokemonService(context);

            var pikachu_query = context.EvolutionRules.FirstOrDefault(p => p.Name == "Pikachu");
            if (!(pikachu_query is PokemonMaster))
            {
                PokemonMaster pikachu = new PokemonMaster()
                {
                    Name = "Pikachu",
                    NoToEvolve = 2,
                    EvolveTo = "Raichu"
                };
                context.EvolutionRules.Add(pikachu);
                context.SaveChanges();
            }

            var eevee_query = context.EvolutionRules.FirstOrDefault(p => p.Name == "Eevee");
            if (!(eevee_query is PokemonMaster))
            {
                PokemonMaster eevee = new PokemonMaster()
                {
                    Name = "Eevee",
                    NoToEvolve = 3,
                    EvolveTo = "Flareon"
                };
                context.EvolutionRules.Add(eevee);
                context.SaveChanges();
            }

            var charmander_query = context.EvolutionRules.FirstOrDefault(p => p.Name == "Charmander");
            if (!(charmander_query is PokemonMaster))
            {
                PokemonMaster charmander = new PokemonMaster()
                {
                    Name = "Charmander",
                    NoToEvolve = 1,
                    EvolveTo = "Charmeleon"
                };
                context.EvolutionRules.Add(charmander);
                context.SaveChanges();
            }

            context.SaveChanges();

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

