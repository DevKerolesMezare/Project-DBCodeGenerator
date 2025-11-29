using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace Project_DBCodeGenerator.Classes
{
    public class clsDatabaseInfo
    {
        public string ServerName {  get; set; }
        public string DBName { get; set; }


        private SqlConnection connection ;


        public clsDatabaseInfo(string serverName, string DBName)
        {
            this.ServerName = serverName;
            this.DBName = DBName;
        }


        // Get Tables Name From DB
        public DataTable GetTablesName()
        {
            DataTable dt = new DataTable();

            using (connection = new SqlConnection($"Data Source = {this.ServerName}; Initial Catalog = {this.DBName}; Integrated Security = true; "))
            {
                connection.Open();
                dt = connection.GetSchema("Tables");
          
            }

            return dt; 

        }


        // Get All Columns From Table in DB
        public DataTable GetTableColumns(string tableName)
        {
            DataTable dt = new DataTable();

            string[] ColRestruction = new string[4];

            /*
                         // مصفوفة قيود:
            {Database, Owner, TableName, ColumnName}

            string[] ColRestriction = new string[4];

            ColRestriction[0] = "MyDB";        // اسم قاعدة البيانات
            ColRestriction[1] = "dbo";         // مالك الجدول
            ColRestriction[2] = "Users";       // اسم الجدول
            ColRestriction[3] = null;          // اسم العمود (null = كل الأعمدة)
             */



            using (connection = new SqlConnection($"Data Source = {this.ServerName}; Initial Catalog = {this.DBName}; Integrated Security = true; "))
            {
                connection.Open();

                ColRestruction[2] = tableName;  // ColRestriction[2] = "Users";       // اسم الجدول

                dt = connection.GetSchema("Columns", ColRestruction);
            }

            return dt;
        }



        // Get Custom Columns From Table in DB
        public DataTable GetTableColumnsWithType(string tableName, string schemaName = "dbo")
        {
            // إنشاء DataTable جديد يحتوي عمودين: ColumnName و DataType
            DataTable dtColumns = new DataTable();
            dtColumns.Columns.Add("Columns_Name", typeof(string));
            dtColumns.Columns.Add("Columns_Type" , typeof(string));

            using (SqlConnection connection = new SqlConnection($"Data Source={this.ServerName}; Initial Catalog={this.DBName}; Integrated Security=true;"))
            {
                connection.Open();

                // مصفوفة القيود: {Database, Owner, TableName, ColumnName}
                string[] colRestriction = new string[4];
                colRestriction[1] = schemaName;  // Schema
                colRestriction[2] = tableName;   // اسم الجدول

                // جلب كل الأعمدة
                DataTable schemaTable = connection.GetSchema("Columns", colRestriction);

                // إضافة اسم العمود ونوعه
                foreach (DataRow row in schemaTable.Rows)
                {
                    string columnName = row["COLUMN_NAME"].ToString();
                    string dataType = row["DATA_TYPE"].ToString(); // نوع البيانات
                    dtColumns.Rows.Add(columnName, dataType);
                }
            }

            return dtColumns;
        }
    }
}
