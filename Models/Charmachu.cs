namespace PokemonPocket.Models
{
    public class Charmachu : Pokemon
    {

        public Charmachu() : base("Charmachu", hp: 180, exp: 0, skill: "Electric Claw", skillDamage: 50) { }

        public Charmachu(int hp, int exp, int level, int skillDamage) : base("Charmachu", hp, exp, "Electric Claw", skillDamage: 50 + skillDamage)
        {
            this.Level = level;
        }

        protected override int GetDamageMultiplier() => 8;


        // for marking
        void calculateDamage(int damage)
        {
            this.HP -= damage * GetDamageMultiplier();
        }
    }
}
