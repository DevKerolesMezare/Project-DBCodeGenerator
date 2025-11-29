using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_DBCodeGenerator.Classes
{
    public class clsServerInfo
    {
        // Get a list of available SQL Server names with their Instances
        public List<string> GetServerNameFromPC()
        {
            List<string> ServerList = new List<string>();
            DataTable ServerInfo = SqlDataSourceEnumerator.Instance.GetDataSources();

            foreach (DataRow row in ServerInfo.Rows)
            {
                string ServerName = row[0].ToString();

                if (!string.IsNullOrEmpty(row[1].ToString()))
                    ServerName += "\\" + row[1].ToString();

                ServerList.Add(ServerName);
            }

            return ServerList;
        }




         //   Get The DataBase Available in the Server
        public List<string> GetDBNamesFromServer(string ServerName)
        {
            List<String> DBNames = new List<String>();

            SqlConnectionStringBuilder con = new SqlConnectionStringBuilder()
            {
                DataSource = ServerName,
                IntegratedSecurity = true,
            };

            using (SqlConnection connection = new SqlConnection(con.ToString()))
            {
                try
                {
                    connection.Open();

                    DataTable dt = connection.GetSchema("Databases");

                    foreach (DataRow row in dt.Rows)
                    {
                        DBNames.Add(row["database_name"].ToString());
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: "+ ex.Message);
                }
            }


            return DBNames;
        }


    }

}
