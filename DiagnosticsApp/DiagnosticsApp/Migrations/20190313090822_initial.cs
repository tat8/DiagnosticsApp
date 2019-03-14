using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiagnosticsApp.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    clientId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    firstName = table.Column<string>(maxLength: 50, nullable: false),
                    fatherName = table.Column<string>(maxLength: 50, nullable: true),
                    lastName = table.Column<string>(maxLength: 50, nullable: false),
                    birthDate = table.Column<DateTime>(type: "date", nullable: false),
                    isMale = table.Column<bool>(nullable: false),
                    passport = table.Column<string>(maxLength: 50, nullable: false),
                    SNILS = table.Column<string>(maxLength: 50, nullable: false),
                    phoneNumber = table.Column<string>(maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.clientId);
                });

            migrationBuilder.CreateTable(
                name: "Diagnosis",
                columns: table => new
                {
                    diagnosisId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    diagnosisName = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnosis", x => x.diagnosisId);
                });

            migrationBuilder.CreateTable(
                name: "Examination",
                columns: table => new
                {
                    examinationId = table.Column<long>(nullable: false),
                    temperature = table.Column<double>(nullable: false),
                    pressure = table.Column<string>(maxLength: 50, nullable: false),
                    height = table.Column<int>(nullable: false),
                    weight = table.Column<int>(nullable: false),
                    breath = table.Column<string>(maxLength: 50, nullable: false),
                    complaint = table.Column<string>(maxLength: 200, nullable: false),
                    other = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examination", x => x.examinationId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    roleId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    roleName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.roleId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    firstName = table.Column<string>(maxLength: 50, nullable: false),
                    fatherName = table.Column<string>(maxLength: 50, nullable: true),
                    lastName = table.Column<string>(maxLength: 50, nullable: false),
                    INN = table.Column<string>(maxLength: 12, nullable: false),
                    roleId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.userId);
                    table.ForeignKey(
                        name: "FK_User_Role",
                        column: x => x.roleId,
                        principalTable: "Role",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    appointmentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    doctorId = table.Column<long>(nullable: false),
                    startTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    clientId = table.Column<long>(nullable: false),
                    prescription = table.Column<string>(maxLength: 200, nullable: false),
                    examinationId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment", x => x.appointmentId);
                    table.ForeignKey(
                        name: "FK_Appointment_Client",
                        column: x => x.clientId,
                        principalTable: "Client",
                        principalColumn: "clientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointment_User",
                        column: x => x.doctorId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointment_Examination",
                        column: x => x.examinationId,
                        principalTable: "Examination",
                        principalColumn: "examinationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Diagnostics",
                columns: table => new
                {
                    diagnosticsId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    doctorId = table.Column<long>(nullable: false),
                    startTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    clientId = table.Column<long>(nullable: false),
                    examinationId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnostics", x => x.diagnosticsId);
                    table.ForeignKey(
                        name: "FK_Diagnostics_Client",
                        column: x => x.clientId,
                        principalTable: "Client",
                        principalColumn: "clientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Diagnostics_User",
                        column: x => x.doctorId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Diagnostics_Examination",
                        column: x => x.examinationId,
                        principalTable: "Examination",
                        principalColumn: "examinationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPassword",
                columns: table => new
                {
                    userId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    password = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPassword", x => x.userId);
                    table.ForeignKey(
                        name: "FK_UserPassword_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentDiagnosis",
                columns: table => new
                {
                    appointmentId = table.Column<long>(nullable: false),
                    diagnosisId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentDiagnosis", x => new { x.appointmentId, x.diagnosisId });
                    table.ForeignKey(
                        name: "FK_AppointmentDiagnosis_Appointment",
                        column: x => x.appointmentId,
                        principalTable: "Appointment",
                        principalColumn: "appointmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppointmentDiagnosis_Diagnosis",
                        column: x => x.diagnosisId,
                        principalTable: "Diagnosis",
                        principalColumn: "diagnosisId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Image",
                columns: table => new
                {
                    imageId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    diagnosticsId = table.Column<long>(nullable: false),
                    calcinatesPercent = table.Column<double>(nullable: false),
                    calcinateBiggest = table.Column<double>(nullable: false),
                    calcinatesCount = table.Column<int>(nullable: false),
                    refNotParsed = table.Column<string>(maxLength: 50, nullable: false),
                    refParsed = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.imageId);
                    table.ForeignKey(
                        name: "FK_Image_Diagnostics",
                        column: x => x.diagnosticsId,
                        principalTable: "Diagnostics",
                        principalColumn: "diagnosticsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_clientId",
                table: "Appointment",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_examinationId",
                table: "Appointment",
                column: "examinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment",
                table: "Appointment",
                columns: new[] { "doctorId", "startTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDiagnosis_diagnosisId",
                table: "AppointmentDiagnosis",
                column: "diagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentDiagnosis",
                table: "AppointmentDiagnosis",
                columns: new[] { "appointmentId", "diagnosisId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client",
                table: "Client",
                column: "SNILS",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosis",
                table: "Diagnosis",
                column: "diagnosisName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diagnostics_clientId",
                table: "Diagnostics",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnostics_examinationId",
                table: "Diagnostics",
                column: "examinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnostics",
                table: "Diagnostics",
                columns: new[] { "doctorId", "startTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_diagnosticsId",
                table: "Image",
                column: "diagnosticsId");

            migrationBuilder.CreateIndex(
                name: "IX_Role",
                table: "Role",
                column: "roleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User",
                table: "User",
                column: "INN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_roleId",
                table: "User",
                column: "roleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentDiagnosis");

            migrationBuilder.DropTable(
                name: "Image");

            migrationBuilder.DropTable(
                name: "UserPassword");

            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "Diagnosis");

            migrationBuilder.DropTable(
                name: "Diagnostics");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Examination");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
