using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMApp.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_CustomerCompanies_CompanyCustomerId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_CustomerIndividuals_IndividualCustomerId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhones_CustomerCompanies_CompanyCustomerId",
                table: "ContactPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhones_CustomerIndividuals_IndividualCustomerId",
                table: "ContactPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCompanyRelations_CustomerCompanies_CompanyCustomerId",
                table: "CustomerCompanyRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCompanyRelations_CustomerIndividuals_IndividualCustomerId",
                table: "CustomerCompanyRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_CustomerCompanies_CompanyCustomerId",
                table: "Emails");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_CustomerIndividuals_IndividualCustomerId",
                table: "Emails");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_CustomerCompanies_CompanyCustomerId",
                table: "Addresses",
                column: "CompanyCustomerId",
                principalTable: "CustomerCompanies",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_CustomerIndividuals_IndividualCustomerId",
                table: "Addresses",
                column: "IndividualCustomerId",
                principalTable: "CustomerIndividuals",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhones_CustomerCompanies_CompanyCustomerId",
                table: "ContactPhones",
                column: "CompanyCustomerId",
                principalTable: "CustomerCompanies",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhones_CustomerIndividuals_IndividualCustomerId",
                table: "ContactPhones",
                column: "IndividualCustomerId",
                principalTable: "CustomerIndividuals",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCompanyRelations_CustomerCompanies_CompanyCustomerId",
                table: "CustomerCompanyRelations",
                column: "CompanyCustomerId",
                principalTable: "CustomerCompanies",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCompanyRelations_CustomerIndividuals_IndividualCustomerId",
                table: "CustomerCompanyRelations",
                column: "IndividualCustomerId",
                principalTable: "CustomerIndividuals",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_CustomerCompanies_CompanyCustomerId",
                table: "Emails",
                column: "CompanyCustomerId",
                principalTable: "CustomerCompanies",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_CustomerIndividuals_IndividualCustomerId",
                table: "Emails",
                column: "IndividualCustomerId",
                principalTable: "CustomerIndividuals",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_CustomerCompanies_CompanyCustomerId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_CustomerIndividuals_IndividualCustomerId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhones_CustomerCompanies_CompanyCustomerId",
                table: "ContactPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhones_CustomerIndividuals_IndividualCustomerId",
                table: "ContactPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCompanyRelations_CustomerCompanies_CompanyCustomerId",
                table: "CustomerCompanyRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCompanyRelations_CustomerIndividuals_IndividualCustomerId",
                table: "CustomerCompanyRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_CustomerCompanies_CompanyCustomerId",
                table: "Emails");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_CustomerIndividuals_IndividualCustomerId",
                table: "Emails");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_CustomerCompanies_CompanyCustomerId",
                table: "Addresses",
                column: "CompanyCustomerId",
                principalTable: "CustomerCompanies",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_CustomerIndividuals_IndividualCustomerId",
                table: "Addresses",
                column: "IndividualCustomerId",
                principalTable: "CustomerIndividuals",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhones_CustomerCompanies_CompanyCustomerId",
                table: "ContactPhones",
                column: "CompanyCustomerId",
                principalTable: "CustomerCompanies",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhones_CustomerIndividuals_IndividualCustomerId",
                table: "ContactPhones",
                column: "IndividualCustomerId",
                principalTable: "CustomerIndividuals",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCompanyRelations_CustomerCompanies_CompanyCustomerId",
                table: "CustomerCompanyRelations",
                column: "CompanyCustomerId",
                principalTable: "CustomerCompanies",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCompanyRelations_CustomerIndividuals_IndividualCustomerId",
                table: "CustomerCompanyRelations",
                column: "IndividualCustomerId",
                principalTable: "CustomerIndividuals",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_CustomerCompanies_CompanyCustomerId",
                table: "Emails",
                column: "CompanyCustomerId",
                principalTable: "CustomerCompanies",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_CustomerIndividuals_IndividualCustomerId",
                table: "Emails",
                column: "IndividualCustomerId",
                principalTable: "CustomerIndividuals",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
