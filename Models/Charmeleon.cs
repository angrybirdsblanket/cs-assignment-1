//Ivan Dochev 241836X
namespace PokemonPocket.Models
{

    public class Charmeleon : Pokemon
    {
        public Charmeleon() : base("Charmander", hp: 110, exp: 0, skill: "Solar Power", skillDamage: 10) { }

        protected override int GetDamageMultiplier() => 2;

        // for marking
        void calculateDamage(int damage)
        {
            this.HP -= damage * GetDamageMultiplier();
        }
    }
}
