using static System.Math;
using System.ComponentModel.DataAnnotations;
namespace PokemonPocket.Models

{
    // Base class for all Pokémon
    public abstract class Pokemon
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int HP { get; set; }
        public int Exp { get; set; }
        public string Skill { get; set; }
        public int SkillDamage { get; set; }

        // empty constructor for EF Core
        public Pokemon() { }

        public Pokemon(string name, int hp, int exp, string skill, int skillDamage)
        {
            Name = name;
            HP = hp;
            Exp = exp;
            Skill = skill;
            SkillDamage = skillDamage;
        }

        public void Attack (Pokemon defender) {
          defender.HP = Max(0, defender.HP - this.SkillDamage * this.GetDamageMultiplier());
        }

        protected abstract int GetDamageMultiplier();
    }

}
