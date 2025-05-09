namespace PokemonPocket.Models
{
    public class Eevee : Pokemon
    {
        public Eevee() : base("Eevee", hp: 90, exp: 0, skill: "Run Away", skillDamage: 25) { }

        public Eevee(int hp, int exp) : base("Eevee", hp, exp, skill: "Run Away", skillDamage: 25) { }

        protected override int GetDamageMultiplier() => 2;
    }
}
