//Ivan Dochev 241836X
using System.ComponentModel.DataAnnotations;

namespace PokemonPocket.Models
{
    public class GymLeader
    {
        [Key]
        public int Id { get; set; }

        public string GymName { get; set; } = string.Empty;

        public List<Pokemon> PokemonTeam { get; set; }

        public string BadgeName { get; set; }

        public bool Defeated { get; set; }

        public string TrainerName { get; set; }
    }
}
