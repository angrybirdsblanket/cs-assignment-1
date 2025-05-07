using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PokemonPocket.Models
{
    public class GymLeader
    {
        [Key]
        public int Id { get; set; }
        
        public string GymName { get; set; } = string.Empty;
        
        public List<Pokemon> PokemonTeam { get; set; } 

        public string BadgeName;
    }
}
