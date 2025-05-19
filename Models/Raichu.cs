namespace PokemonPocket.Models
{

    public class Raichu : Pokemon
    {

        public Raichu() : base("Raichu", hp: 100, exp: 0, skill: "Lightning Bolt", skillDamage: 30)
        {

        }

        protected override int GetDamageMultiplier() => 4;

        //for marking
        void calculateDamage(int damage) {
          this.HP -= damage * GetDamageMultiplier();
        }

    }

}
