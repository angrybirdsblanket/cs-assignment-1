using static System.Math;
using System.ComponentModel.DataAnnotations;


namespace PokemonPocket.Models

{
    // Base class for all Pokémon
    public abstract class Pokemon
    {
        private readonly Random _random = new Random();

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int HP { get; set; }
        public int Exp { get; set; }
        public string Skill { get; set; }
        public int SkillDamage { get; set; }
        public int MaxHP { get; set; }
        public int Level { get; set; }

        // empty constructor for EF Core
        public Pokemon()
        {

        }

        public Pokemon(string name, int hp, int exp, string skill, int skillDamage)
        {
            this.Name = name;
            this.HP = hp;
            this.MaxHP = hp;
            this.Exp = exp;
            this.Skill = skill;
            this.SkillDamage = skillDamage;
            this.Level = 1;
        }

        public int Attack(Pokemon defender)
        {
            double multiplier = Max(this._random.NextDouble(), 0.5);
            int damage = Convert.ToInt32(this.SkillDamage * this.GetDamageMultiplier() * multiplier);

            defender.HP = Max(0, defender.HP - damage);
            return damage;
        }

        public void Heal()
        {
            this.HP = this.MaxHP;
        }

        public void LevelUp()
        {
            while (this.Exp >= 100)
            {
                this.Exp -= 100;
                this.Level += 1;
                this.MaxHP += 50;
                this.HP = this.MaxHP;
                this.SkillDamage += 5;
            }
        }


        public int calculateExp(Pokemon enemy, int damageDealt)
        {
            const int maxExp = 50;
            int expGained;
            double levelRatio = (double)enemy.Level / (double)this.Level;

            if (damageDealt >= enemy.MaxHP)
            {
                expGained = (int)(maxExp * levelRatio);
            }
            else
            {
                double damageRatio = (double)damageDealt / enemy.MaxHP;
                expGained = (int)Math.Floor(damageRatio * maxExp);
                if (damageDealt > 0 && expGained < 1)
                {
                    expGained = 1;
                }
            }

            return expGained;
        }

        protected abstract int GetDamageMultiplier();
    }

}
