using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PokemonPocket.Models {

  public class Player {

    [Key]
    public int Id { get; set; }
    public int gold { get; set; }
    public ICollection<Badge> Badges { get; set; } = null!;

    public Player() {
      this.gold = 10;
    }

  }

}
