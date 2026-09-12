using Backend.Domain.Cadastros;
using Backend.Domain.Integracoes;
using Backend.Domain.Tickets;
using Backend.Infra.Postgres.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend.UnitTests.Persistence;

public sealed class EnumCommentsTests
{
    [Fact]
    public void EnumPersistidoPossuiValoresDescricoesEComentario()
    {
        using BackendDbContext context = CreateContext();
        IModel model = context.GetService<IDesignTimeModel>().Model;
        AssertProperty<Cadastro>(
            model,
            nameof(Cadastro.Tipo),
            "1 = Fazenda — Fazenda; 2 = AnoAgricola — Ano agrícola; 3 = Safra — Safra; 4 = Cultura — Cultura");
        AssertProperty<ChecklistItem>(
            model,
            nameof(ChecklistItem.Resultado),
            "1 = Conforme — Conforme; 2 = NaoConforme — Não conforme");
        AssertProperty<Integracao>(
            model,
            nameof(Integracao.Status),
            "1 = Pendente — Pendente; 2 = Processando — Processando; 3 = Concluida — Concluída; 4 = FalhaDefinitiva — Falha definitiva");
    }

    private static BackendDbContext CreateContext()
    {
        DbContextOptions<BackendDbContext> options = new DbContextOptionsBuilder<BackendDbContext>()
            .UseNpgsql("Host=localhost;Database=backend_enum_comments")
            .Options;
        return new(options);
    }

    private static void AssertProperty<TEntity>(IModel model, string propertyName, string expectedComment)
    {
        IProperty property = model.FindEntityType(typeof(TEntity))!.FindProperty(propertyName)!;
        Assert.Equal(expectedComment, property.GetComment());
        Assert.Equal("integer", property.GetRelationalTypeMapping().StoreType);
    }
}
