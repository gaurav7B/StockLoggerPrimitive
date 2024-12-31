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
                name: "AbandonedBabyDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAbandonedBabyDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbandonedBabyDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BeltHoldDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBeltHoldDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeltHoldDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BreakawayDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBreakawayDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakawayDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BullishEngulfingDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBullishEngulfingDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BullishEngulfingDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BullishHaramiDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBullishHaramiDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BullishHaramiDb", x => x.Id);
                });

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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Candel10min",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candel10min", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Candel15min",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candel15min", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Candel5min",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candel5min", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DragonflyDojiDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDragonflyDojiDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DragonflyDojiDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HammerDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHammerDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HammerDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvertedHammerDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInvertedHammerDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvertedHammerDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarubozuDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMarubozuDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarubozuDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MorningStarDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMorningStarDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MorningStarDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PiercingLineDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPiercingLineDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiercingLineDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RisingSunDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRisingSunDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RisingSunDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RisingThreeMethodsDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRisingThreeMethodsDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RisingThreeMethodsDb", x => x.Id);
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
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTickerExchanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThreeWhiteSoilderDbs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsThreeWhiteSoilderDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreeWhiteSoilderDbs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TowerBottomDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsTowerBottomDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TowerBottomDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TweezerBottomDb",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TickerId = table.Column<long>(type: "bigint", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsTweezerBottomDetected = table.Column<bool>(type: "bit", nullable: false),
                    DetectionRange = table.Column<int>(type: "int", nullable: false),
                    DetectionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweezerBottomDb", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbandonedBabyCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AbandonedBabyDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbandonedBabyCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbandonedBabyCandels_AbandonedBabyDb_AbandonedBabyDbId",
                        column: x => x.AbandonedBabyDbId,
                        principalTable: "AbandonedBabyDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BeltHoldCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BeltHoldDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeltHoldCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeltHoldCandels_BeltHoldDb_BeltHoldDbId",
                        column: x => x.BeltHoldDbId,
                        principalTable: "BeltHoldDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BreakawayCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BreakawayDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakawayCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreakawayCandels_BreakawayDb_BreakawayDbId",
                        column: x => x.BreakawayDbId,
                        principalTable: "BreakawayDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BullishEngulfingCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BullishEngulfingDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BullishEngulfingCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BullishEngulfingCandels_BullishEngulfingDb_BullishEngulfingDbId",
                        column: x => x.BullishEngulfingDbId,
                        principalTable: "BullishEngulfingDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BullishHaramiCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BullishHaramiDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BullishHaramiCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BullishHaramiCandels_BullishHaramiDb_BullishHaramiDbId",
                        column: x => x.BullishHaramiDbId,
                        principalTable: "BullishHaramiDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DragonflyDojiCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DragonflyDojiDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DragonflyDojiCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DragonflyDojiCandels_DragonflyDojiDb_DragonflyDojiDbId",
                        column: x => x.DragonflyDojiDbId,
                        principalTable: "DragonflyDojiDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HammerCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HammerDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HammerCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HammerCandels_HammerDb_HammerDbId",
                        column: x => x.HammerDbId,
                        principalTable: "HammerDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InvertedHammerCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvertedHammerDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvertedHammerCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvertedHammerCandels_InvertedHammerDb_InvertedHammerDbId",
                        column: x => x.InvertedHammerDbId,
                        principalTable: "InvertedHammerDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MarubozuCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarubozuDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarubozuCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarubozuCandels_MarubozuDb_MarubozuDbId",
                        column: x => x.MarubozuDbId,
                        principalTable: "MarubozuDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MorningStarCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MorningStarDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MorningStarCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MorningStarCandels_MorningStarDb_MorningStarDbId",
                        column: x => x.MorningStarDbId,
                        principalTable: "MorningStarDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PiercingLineCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PiercingLineDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiercingLineCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PiercingLineCandels_PiercingLineDb_PiercingLineDbId",
                        column: x => x.PiercingLineDbId,
                        principalTable: "PiercingLineDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RisingSunCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RisingSunDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RisingSunCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RisingSunCandels_RisingSunDb_RisingSunDbId",
                        column: x => x.RisingSunDbId,
                        principalTable: "RisingSunDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RisingThreeMethodsCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RisingThreeMethodsDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RisingThreeMethodsCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RisingThreeMethodsCandels_RisingThreeMethodsDb_RisingThreeMethodsDbId",
                        column: x => x.RisingThreeMethodsDbId,
                        principalTable: "RisingThreeMethodsDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThreeWhiteSoilderCandelss",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThreeWhiteSoilderDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreeWhiteSoilderCandelss", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreeWhiteSoilderCandelss_ThreeWhiteSoilderDbs_ThreeWhiteSoilderDbId",
                        column: x => x.ThreeWhiteSoilderDbId,
                        principalTable: "ThreeWhiteSoilderDbs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TowerBottomCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TowerBottomDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TowerBottomCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TowerBottomCandels_TowerBottomDb_TowerBottomDbId",
                        column: x => x.TowerBottomDbId,
                        principalTable: "TowerBottomDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TweezerBottomCandels",
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
                    IsBullish = table.Column<bool>(type: "bit", nullable: true),
                    IsBearish = table.Column<bool>(type: "bit", nullable: true),
                    PriceChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChangePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TweezerBottomDbId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweezerBottomCandels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TweezerBottomCandels_TweezerBottomDb_TweezerBottomDbId",
                        column: x => x.TweezerBottomDbId,
                        principalTable: "TweezerBottomDb",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbandonedBabyCandels_AbandonedBabyDbId",
                table: "AbandonedBabyCandels",
                column: "AbandonedBabyDbId");

            migrationBuilder.CreateIndex(
                name: "IX_BeltHoldCandels_BeltHoldDbId",
                table: "BeltHoldCandels",
                column: "BeltHoldDbId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakawayCandels_BreakawayDbId",
                table: "BreakawayCandels",
                column: "BreakawayDbId");

            migrationBuilder.CreateIndex(
                name: "IX_BullishEngulfingCandels_BullishEngulfingDbId",
                table: "BullishEngulfingCandels",
                column: "BullishEngulfingDbId");

            migrationBuilder.CreateIndex(
                name: "IX_BullishHaramiCandels_BullishHaramiDbId",
                table: "BullishHaramiCandels",
                column: "BullishHaramiDbId");

            migrationBuilder.CreateIndex(
                name: "IX_DragonflyDojiCandels_DragonflyDojiDbId",
                table: "DragonflyDojiCandels",
                column: "DragonflyDojiDbId");

            migrationBuilder.CreateIndex(
                name: "IX_HammerCandels_HammerDbId",
                table: "HammerCandels",
                column: "HammerDbId");

            migrationBuilder.CreateIndex(
                name: "IX_InvertedHammerCandels_InvertedHammerDbId",
                table: "InvertedHammerCandels",
                column: "InvertedHammerDbId");

            migrationBuilder.CreateIndex(
                name: "IX_MarubozuCandels_MarubozuDbId",
                table: "MarubozuCandels",
                column: "MarubozuDbId");

            migrationBuilder.CreateIndex(
                name: "IX_MorningStarCandels_MorningStarDbId",
                table: "MorningStarCandels",
                column: "MorningStarDbId");

            migrationBuilder.CreateIndex(
                name: "IX_PiercingLineCandels_PiercingLineDbId",
                table: "PiercingLineCandels",
                column: "PiercingLineDbId");

            migrationBuilder.CreateIndex(
                name: "IX_RisingSunCandels_RisingSunDbId",
                table: "RisingSunCandels",
                column: "RisingSunDbId");

            migrationBuilder.CreateIndex(
                name: "IX_RisingThreeMethodsCandels_RisingThreeMethodsDbId",
                table: "RisingThreeMethodsCandels",
                column: "RisingThreeMethodsDbId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreeWhiteSoilderCandelss_ThreeWhiteSoilderDbId",
                table: "ThreeWhiteSoilderCandelss",
                column: "ThreeWhiteSoilderDbId");

            migrationBuilder.CreateIndex(
                name: "IX_TowerBottomCandels_TowerBottomDbId",
                table: "TowerBottomCandels",
                column: "TowerBottomDbId");

            migrationBuilder.CreateIndex(
                name: "IX_TweezerBottomCandels_TweezerBottomDbId",
                table: "TweezerBottomCandels",
                column: "TweezerBottomDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbandonedBabyCandels");

            migrationBuilder.DropTable(
                name: "BeltHoldCandels");

            migrationBuilder.DropTable(
                name: "BreakawayCandels");

            migrationBuilder.DropTable(
                name: "BullishEngulfingCandels");

            migrationBuilder.DropTable(
                name: "BullishHaramiCandels");

            migrationBuilder.DropTable(
                name: "Candel");

            migrationBuilder.DropTable(
                name: "Candel10min");

            migrationBuilder.DropTable(
                name: "Candel15min");

            migrationBuilder.DropTable(
                name: "Candel5min");

            migrationBuilder.DropTable(
                name: "DragonflyDojiCandels");

            migrationBuilder.DropTable(
                name: "HammerCandels");

            migrationBuilder.DropTable(
                name: "InvertedHammerCandels");

            migrationBuilder.DropTable(
                name: "MarubozuCandels");

            migrationBuilder.DropTable(
                name: "MorningStarCandels");

            migrationBuilder.DropTable(
                name: "PiercingLineCandels");

            migrationBuilder.DropTable(
                name: "RisingSunCandels");

            migrationBuilder.DropTable(
                name: "RisingThreeMethodsCandels");

            migrationBuilder.DropTable(
                name: "StockPricePerSec");

            migrationBuilder.DropTable(
                name: "StockTickerExchanges");

            migrationBuilder.DropTable(
                name: "ThreeWhiteSoilderCandelss");

            migrationBuilder.DropTable(
                name: "TowerBottomCandels");

            migrationBuilder.DropTable(
                name: "TweezerBottomCandels");

            migrationBuilder.DropTable(
                name: "AbandonedBabyDb");

            migrationBuilder.DropTable(
                name: "BeltHoldDb");

            migrationBuilder.DropTable(
                name: "BreakawayDb");

            migrationBuilder.DropTable(
                name: "BullishEngulfingDb");

            migrationBuilder.DropTable(
                name: "BullishHaramiDb");

            migrationBuilder.DropTable(
                name: "DragonflyDojiDb");

            migrationBuilder.DropTable(
                name: "HammerDb");

            migrationBuilder.DropTable(
                name: "InvertedHammerDb");

            migrationBuilder.DropTable(
                name: "MarubozuDb");

            migrationBuilder.DropTable(
                name: "MorningStarDb");

            migrationBuilder.DropTable(
                name: "PiercingLineDb");

            migrationBuilder.DropTable(
                name: "RisingSunDb");

            migrationBuilder.DropTable(
                name: "RisingThreeMethodsDb");

            migrationBuilder.DropTable(
                name: "ThreeWhiteSoilderDbs");

            migrationBuilder.DropTable(
                name: "TowerBottomDb");

            migrationBuilder.DropTable(
                name: "TweezerBottomDb");
        }
    }
}
