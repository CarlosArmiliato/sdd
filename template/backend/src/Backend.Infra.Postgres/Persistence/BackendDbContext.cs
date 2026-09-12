using Backend.Domain.Cadastros;
using Backend.Domain.Integracoes;
using Backend.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infra.Postgres.Persistence;

public sealed class BackendDbContext(DbContextOptions<BackendDbContext> options) : DbContext(options)
{
    public DbSet<Cadastro> Cadastros => Set<Cadastro>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Integracao> Integracoes => Set<Integracao>();
    public DbSet<EventoRecebido> EventosRecebidos => Set<EventoRecebido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cadastro>(entity =>
        {
            entity.ToTable("Cadastros");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tipo).HasConversion<string>();
            entity.Property(x => x.Codigo).HasMaxLength(80);
            entity.Property(x => x.Nome).HasMaxLength(200);
            entity.Property(x => x.CodigoExterno).HasMaxLength(80);
            entity.HasIndex(x => new { x.Tipo, x.Codigo }).IsUnique();
            entity.HasIndex(x => new { x.Tipo, x.CodigoExterno }).IsUnique();
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NumeroExterno).HasMaxLength(100);
            entity.Property(x => x.CorrelationId).HasMaxLength(160);
            entity.HasIndex(x => x.NumeroExterno).IsUnique();
            entity.HasMany(x => x.Checklist)
                .WithOne()
                .HasForeignKey("TicketId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChecklistItem>(entity =>
        {
            entity.ToTable("ChecklistItens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(80);
            entity.Property(x => x.Resultado).HasConversion<string>();
        });

        modelBuilder.Entity<Integracao>(entity =>
        {
            entity.ToTable("Integracoes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>();
            entity.Property(x => x.Payload).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.Status, x.DisponivelEm });
        });

        modelBuilder.Entity<EventoRecebido>(entity =>
        {
            entity.ToTable("EventosRecebidos");
            entity.HasKey(x => x.EventId);
            entity.Property(x => x.EventId).HasMaxLength(160);
        });
    }
}

public sealed class EventoRecebido
{
    private EventoRecebido() { }
    public EventoRecebido(string eventId, Guid ticketId)
    {
        EventId = eventId;
        TicketId = ticketId;
        RecebidoEm = DateTimeOffset.UtcNow;
    }

    public string EventId { get; private set; } = string.Empty;
    public Guid TicketId { get; private set; }
    public DateTimeOffset RecebidoEm { get; private set; }
}
