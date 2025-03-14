using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedReportTemplates = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ModifiedReportTemplates = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "light"),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cameras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    StreamUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Credentials = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastActive = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cameras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Monitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    DisplayResolution = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentContent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LastPing = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsConnected = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Brightness = table.Column<float>(type: "real", nullable: true),
                    Contrast = table.Column<float>(type: "real", nullable: true),
                    Sharpness = table.Column<float>(type: "real", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    MaintainAspectRatio = table.Column<bool>(type: "bit", nullable: false),
                    RotationDegrees = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IncludeImages = table.Column<bool>(type: "bit", nullable: false),
                    IncludeMotionStats = table.Column<bool>(type: "bit", nullable: false),
                    IncludeImageAnalysis = table.Column<bool>(type: "bit", nullable: false),
                    IncludeProcessingHistory = table.Column<bool>(type: "bit", nullable: false),
                    TimeRangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomDays = table.Column<int>(type: "int", nullable: true),
                    CameraFilter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MinMotionPercentage = table.Column<float>(type: "real", nullable: true),
                    MaxMotionPercentage = table.Column<float>(type: "real", nullable: true),
                    OnlyAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    GroupByCamera = table.Column<bool>(type: "bit", nullable: false),
                    GroupByDate = table.Column<bool>(type: "bit", nullable: false),
                    IsScheduled = table.Column<bool>(type: "bit", nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailRecipients = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MotionEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CameraId = table.Column<int>(type: "int", nullable: false),
                    CameraName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DetectionX = table.Column<int>(type: "int", nullable: false),
                    DetectionY = table.Column<int>(type: "int", nullable: false),
                    DetectionWidth = table.Column<int>(type: "int", nullable: false),
                    DetectionHeight = table.Column<int>(type: "int", nullable: false),
                    Sensitivity = table.Column<decimal>(type: "decimal(4,3)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Acknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProcessingError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LastProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingDurationMs = table.Column<int>(type: "int", nullable: true),
                    ImageSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ImageResolution = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BlurLevel = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    BrightnessLevel = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsFalsePositive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotionEvents_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportExportHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportTemplateId = table.Column<int>(type: "int", nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordsCount = table.Column<int>(type: "int", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    WasScheduled = table.Column<bool>(type: "bit", nullable: false),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailRecipients = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GeneratedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExportedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportExportHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportExportHistory_ReportTemplates_ReportTemplateId",
                        column: x => x.ReportTemplateId,
                        principalTable: "ReportTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageProcessingHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MotionEventId = table.Column<int>(type: "int", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProcessingType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageProcessingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageProcessingHistory_MotionEvents_MotionEventId",
                        column: x => x.MotionEventId,
                        principalTable: "MotionEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CreatedAt",
                table: "AspNetUsers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FirstName_LastName",
                table: "AspNetUsers",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsActive",
                table: "AspNetUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastLoginAt",
                table: "AspNetUsers",
                column: "LastLoginAt");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_IpAddress",
                table: "Cameras",
                column: "IpAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_IsActive",
                table: "Cameras",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_LastActive",
                table: "Cameras",
                column: "LastActive");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_Name_Location",
                table: "Cameras",
                columns: new[] { "Name", "Location" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_IsDefault",
                table: "EmailTemplates",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Name",
                table: "EmailTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageProcessingHistory_MotionEventId",
                table: "ImageProcessingHistory",
                column: "MotionEventId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageProcessingHistory_MotionEventId_ProcessingType",
                table: "ImageProcessingHistory",
                columns: new[] { "MotionEventId", "ProcessingType" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageProcessingHistory_ProcessedAt",
                table: "ImageProcessingHistory",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Monitors_IpAddress",
                table: "Monitors",
                column: "IpAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Monitors_IsActive",
                table: "Monitors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Monitors_LastPing",
                table: "Monitors",
                column: "LastPing");

            migrationBuilder.CreateIndex(
                name: "IX_MotionEvents_Acknowledged",
                table: "MotionEvents",
                column: "Acknowledged");

            migrationBuilder.CreateIndex(
                name: "IX_MotionEvents_Acknowledged_Timestamp",
                table: "MotionEvents",
                columns: new[] { "Acknowledged", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_MotionEvents_CameraId",
                table: "MotionEvents",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_MotionEvents_CameraId_Timestamp",
                table: "MotionEvents",
                columns: new[] { "CameraId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_MotionEvents_Timestamp",
                table: "MotionEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExportHistory_ExportedAt",
                table: "ReportExportHistory",
                column: "ExportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExportHistory_ReportTemplateId_ExportedAt",
                table: "ReportExportHistory",
                columns: new[] { "ReportTemplateId", "ExportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExportHistory_Status",
                table: "ReportExportHistory",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_CreatedAt",
                table: "ReportTemplates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_IsScheduled",
                table: "ReportTemplates",
                column: "IsScheduled");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_Name_Format",
                table: "ReportTemplates",
                columns: new[] { "Name", "Format" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Category_EventType",
                table: "SystemLogs",
                columns: new[] { "Category", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Severity_Timestamp",
                table: "SystemLogs",
                columns: new[] { "Severity", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Timestamp",
                table: "SystemLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "ImageProcessingHistory");

            migrationBuilder.DropTable(
                name: "Monitors");

            migrationBuilder.DropTable(
                name: "ProcessingTemplates");

            migrationBuilder.DropTable(
                name: "ReportExportHistory");

            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "MotionEvents");

            migrationBuilder.DropTable(
                name: "ReportTemplates");

            migrationBuilder.DropTable(
                name: "Cameras");
        }
    }
}
