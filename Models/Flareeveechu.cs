//flareeveechu   => 1 flareon + 1 eevee + 1 pikachu,      skill: Elemental Charge,  damage: 75,  damage multiplier: 12

namespace PokemonPocket.Models
{
    public class Flareeveechu : Pokemon
    {

        public Flareeveechu() : base("Flareeeveechu", hp: 150, exp: 0, skill: "Elemental Charge", skillDamage: 75) { }

        public Flareeveechu(int hp, int exp, int level, int skillDamage) : base("Flareeeveechu", hp, exp, "Elemental Charge", skillDamage: 75 + skillDamage)
        {
            this.Level = level;
        }

        protected override int GetDamageMultiplier() => 12;
        // for marking
        void calculateDamage(int damage)
        {
            this.HP -= damage * GetDamageMultiplier();
        }
    }

}
