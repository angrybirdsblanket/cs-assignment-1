namespace PokemonPocket.Models
{

    public class Charmeleon : Pokemon
    {
        public Charmeleon() : base("Charmander", hp: 110, exp: 0, skill: "Solar Power", skillDamage: 10) { }

        protected override int GetDamageMultiplier() => 1;
    }
}
