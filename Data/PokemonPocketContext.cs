namespace PokemonPocket.Data;
using Microsoft.EntityFrameworkCore;

public enum PokemonType
{
    Pikachu,
    Charmander,
    Eevee,
    Raichu,
    Flareon,
    Charmeleon,
    Bulbasaur,
    Ivysaur,
    Charmachu,
    Charaichu,
    Eeveechu,
    Eeveeon,
    Pikasaur,
    Flareeveechu,
    Charvysaur,
    Charvysaurion
}

public class PokemonPocketContext : DbContext
{
    public DbSet<PokemonMaster> EvolutionRules { get; set; } = null!;
    public DbSet<Pokemon> Pokemon { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Badge> Badges { get; set; } = null!;
    public DbSet<GymLeader> GymLeaders { get; set; } = null!;
    public DbSet<SplicingRule> SplicingRules { get; set; } = null!;

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
            .HasValue<Eevee>(PokemonType.Eevee)
            .HasValue<Raichu>(PokemonType.Raichu)
            .HasValue<Flareon>(PokemonType.Flareon)
            .HasValue<Charmeleon>(PokemonType.Charmeleon)
            .HasValue<Bulbasaur>(PokemonType.Bulbasaur)
            .HasValue<Ivysaur>(PokemonType.Ivysaur)
            .HasValue<Eeveechu>(PokemonType.Eeveechu)
            .HasValue<Eeveeon>(PokemonType.Eeveeon)
            .HasValue<Pikasaur>(PokemonType.Pikasaur)
            .HasValue<Flareeveechu>(PokemonType.Flareeveechu)
            .HasValue<Charvysaur>(PokemonType.Charvysaur)
            .HasValue<Charaichu>(PokemonType.Charaichu)
            .HasValue<Charmachu>(PokemonType.Charmachu)
            .HasValue<Charvysaurion>(PokemonType.Charvysaurion);

        modelBuilder.Entity<Player>()
            .HasMany(p => p.Badges)
            .WithOne(b => b.Player)
            .HasForeignKey(b => b.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GymLeader>()
            .HasMany(gl => gl.PokemonTeam)
            .WithOne()
            .HasForeignKey("GymLeaderId")
            .IsRequired(false)  // This allows Pokemon not belonging to a GymLeader to have a null GymLeaderId.
            .OnDelete(DeleteBehavior.Cascade);
    }
}
