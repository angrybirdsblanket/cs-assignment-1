namespace PokemonPocket.Models
{

    public class Bulbasaur : Pokemon
    {

        public Bulbasaur() { }

        public Bulbasaur(int hp, int exp) : base("Bulbasaur", hp, exp, "Verdant Spiral", 15) { }


        protected override int GetDamageMultiplier() => 5;
    }

}
