namespace PokemonPocket.Models
{
    public class Flareon : Pokemon
    {
        public Flareon() : base("Flareon", hp: 90, exp: 0, skill: "Run Away", skillDamage: 25) { }

        protected override int GetDamageMultiplier() => 2;
    }
}

