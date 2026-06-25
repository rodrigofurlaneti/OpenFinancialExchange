using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    // Timestamp fixo para o seed (HasData exige valores determinísticos).
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.UserId).HasColumnName("UserId");  // nullable = categoria de sistema
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Kind).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(7).IsRequired();
        builder.Property(x => x.IsSystem).IsRequired();
        builder.Property(x => x.IsInternal).IsRequired();
        builder.Property(x => x.Keywords).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2").IsRequired();

        // Nome único por usuário; categorias de sistema (UserId null) são únicas entre si.
        builder.HasIndex(x => new { x.UserId, x.Name })
            .IsUnique()
            .HasDatabaseName("UX_Categories_User_Name");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("FK_Categories_Users")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedCategories());
    }

    private static IEnumerable<object> SeedCategories()
    {
        // (Id, Nome, Kind, Cor, Interna, PalavrasChave). Palavras separadas por '\n'.
        // O matcher normaliza (maiúsculas, sem acento) e a maior palavra contida vence.
        var defs = new (long Id, string Name, string Kind, string Color, bool IsInternal, string Keywords)[]
        {
            (1,  "Alimentação",        "DEBIT",  "#ef4444", false, "PANIFICADORA\nPADARIA\nRESTAURANTE\nSUPERMERCADO\nMERCADO\nHORTIFRUTI\nACOUGUE\nLANCHONETE\nIFOOD\nRAPPI"),
            (2,  "Transporte",         "DEBIT",  "#f97316", false, "POSTO\nCOMBUSTIVEL\nGASOLINA\nESTACIONAMENTO\nUBER\n99 TECNOLOGIA\n99POP\nPEDAGIO\nONIBUS\nMETRO"),
            (3,  "Moradia",            "DEBIT",  "#eab308", false, "ALUGUEL\nCONDOMINIO\nENERGIA\nENEL\nCEMIG\nSABESP\nAGUA\nINTERNET\nVIVO FIBRA\nCLARO NET"),
            (4,  "Saúde",              "DEBIT",  "#14b8a6", false, "FARMACIA\nDROGARIA\nDROGASIL\nRAIA\nPACHECO\nHOSPITAL\nCLINICA\nLABORATORIO\nUNIMED\nDENTISTA"),
            (5,  "Educação",           "DEBIT",  "#3b82f6", false, "ESCOLA\nCOLEGIO\nFACULDADE\nUNIVERSIDADE\nCURSO\nUDEMY\nALURA\nLIVRARIA"),
            (6,  "Lazer",              "DEBIT",  "#a855f7", false, "CINEMA\nNETFLIX\nSPOTIFY\nDISNEY\nHBO\nAMAZON PRIME\nINGRESSO\nTEATRO\nSTEAM"),
            (7,  "Compras",            "DEBIT",  "#ec4899", false, "MAGAZINE\nMAGALU\nAMERICANAS\nMERCADO LIVRE\nMERCADOLIVRE\nAMAZON\nSHOPEE\nALIEXPRESS\nSHOPPING\nRENNER\nRIACHUELO"),
            (8,  "Serviços",           "DEBIT",  "#64748b", false, "BARBEARIA\nSALAO\nLAVANDERIA\nASSINATURA"),
            (9,  "Salário",            "CREDIT", "#22c55e", false, "SALARIO\nFOLHA PGTO\nFOLHA DE PAGAMENTO\nPROVENTOS\nREMUNERACAO\nORDENADO"),
            (10, "Investimentos",      "BOTH",   "#10b981", true,  "RESGATE\nAPLICACAO\nAPLICACAO INV\nINV FAC\nINVEST FACIL\nINVESTIMENTO FACIL\nCDB\nTESOURO DIRETO\nFUNDO"),
            (11, "Transferências",     "BOTH",   "#06b6d4", true,  ""),
            (12, "Tarifas Bancárias",  "DEBIT",  "#f43f5e", false, "TARIFA\nCESTA\nANUIDADE\nIOF\nPACOTE SERVICOS\nMANUTENCAO CONTA"),
            (13, "Outros",             "BOTH",   "#94a3b8", false, ""),
        };

        foreach (var d in defs)
            yield return new
            {
                d.Id,
                UserId = (long?)null,   // categoria de sistema, compartilhada por todos
                d.Name,
                d.Kind,
                d.Color,
                IsSystem = true,
                d.IsInternal,
                d.Keywords,
                IsActive = true,
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate,
            };
    }
}
