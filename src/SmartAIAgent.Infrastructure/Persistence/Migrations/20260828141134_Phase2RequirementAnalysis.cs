using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAIAgent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2RequirementAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "AgentRuns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromptVersion",
                table: "AgentRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "AgentRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "AgentRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "AgentRunId",
                table: "UserStories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssumptionsJson",
                table: "UserStories",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DevelopmentTasksJson",
                table: "UserStories",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "TechnicalAreasJson",
                table: "UserStories",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateIndex(
                name: "IX_UserStories_AgentRunId",
                table: "UserStories",
                column: "AgentRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStories_AgentRuns_AgentRunId",
                table: "UserStories",
                column: "AgentRunId",
                principalTable: "AgentRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStories_AgentRuns_AgentRunId",
                table: "UserStories");

            migrationBuilder.DropIndex(
                name: "IX_UserStories_AgentRunId",
                table: "UserStories");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "AgentRuns");

            migrationBuilder.DropColumn(
                name: "PromptVersion",
                table: "AgentRuns");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "AgentRuns");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "AgentRuns");

            migrationBuilder.DropColumn(
                name: "AgentRunId",
                table: "UserStories");

            migrationBuilder.DropColumn(
                name: "AssumptionsJson",
                table: "UserStories");

            migrationBuilder.DropColumn(
                name: "DevelopmentTasksJson",
                table: "UserStories");

            migrationBuilder.DropColumn(
                name: "TechnicalAreasJson",
                table: "UserStories");
        }
    }
}
