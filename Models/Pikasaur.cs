namespace PokemonPocket.Models
{
    public class Pikasaur : Pokemon
    {
        public Pikasaur() : base("Pikasaur", hp: 150, exp: 0, skill: "Thunder Seed", skillDamage: 45) { }

        public Pikasaur(int hp, int exp, int level, int skillDamage) : base("Pikasaur", hp, exp, "Thunder Seed", skillDamage: 45 + skillDamage)
        {
            this.Level = level;
        }

        protected override int GetDamageMultiplier() => 7;
    }
}
