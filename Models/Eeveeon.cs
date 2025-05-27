//eeveeon        => 1 eevee + 3 flareon,                  skill: Flare Storm,       damage: 65,  damage multiplier: 5

namespace PokemonPocket.Models
{
    public class Eeveeon : Pokemon
    {
        public Eeveeon() : base("Eeveeon", hp: 300, exp: 0, skill: "Flare Storm", skillDamage: 65) { }

        public Eeveeon(int hp, int exp, int level, int skillDamage)
            : base("Eeveeon", hp, exp, "Flare Storm", skillDamage: 65 + skillDamage)
        {
            this.Level = level;
        }

        protected override int GetDamageMultiplier() => 5;

        // for marking
        void calculateDamage(int damage)
        {
            this.HP -= damage * GetDamageMultiplier();
        }
    }
}
