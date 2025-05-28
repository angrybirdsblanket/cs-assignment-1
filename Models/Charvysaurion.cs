//Ivan Dochev 241836X
namespace PokemonPocket.Models
{

    public class Charvysaurion : Pokemon
    {

        public Charvysaurion() : base("Charvysaurion", hp: 150, exp: 0, skill: "Tri-Force Slash", skillDamage: 60) { }

        public Charvysaurion(int hp, int exp, int level, int skillDamage) : base("Charvysaurion", hp, exp, "Tri-Force Slash", skillDamage: 60 + skillDamage)
        {
            this.Level = level;
        }

        protected override int GetDamageMultiplier() => 10;

        // for marking
        void calculateDamage(int damage)
        {
            this.HP -= damage * GetDamageMultiplier();
        }
    }

}
