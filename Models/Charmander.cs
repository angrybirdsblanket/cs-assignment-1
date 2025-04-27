namespace PokemonPocket.Models
{

    public class Charmander : Pokemon
    {
        public Charmander() : base("Charmander", hp: 110, exp: 0, skill: "Solar Power", skillDamage: 10) { }

        protected override int GetDamageMultiplier() => 1;
    }
}
