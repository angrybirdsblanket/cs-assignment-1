namespace PokemonPocket.Data;
using Microsoft.EntityFrameworkCore;
using PokemonPocket.Models;

public enum PokemonType
{
    Pikachu,
    Charmander,
    Eevee
}

public class PokemonPocketContext : DbContext
{
    public DbSet<PokemonMaster> EvolutionRules { get; set; } = null!;
    public DbSet<Pokemon> Pokemon { get; set; } = null!;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=pokemon.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PokemonMaster>()
          .ToTable("EvolutionRules");

        modelBuilder.Entity<Pokemon>()
          .ToTable("Pokemon")
          .HasDiscriminator<PokemonType>("PokemonType")
          .HasValue<Pikachu>(PokemonType.Pikachu)
          .HasValue<Charmander>(PokemonType.Charmander)
          .HasValue<Eevee>(PokemonType.Eevee);
    }

}
