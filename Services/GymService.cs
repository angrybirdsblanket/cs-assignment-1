using System;
using System.Collections.Generic;
using System.Linq;
using PokemonPocket.Data;
using PokemonPocket.Models;
using Microsoft.EntityFrameworkCore;

namespace PokemonPocket.Services
{

    public class GymService
    {

        private readonly PokemonPocketContext _context;
        private readonly Random _random = new Random();

        public GymService(PokemonPocketContext context)
        {
            this._context = context;
        }

        private void displayGymMenu()
        {
            Console.WriteLine("==== Pokemon Gym ====");
            Console.WriteLine("(1) Check Upcoming Gym battles");
            Console.WriteLine("(2) Check next Gym Leader's Team");
            Console.WriteLine("(3) Start Gym fight");
            Console.Write("Please enter [1,2,3] or B to go back: ");
        }



        public bool HandleGymMenu()
        {
            this.displayGymMenu();

            string input;

            input = Console.ReadLine();
            while (string.IsNullOrEmpty(input))
            {

                Console.WriteLine("No input was detected, please try again");
                this.displayGymMenu();
                input = Console.ReadLine();
            }

            input = input.ToLower();

            switch (input)
            {
                case "1":
                    listGyms();
                    return true;
                case "2":
                    getNextFightPokemon();
                    return true;
                case "3":
                    startGymFight();
                    return true;
                case "b":
                    return true;
                default:
                    Console.Clear();
                    Console.WriteLine("An invalid character was detected, please try again");
                    Console.WriteLine();
                    return false;
            }
        }

        public void InitialiseGyms()
        {
            // Check if gym leaders have already been added
            var firstLaunchTest = this._context.GymLeaders.FirstOrDefault();
            if (firstLaunchTest == null)
            {
                // Define gym leaders with exactly 3 Pokémon each, using their default skills
                GymLeader leader_1 = new GymLeader
                {
                    GymName = "Thunderdome",
                    BadgeName = "Electric Badge",
                    PokemonTeam = new List<Pokemon>
          {
            new Pikachu { Name = "Pikachu", HP = 200, MaxHP = 200, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 30, Level = 2 },
            new Raichu { Name = "Raichu", HP = 400, MaxHP = 400, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 40, Level = 3 },
            new Eevee { Name = "Eevee", HP = 300, MaxHP = 300, Exp = 0, Skill = "Run Away", SkillDamage = 20, Level = 2 }
          },
                    Defeated = false,
                    TrainerName = "Arden"
                };

                GymLeader leader_2 = new GymLeader
                {
                    GymName = "Verdant Grove",
                    BadgeName = "Grass Badge",
                    PokemonTeam = new List<Pokemon>
          {
            new Bulbasaur { Name = "Bulbasaur", HP = 325, MaxHP = 325, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 25, Level = 3 },
            new Ivysaur { Name = "Ivysaur", HP = 450, MaxHP = 450, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 30, Level = 4 },
            new Eevee { Name = "Eevee", HP = 300, MaxHP = 300, Exp = 0, Skill = "Run Away", SkillDamage = 15, Level = 2 }
          },
                    Defeated = false,
                    TrainerName = "Kael"
                };

                GymLeader leader_3 = new GymLeader
                {
                    GymName = "Blazing Inferno",
                    BadgeName = "Fire Badge",
                    PokemonTeam = new List<Pokemon>
          {
            new Charmander { Name = "Charmander", HP = 350, MaxHP = 350, Exp = 0, Skill = "Solar Power", SkillDamage = 20, Level = 3 },
            new Flareon { Name = "Flareon", HP = 450, MaxHP = 450, Exp = 0, Skill = "Run Away", SkillDamage = 40, Level = 4 },
            new Charmeleon { Name = "Charmeleon", HP = 500, MaxHP = 500, Exp = 0, Skill = "Solar Power", SkillDamage = 35, Level = 5 }
          },
                    Defeated = false,
                    TrainerName = "Liora"
                };

                GymLeader leader_4 = new GymLeader
                {
                    GymName = "Mystic Lagoon",
                    BadgeName = "Water Badge",
                    PokemonTeam = new List<Pokemon>
          {
            new Ivysaur { Name = "Ivysaur", HP = 500, MaxHP = 500, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 35, Level = 5 },
            new Eevee { Name = "Eevee", HP = 525, MaxHP = 525, Exp = 0, Skill = "Run Away", SkillDamage = 25, Level = 4 },
            new Flareon { Name = "Flareon", HP = 450, MaxHP = 450, Exp = 0, Skill = "Run Away", SkillDamage = 30, Level = 5 }
          },
                    Defeated = false,
                    TrainerName = "Darius"
                };

                GymLeader leader_5 = new GymLeader
                {
                    GymName = "Dragon's Den",
                    BadgeName = "Dragon Badge",
                    PokemonTeam = new List<Pokemon>
          {
            new Charmeleon { Name = "Charmeleon", HP = 550, MaxHP = 550, Exp = 0, Skill = "Solar Power", SkillDamage = 40, Level = 6 },
            new Raichu { Name = "Raichu", HP = 600, MaxHP = 600, Exp = 0, Skill = "Lightning Bolt", SkillDamage = 50, Level = 7 },
            new Ivysaur { Name = "Ivysaur", HP = 525, MaxHP = 525, Exp = 0, Skill = "Verdant Spiral", SkillDamage = 45, Level = 6 }
          },
                    Defeated = false,
                    TrainerName = "Sylas"
                };

                // Add all gym leaders to the database
                this._context.GymLeaders.AddRange(new[] { leader_1, leader_2, leader_3, leader_4, leader_5 });
                this._context.SaveChanges();
            }
        }

        private void listGyms()
        {
            var gyms = this._context.GymLeaders
              .ToList();

            Console.WriteLine("\nThe following gyms are coming up:");
            Console.WriteLine("-------------------------------");

            int gym = 1;
            foreach (GymLeader leader in gyms)
            {
                string status = leader.Defeated ? "[DEFEATED]" : "[CHALLENGE AVAILABLE]";
                Console.WriteLine($"{gym}. {leader.GymName} - {leader.TrainerName} {status}");
                gym++;
            }

            Console.WriteLine();
        }

        private void getNextFightPokemon()
        {
            var nextLeader = this._context.GymLeaders
              .Where(l => l.Defeated == false)
              .Include(l => l.PokemonTeam)
              .FirstOrDefault();

            if (nextLeader != null)
            {
                Console.WriteLine($"\nThe {nextLeader.GymName} is next");
                Console.WriteLine($"Gym Leader: {nextLeader.TrainerName}");
                Console.WriteLine($"Badge: {nextLeader.BadgeName}");
                Console.WriteLine("\nYou will be fighting the following pokemon:");
                Console.WriteLine("----------------------------------------");

                foreach (Pokemon pokemon in nextLeader.PokemonTeam)
                {
                    Console.WriteLine($"{pokemon.Name}: {pokemon.MaxHP} HP, Level {pokemon.Level}, Skill: {pokemon.Skill} ({pokemon.SkillDamage} damage)");
                }
            }
            else
            {
                Console.WriteLine("\nCongratulations! You have defeated all gym leaders!");
            }
        }

        private void startGymFight()
        {
            var leader = this._context.GymLeaders
              .Include(l => l.PokemonTeam)
              .Where(l => l.Defeated == false)
              .FirstOrDefault();

            if (leader == null)
            {
                Console.WriteLine("\nYou have defeated all leaders! Congratulations on becoming a Pokemon Master!");
                Console.WriteLine();
                return;
            }

            // Create copies of the Pokemon teams for battle to avoid modifying the database directly during fight
            List<Pokemon> leaderTeam = leader.PokemonTeam.Where(p => p.HP > 0).ToList();

            // Get player's Pokemon using PokemonService (assuming it's a static method)
            List<Pokemon> pocket = PokemonService.GetPlayerPokemon(this._context)
              .Where(p => p.HP > 0)
              .ToList();

            if (pocket.Count() == 0)
            {
                Console.WriteLine("\nYou do not have any Pokemon available for battle.");
                Console.WriteLine("Please heal your Pokemon at the Pokemon Center before challenging a gym.");
                return;
            }

            Console.Clear();
            Console.WriteLine($"=== GYM BATTLE: {leader.GymName.ToUpper()} ===");
            Console.WriteLine($"\nGym Leader {leader.TrainerName} challenges you to a battle!");
            Console.WriteLine($"The {leader.BadgeName} is at stake!");

            // Battle logic starts here
            bool playerWon = true;

            int pokemonSelection = selectPokemon(pocket);
            Pokemon playerPokemon = pocket[pokemonSelection];

            int currentLeaderPokemonIndex = 0;
            Pokemon leaderCurrentPokemon = leaderTeam[currentLeaderPokemonIndex];
            Console.WriteLine($"\nGym Leader {leader.TrainerName} sends out {leaderCurrentPokemon.Name}!");
            Console.WriteLine($"You send out {playerPokemon.Name}!");

            while (pocket.Count > 0 && leaderTeam.Count > 0)
            {
                // Give player option to attack or switch Pokemon
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("(1) Attack");
                Console.WriteLine("(2) Switch Pokemon");
                Console.Write("Your choice: ");

                string actionChoice = Console.ReadLine();

                if (actionChoice == "2")
                {
                    // Switch Pokemon
                    pokemonSelection = selectPokemon(pocket);
                    playerPokemon = pocket[pokemonSelection];
                    Console.WriteLine($"You switch to {playerPokemon.Name}!");

                    // Leader gets a free attack when player switches
                    int damage = leaderCurrentPokemon.Attack(playerPokemon);
                    Console.WriteLine($"The trainer's {leaderCurrentPokemon.Name} attacked your {playerPokemon.Name} for {damage} damage and left it with {playerPokemon.HP} HP!");

                    if (playerPokemon.HP <= 0)
                    {
                        Console.WriteLine($"{playerPokemon.Name} has fainted.");
                        pocket.RemoveAt(pokemonSelection);

                        if (pocket.Count == 0)
                        {
                            playerWon = false;
                            break;
                        }

                        pokemonSelection = selectPokemon(pocket);
                        playerPokemon = pocket[pokemonSelection];
                        Console.WriteLine($"You send out {playerPokemon.Name}!");
                    }

                    continue;
                }

                // Execute battle round
                executeBattleRound(playerPokemon, leaderCurrentPokemon);

                // Check if leader's Pokémon fainted
                if (leaderCurrentPokemon.HP <= 0)
                {
                    Console.WriteLine($"Gym Leader's {leaderCurrentPokemon.Name} has fainted!");

                    // Remove the fainted Pokémon from the active list
                    leaderTeam.RemoveAt(currentLeaderPokemonIndex);

                    if (leaderTeam.Count == 0)
                    {
                        Console.WriteLine($"You've defeated all of Gym Leader {leader.TrainerName}'s Pokémon!");
                        break;
                    }

                    // Leader sends out next Pokémon
                    currentLeaderPokemonIndex = 0;
                    leaderCurrentPokemon = leaderTeam[currentLeaderPokemonIndex];
                    Console.WriteLine($"Gym Leader {leader.TrainerName} sends out {leaderCurrentPokemon.Name}!");
                }

                // Check if player's Pokémon fainted
                if (playerPokemon.HP <= 0)
                {
                    Console.WriteLine($"{playerPokemon.Name} has fainted.");
                    pocket.RemoveAt(pokemonSelection);

                    if (pocket.Count == 0)
                    {
                        playerWon = false;
                        break;
                    }

                    pokemonSelection = selectPokemon(pocket);
                    playerPokemon = pocket[pokemonSelection];
                    Console.WriteLine($"You send out {playerPokemon.Name}!");
                }
            }

            if (playerWon)
            {
                Console.WriteLine($"\nCongratulations! You've defeated Gym Leader {leader.TrainerName}!");
                Console.WriteLine($"You've earned the {leader.BadgeName}!");

                // Mark the gym leader as defeated
                var dbLeader = this._context.GymLeaders.Find(leader.Id);
                if (dbLeader != null)
                {
                    dbLeader.Defeated = true;

                    // Restore leader's Pokemon HP for future view
                    foreach (var pokemon in dbLeader.PokemonTeam)
                    {
                        pokemon.HP = pokemon.MaxHP;
                    }
                }

                // Give player a reward
                Player player = this._context.Players.Where(p => p.Id == 1).First();
                int goldReward = 100 * (leader.PokemonTeam.Count);
                player.Gold += goldReward;
                Console.WriteLine($"You received {goldReward} gold as a reward!");

                // Level up eligible Pokémon
                var levelablePokemon = PokemonService.GetPlayerPokemon(this._context)
                  .Where(p => p.Exp >= 100)
                  .ToList();

                foreach (Pokemon pokemon in levelablePokemon)
                {
                    pokemon.LevelUp();
                    Console.WriteLine($"Your {pokemon.Name} leveled up to level {pokemon.Level}!");
                }
            }
            else
            {
                Console.WriteLine($"\nYou were defeated by Gym Leader {leader.TrainerName}.");
                Console.WriteLine("Train harder and try again!");
            }

            this._context.SaveChanges();

            Console.WriteLine();
        }

        private void calculateExp(Pokemon enemy, Pokemon attacker, int damageDealt)
        {
            const int BaseExp = 50;
            double damageRatio = (double)damageDealt / enemy.MaxHP;
            double rawXp = damageRatio * BaseExp;

            int xpGained = (int)Math.Floor(rawXp);
            if (damageDealt > 0 && xpGained < 1)
                xpGained = 1;

            attacker.Exp += xpGained;
            this._context.SaveChanges();
        }

        private List<Pokemon> getAvailablePokemon()
        {
            return PokemonService.GetPlayerPokemon(this._context)
              .OrderBy(p => p.Id)
              .Where(p => p.HP > 0)
              .ToList();
        }

        private int selectPokemon(List<Pokemon> pocket)
        {
            int selection = -1;

            while (selection < 0 || selection >= pocket.Count)
            {
                Console.WriteLine("\nChoose a Pokémon from your pocket:");
                for (int i = 0; i < pocket.Count; i++)
                {
                    var p = pocket[i];
                    Console.WriteLine($"{i}: --> {p.Name}, HP: {p.HP}/{p.MaxHP}, Level: {p.Level}, Exp: {p.Exp}/100");
                }

                Console.Write("Your choice: ");
                string input = Console.ReadLine();

                if (!int.TryParse(input, out selection) || selection < 0 || selection >= pocket.Count)
                {
                    Console.WriteLine("Invalid selection. Please try again.");
                    selection = -1;
                }
            }

            return selection;
        }

        private void executeBattleRound(Pokemon attacker, Pokemon leaderPokemon)
        {
            int damage = attacker.Attack(leaderPokemon);
            Console.WriteLine($"Your {attacker.Name} attacked the trainer's {leaderPokemon.Name} for {damage} damage and left it with {leaderPokemon.HP} HP!");

            // Calculate EXP gain for player's Pokemon
            calculateExp(leaderPokemon, attacker, damage);

            if (leaderPokemon.HP > 0)
            {
                damage = leaderPokemon.Attack(attacker);
                Console.WriteLine($"The trainer's {leaderPokemon.Name} attacked your {attacker.Name} for {damage} damage and left it with {attacker.HP} HP!");
            }
        }
    }
}
