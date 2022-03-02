﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ryba.Data;

#nullable disable

namespace Ryba.Data.Migrations
{
    [DbContext(typeof(RybaContext))]
    partial class RybaContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Ryba.Data.PortablePin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Channel")
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .HasColumnType("text");

                    b.Property<string>("RybaUserId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RybaUserId");

                    b.ToTable("PortablePin");
                });

            modelBuilder.Entity("Ryba.Data.RybaUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Language")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Ryba.Data.PortablePin", b =>
                {
                    b.HasOne("Ryba.Data.RybaUser", null)
                        .WithMany("PortablePins")
                        .HasForeignKey("RybaUserId");
                });

            modelBuilder.Entity("Ryba.Data.RybaUser", b =>
                {
                    b.Navigation("PortablePins");
                });
#pragma warning restore 612, 618
        }
    }
}
