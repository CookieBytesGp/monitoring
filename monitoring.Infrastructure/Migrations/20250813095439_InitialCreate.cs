using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monitoring.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cameras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location_Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Location_Zone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location_Latitude = table.Column<double>(type: "float(10)", precision: 10, scale: 8, nullable: true),
                    Location_Longitude = table.Column<double>(type: "float(11)", precision: 11, scale: 8, nullable: true),
                    Network_IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Network_Port = table.Column<int>(type: "int", nullable: false),
                    Network_Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Network_Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Network_Type = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Connection_StreamUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Connection_SnapshotUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Connection_BackupStreamUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Connection_IsConnected = table.Column<bool>(type: "bit", nullable: false),
                    Connection_ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Connection_LastHeartbeat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Connection_Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Connection_AdditionalInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastActiveAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cameras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CameraCapabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CameraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CameraId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraCapabilities_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CameraCapabilities_Cameras_CameraId1",
                        column: x => x.CameraId1,
                        principalTable: "Cameras",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CameraConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FrameRate = table.Column<int>(type: "int", nullable: false),
                    VideoCodec = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Bitrate = table.Column<int>(type: "int", nullable: false),
                    AudioEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AudioCodec = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdditionalSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotionDetection_IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MotionDetection_Sensitivity = table.Column<int>(type: "int", nullable: false),
                    MotionDetection_Zone = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Recording_IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Recording_Quality = table.Column<int>(type: "int", nullable: false),
                    Recording_Duration = table.Column<double>(type: "float", nullable: false),
                    Recording_StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CameraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CameraId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraConfigurations_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CameraConfigurations_Cameras_CameraId1",
                        column: x => x.CameraId1,
                        principalTable: "Cameras",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CameraStreams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quality = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CameraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CameraId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraStreams_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CameraStreams_Cameras_CameraId1",
                        column: x => x.CameraId1,
                        principalTable: "Cameras",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CameraCapabilities_CameraId",
                table: "CameraCapabilities",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraCapabilities_CameraId1",
                table: "CameraCapabilities",
                column: "CameraId1");

            migrationBuilder.CreateIndex(
                name: "IX_CameraCapabilities_Type",
                table: "CameraCapabilities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CameraConfigurations_CameraId",
                table: "CameraConfigurations",
                column: "CameraId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CameraConfigurations_CameraId1",
                table: "CameraConfigurations",
                column: "CameraId1",
                unique: true,
                filter: "[CameraId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CameraConfigurations_Resolution",
                table: "CameraConfigurations",
                column: "Resolution");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_Name",
                table: "Cameras",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_Status",
                table: "Cameras",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_Type",
                table: "Cameras",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CameraStreams_CameraId",
                table: "CameraStreams",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraStreams_CameraId1",
                table: "CameraStreams",
                column: "CameraId1");

            migrationBuilder.CreateIndex(
                name: "IX_CameraStreams_Quality",
                table: "CameraStreams",
                column: "Quality");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CameraCapabilities");

            migrationBuilder.DropTable(
                name: "CameraConfigurations");

            migrationBuilder.DropTable(
                name: "CameraStreams");

            migrationBuilder.DropTable(
                name: "Cameras");
        }
    }
}
