using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockLogger.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HighestPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LowestPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpenTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBullish = table.Column<bool>(type: "bit", nullable: false),
                    IsBearish = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockPricePerSec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    StockDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StockPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockPricePerSec", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTickerExchanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTickerExchanges", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candel");

            migrationBuilder.DropTable(
                name: "StockPricePerSec");

            migrationBuilder.DropTable(
                name: "StockTickerExchanges");
        }
    }
}
