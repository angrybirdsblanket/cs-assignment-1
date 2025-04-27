namespace PokemonPocket.Models
{
    public class Pikachu : Pokemon
    {
        public Pikachu() : base("Pikachu", hp: 100, exp: 0, skill: "Lightning Bolt", skillDamage: 30) { }

        protected override int GetDamageMultiplier() => 3;
    }
}
