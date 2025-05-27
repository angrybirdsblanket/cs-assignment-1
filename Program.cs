using PokemonPocket.Services;
using System.Threading.Tasks;

namespace PokemonPocket
{
    class Program
    {
        static async Task Main(string[] args)
        {

            using PokemonPocketContext context = new PokemonPocketContext();

            context.Database.EnsureCreated();

            BattleService battles = new BattleService(context);
            GymService gyms = new GymService(context);
            SpliceService splice = new SpliceService(context);
            PokemonService service = new PokemonService(context, battles, gyms, splice);

            await service.InitialiseEvoRulesAsync();
            await splice.InitialiseSplicingRulesAsync();
            await gyms.InitialiseGymsAsync();

            if (args.Count() > 0 && "--seed".Equals(args[0]))
            {
                List<Pokemon> list = PokemonService.GetPlayerPokemon(context);
                context.RemoveRange(list);
                service.TestPokemon();
                service.TestPokemon();
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
