//Ivan Dochev 241836X
using System.ComponentModel.DataAnnotations;

namespace PokemonPocket.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }
        public int Gold { get; set; }
        public ICollection<Badge> Badges { get; set; } = new List<Badge>();

        public Player()
        {
            this.Gold = 10;
        }
    }
}
