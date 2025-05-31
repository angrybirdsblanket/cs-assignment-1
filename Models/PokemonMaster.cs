//Ivan Dochev 241836X
using System.ComponentModel.DataAnnotations;

namespace PokemonPocket.Models
{
    public class PokemonMaster
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int NoToEvolve { get; set; }
        public string EvolveTo { get; set; }

        // default constructor
        public PokemonMaster() { } // for EF Core

        public PokemonMaster(int id, string name, int noToEvolve, string evolveTo)
        {
            Id = id;
            Name = name;
            NoToEvolve = noToEvolve;
            EvolveTo = evolveTo;
        }
    }
}
