using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ronadash_data.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Counties",
                columns: table => new
                {
                    CountyId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FIPS = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Province_State = table.Column<string>(nullable: true),
                    Country_Region = table.Column<string>(nullable: true),
                    Last_Update = table.Column<DateTime>(nullable: false),
                    Lat = table.Column<string>(nullable: true),
                    Long = table.Column<string>(nullable: true),
                    Confirmed = table.Column<int>(nullable: false),
                    Deaths = table.Column<int>(nullable: false),
                    Recovered = table.Column<int>(nullable: false),
                    Active = table.Column<int>(nullable: false),
                    Combined_Key = table.Column<string>(nullable: true),
                    Incidence_Rate = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counties", x => x.CountyId);
                });

            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FIPS = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Province_State = table.Column<string>(nullable: true),
                    County = table.Column<string>(nullable: true),
                    Last_Update = table.Column<DateTime>(nullable: false),
                    Lat = table.Column<double>(nullable: true),
                    Long = table.Column<double>(nullable: true),
                    Confirmed = table.Column<int>(nullable: true),
                    Deaths = table.Column<int>(nullable: true),
                    Recovered = table.Column<int>(nullable: true),
                    Active = table.Column<int>(nullable: true),
                    Combined_Key = table.Column<string>(nullable: true),
                    Incidence_Rate = table.Column<double>(nullable: true),
                    Case_Fatality_Ratio = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Province_State = table.Column<string>(nullable: true),
                    Active = table.Column<int>(nullable: false),
                    Deaths = table.Column<int>(nullable: false),
                    Confirmed = table.Column<int>(nullable: false),
                    Recovered = table.Column<int>(nullable: false),
                    Last_Update = table.Column<DateTime>(nullable: false),
                    Counties = table.Column<List<int>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Records_Country",
                table: "Records",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Records_County",
                table: "Records",
                column: "County");

            migrationBuilder.CreateIndex(
                name: "IX_Records_Last_Update",
                table: "Records",
                column: "Last_Update");

            migrationBuilder.CreateIndex(
                name: "IX_Records_Province_State",
                table: "Records",
                column: "Province_State");

            migrationBuilder.CreateIndex(
                name: "IX_Records_Country_Province_State_County_Last_Update",
                table: "Records",
                columns: new[] { "Country", "Province_State", "County", "Last_Update" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counties");

            migrationBuilder.DropTable(
                name: "Records");

            migrationBuilder.DropTable(
                name: "States");
        }
    }
}
