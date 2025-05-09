namespace PokemonPocket.Models
{

    public class Ivysaur : Pokemon
    {

        public Ivysaur() { }

        public Ivysaur(int hp, int exp) : base("Ivysaur", hp, exp, "Verdant Spiral", 15) { }

        protected override int GetDamageMultiplier() => 6;
    }

}

