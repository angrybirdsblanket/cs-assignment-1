using PokemonPocket.Services;

namespace PokemonPocket
{
    class Program
    {
        static void Main(string[] args)
        {

            using PokemonPocketContext context = new PokemonPocketContext();

            context.Database.EnsureCreated();

            BattleService battles = new BattleService(context);
            GymService gyms = new GymService(context);
            PokemonService service = new PokemonService(context, battles, gyms);

            service.InitialiseEvoRules();
            gyms.InitialiseGyms();

            if (args.Count() > 0 && "--seed".Equals(args[0]))
            {
                List<Pokemon> list = PokemonService.GetPlayerPokemon(context);
                context.RemoveRange(list);
                service.testPokemon();
            }

            /* there will always be one player
               if no player is found, the below if statement will generate one and add to the database*/

            if (context.Players.FirstOrDefault() == null)
            {
                Player player = new Player();
                context.Add(player);
                context.SaveChanges();
            }

            bool running = true;

            // main game loop
            while (running)
            {
                running = service.GetNextAction();
            }
            Environment.Exit(0);

        }
    }
}
