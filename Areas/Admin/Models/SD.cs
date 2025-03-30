namespace Lab3.Areas.Admin.Models
{
    public class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";
        public const string Role_company = "company";

        public static string Role_Company { get; internal set; }
    }
}
