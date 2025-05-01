namespace PokemonPocket.Models
{
    public class Pikachu : Pokemon
    {
        public Pikachu() : base("Pikachu", hp: 200, exp: 0, skill: "Lightning Bolt", skillDamage: 30) { }

        public Pikachu(int hp, int exp) : base("Pikachu", hp, exp, skill: "Lightning Bolt", skillDamage: 30) { }

        protected override int GetDamageMultiplier() => 3;
    }
}
