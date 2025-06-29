﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace SpeedCalc.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SpeedCalc.Models.AerodynamicsData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<double>("BladeLength")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("blade_length");

                    b.Property<double>("BladeWidth")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("average_blade_width");

                    b.Property<double>("Efficiency1")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("b1");

                    b.Property<double>("Efficiency2")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("b2");

                    b.Property<double>("Efficiency3")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("b3");

                    b.Property<double>("Efficiency4")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("b4");

                    b.Property<double>("ImpellerInletDiameter")
                        .HasColumnType("numeric(18,3)")
                        .HasColumnName("impeller_inlet_diameter");

                    b.Property<double>("ImpellerWidth")
                        .HasColumnType("numeric(18,3)")
                        .HasColumnName("impeller_width");

                    b.Property<double>("MaxDeltaEfficiency")
                        .HasColumnType("numeric(18,4)")
                        .HasColumnName("max_delta");

                    b.Property<double>("MaxSpeed")
                        .HasColumnType("numeric(18,2)")
                        .HasColumnName("max_speed");

                    b.Property<double>("MinDeltaEfficiency")
                        .HasColumnType("numeric(18,4)")
                        .HasColumnName("min_delta");

                    b.Property<double>("MinSpeed")
                        .HasColumnType("numeric(18,2)")
                        .HasColumnName("min_speed");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("aerodynamic_scheme");

                    b.Property<string>("NewMarkOfFan")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("new_mark_of_fan");

                    b.Property<string>("NewMarkOfFanD")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("new_mark_of_fan_d");

                    b.Property<int>("NumberOfBlades")
                        .HasColumnType("integer")
                        .HasColumnName("number_of_blades");

                    b.Property<double>("OutletLength")
                        .HasColumnType("numeric(18,3)")
                        .HasColumnName("a");

                    b.Property<double>("OutletWidth")
                        .HasColumnType("numeric(18,3)")
                        .HasColumnName("b");

                    b.Property<double>("StaticPressure1")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("a1");

                    b.Property<double>("StaticPressure2")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("a2");

                    b.Property<double>("StaticPressure3")
                        .HasColumnType("numeric(18,6)")
                        .HasColumnName("a3");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("gas_type");

                    b.Property<string>("TypeOfBlades")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("type_of_blades");

                    b.HasKey("Id");

                    b.ToTable("aerodynamics_data", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
