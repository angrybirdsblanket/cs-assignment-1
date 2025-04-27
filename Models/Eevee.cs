namespace PokemonPocket.Models
{
    public class Eevee : Pokemon
    {
        public Eevee() : base("Eevee", hp: 90, exp: 0, skill: "Run Away", skillDamage: 25) { }

        protected override int GetDamageMultiplier() => 2;
    }
}
