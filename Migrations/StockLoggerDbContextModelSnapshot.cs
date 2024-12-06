﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StockLogger.Data;

#nullable disable

namespace StockLogger.Migrations
{
    [DbContext(typeof(StockLoggerDbContext))]
    partial class StockLoggerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("StockLogger.Models.Candel.Candel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CloseTime")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("EndPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Exchange")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("HighestPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("IsBearish")
                        .HasColumnType("bit");

                    b.Property<bool>("IsBullish")
                        .HasColumnType("bit");

                    b.Property<decimal>("LowestPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("OpenTime")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("StartPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("TickerId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Candel");
                });

            modelBuilder.Entity("StockLogger.Models.Candel.StockPricePerSec", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("StockDateTime")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("StockPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("TickerId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("StockPricePerSec");
                });

            modelBuilder.Entity("StockLogger.Models.Candel.StockTickerExchange", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Exchange")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("StockTickerExchanges");
                });
#pragma warning restore 612, 618
        }
    }
}
