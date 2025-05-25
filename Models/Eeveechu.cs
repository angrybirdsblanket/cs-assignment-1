namespace PokemonPocket.Models {
  class Eeveechu : Pokemon {

    public Eeveechu() : base("Eeveechu", hp: 200, exp: 0, skill: "Volt Dash", skillDamage: 60) { }

    public Eeveechu(int hp, int exp, int level, int skillDamage): base("Eeveechu", hp, exp, "Volt Dash", skillDamage: 60 + skillDamage) {
      this.Level = level;
    }

    protected override int GetDamageMultiplier() => 5;
  }
}
