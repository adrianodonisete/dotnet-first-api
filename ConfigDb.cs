using Microsoft.Data.SqlClient;

public class ConfigDb
{
    public string getStringCon()
    {
        var builderConn = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            InitialCatalog = "Products",
            UserID = "sa",
            Password = "@Sql2022"
        };

        return builderConn.ConnectionString;
    }
}