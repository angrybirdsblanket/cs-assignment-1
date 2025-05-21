namespace PokemonPocket.Models
{

    public class Ivysaur : Pokemon
    {

        public Ivysaur() : base("Ivysaur", 100, 0, "Verdant Spiral", 15) { }

        public Ivysaur(int hp, int exp) : base("Ivysaur", hp, exp, "Verdant Spiral", 15) { }

        protected override int GetDamageMultiplier() => 6;

        // for marking
        void calculateDamage(int damage) {
          this.HP -= damage * GetDamageMultiplier();
        }
    }

}

