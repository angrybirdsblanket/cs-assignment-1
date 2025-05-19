namespace PokemonPocket.Models
{

    public class Charmander : Pokemon
    {
        public Charmander() : base("Charmander", hp: 110, exp: 0, skill: "Solar Power", skillDamage: 10) { }

        public Charmander(int hp, int exp) : base("Charmander", hp, exp, skill: "Solar Power", skillDamage: 10) { }

        protected override int GetDamageMultiplier() => 1;

        // for marking
        void calculateDamage(int damage) {
          this.HP -=.HP -= damage * GetDamageMultiplier();
        }
    }
}
