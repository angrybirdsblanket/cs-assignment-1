namespace PokemonPocket.Models {

  public class Charvysaur : Pokemon {

    public Charvysaur() : base("Charvysaur", hp: 120, exp: 0, skill: "Vine Whip", skillDamage: 50) { }
    
    public Charvysaur(int hp, int exp, int level, int skillDamage): base("Eeveechu", hp, exp, "Volt Dash", skillDamage: 50 + skillDamage) {
      this.Level = level;
    }
  
    protected override int GetDamageMultiplier() => 7;
  }
  
}
