//Ivan Dochev 241836X
using System.ComponentModel.DataAnnotations;

namespace PokemonPocket.Models
{
    public class Badge
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public Badge() { }

        public Badge(string name, string type, int playerId, Player player)
        {
            this.Name = name;
            this.Type = type;
            this.PlayerId = playerId;
            this.Player = player;
        }
    }
}
