using DemoProject.DataModels;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;

#nullable disable

namespace DemoProject.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var roleNames = typeof(RolesTypes)
           .GetFields(BindingFlags.Public | BindingFlags.Static)
           .Where(f => f.FieldType == typeof(string))
           .Select(f => (string)f.GetValue(null))
           .ToArray();

            foreach (var roleName in roleNames)
            {
                migrationBuilder.InsertData(
                    table: "AspNetRoles",
                    columns: new[] { "Id", "Name", "NormalizedName" },
                    values: new object[]
                    {
                    Guid.NewGuid().ToString(), //Id
                    roleName,
                    roleName.ToUpper(), //NormalizedName 
                    });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
