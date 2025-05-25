namespace PokemonPocket.Models
{

    public class Bulbasaur : Pokemon
    {

        public Bulbasaur() { }

        public Bulbasaur(int hp, int exp) : base("Bulbasaur", hp, exp, "Verdant Spiral", 15) { }


        protected override int GetDamageMultiplier() => 5;

        // for marking
        void calculateDamage(int damage)
        {
            this.HP -= damage * GetDamageMultiplier();
        }
    }

}
