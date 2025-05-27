namespace PokemonPocket.Models {

  public class Charvysaur : Pokemon {

    public Charvysaur() : base("Charvysaur", hp: 120, exp: 0, skill: "Flame Vine", skillDamage: 50) { }
    
    public Charvysaur(int hp, int exp, int level, int skillDamage): base("Charvysaur", hp, exp, "Flame Vine", skillDamage: 50 + skillDamage) {
      this.Level = level;
    }
  
    protected override int GetDamageMultiplier() => 7;
  }
  
}
