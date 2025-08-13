using BladesCalc.Models;
using Microsoft.EntityFrameworkCore;

namespace BladesCalc.Data;

public class AerodynamicsDataBladesContext : DbContext
{
    public AerodynamicsDataBladesContext(DbContextOptions<AerodynamicsDataBladesContext> options)
        : base(options)
    {
    }

    public DbSet<AerodynamicsDataBlades> AerodynamicsDataBlades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурация для AerodynamicsDataBlades
        modelBuilder.Entity<AerodynamicsDataBlades>(entity =>
        {
            // Установка первичного ключа
            entity.HasKey(e => e.Id)
                .HasName("PK_AerodynamicsDataBlades");

            // Настройка Id
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();

            // Настройка TypeOfBlades
            entity.Property(e => e.TypeOfBlades)
                .HasColumnName("TypeOfBlades")
                .HasMaxLength(100)
                .IsUnicode(false);

            // Настройка TypeOfBladesKod (предполагается, что это enum)
            entity.Property(e => e.TypeOfBladesKod)
                .HasColumnName("TypeOfBladesKod")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsUnicode(false);

            // Настройка Scheme
            entity.Property(e => e.Scheme)
                .HasColumnName("Scheme")
                .HasMaxLength(100)
                .IsUnicode(false);

            // Настройка числовых полей
            entity.Property(e => e.StaticPressure1)
                .HasColumnName("StaticPressure1")
                .HasColumnType("float");

            entity.Property(e => e.StaticPressure2)
                .HasColumnName("StaticPressure2")
                .HasColumnType("float");

            entity.Property(e => e.StaticPressure3)
                .HasColumnName("StaticPressure3")
                .HasColumnType("float");

            entity.Property(e => e.Efficiency1)
                .HasColumnName("Efficiency1")
                .HasColumnType("float");

            entity.Property(e => e.Efficiency2)
                .HasColumnName("Efficiency2")
                .HasColumnType("float");

            entity.Property(e => e.Efficiency3)
                .HasColumnName("Efficiency3")
                .HasColumnType("float");

            entity.Property(e => e.Efficiency4)
                .HasColumnName("Efficiency4")
                .HasColumnType("float");

            entity.Property(e => e.OutletLength)
                .HasColumnName("OutletLength")
                .HasColumnType("float");

            entity.Property(e => e.OutletWidth)
                .HasColumnName("OutletWidth")
                .HasColumnType("float");

            entity.Property(e => e.MinDeltaEfficiency)
                .HasColumnName("MinDeltaEfficiency")
                .HasColumnType("float");

            entity.Property(e => e.MaxDeltaEfficiency)
                .HasColumnName("MaxDeltaEfficiency")
                .HasColumnType("float");

            // Настройка строковых полей
            entity.Property(e => e.NewMarkOfFan)
                .HasColumnName("NewMarkOfFan")
                .HasMaxLength(100)
                .IsUnicode(false);

            // Настройка числовых полей (продолжение)
            entity.Property(e => e.BladeLength)
                .HasColumnName("BladeLength")
                .HasColumnType("float");

            entity.Property(e => e.BladeWidth)
                .HasColumnName("BladeWidth")
                .HasColumnType("float");

            entity.Property(e => e.ImpellerWidth)
                .HasColumnName("ImpellerWidth")
                .HasColumnType("float");

            entity.Property(e => e.ImpellerInletDiameter)
                .HasColumnName("ImpellerInletDiameter")
                .HasColumnType("float");

            entity.Property(e => e.NumberOfBlades)
                .HasColumnName("NumberOfBlades")
                .HasColumnType("int");

            entity.Property(e => e.NewMarkOfFand)
                .HasColumnName("NewMarkOfFand")
                .HasMaxLength(100)
                .IsUnicode(false);

            // Индексы при необходимости
            entity.HasIndex(e => e.TypeOfBlades, "IX_AerodynamicsDataBlades_TypeOfBlades");
            entity.HasIndex(e => e.TypeOfBladesKod, "IX_AerodynamicsDataBlades_TypeOfBladesKod");
        });
    }
}