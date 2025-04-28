using System;
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

        public void CalculateDamage(int incomingSkillDamage)
        {
            int totalDamage = incomingSkillDamage * GetDamageMultiplier();
            HP -= System.Math.Max(HP - totalDamage, 0);
            Console.WriteLine($"{Name} took {totalDamage} damage and now has {HP} HP.");
        }

        protected abstract int GetDamageMultiplier();
    }

}
