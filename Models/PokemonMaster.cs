using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PokemonPocket.Models
{
    public class PokemonMaster
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int NoToEvolve { get; set; }
        public string EvolveTo { get; set; } = null!;
    }
}
