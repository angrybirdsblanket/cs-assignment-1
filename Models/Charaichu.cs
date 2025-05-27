namespace PokemonPocket.Models {
  public class Charaichu : Pokemon {

    public Charaichu() : base("Charaichu", hp: 200, exp: 0, skill: "Static Flare", skillDamage: 40) { }
    
    public Charaichu(int hp, int exp, int level, int skillDamage) : base("Charaichu", hp, exp, "Static Flare", skillDamage: 40 + skillDamage) {
      this.Level = level;
    }
  
    protected override int GetDamageMultiplier() => 7;
  }
  
}
