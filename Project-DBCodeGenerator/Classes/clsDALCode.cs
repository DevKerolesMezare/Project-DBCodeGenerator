using System;
using System.Data;
using System.Linq;
using System.Text;

namespace Project_DBCodeGenerator.Classes
{

    // دي صفحه الفورمات ممكن تضيف اي فورمات بدلا من الفورمات الخاص بطريقتي

    public class clsDALCode
    {
        public class DbMethodParams
        {
            // الخصائص
            public DataTable DataTable { get; set; }
            public Guna.UI2.WinForms.Guna2DataGridView DgvTableInfo { get; set; }
            public string SpName { get; set; }
            public string FunName { get; set; }

            public string OutputPramName { get; set; }

            // Constructor لتسهيل الإسناد عند الإنشاء
            public DbMethodParams(DataTable dataTable, Guna.UI2.WinForms.Guna2DataGridView dgvTableInfo, string spName = "", string funName = "", string OutName = "@NewID")
            {
                this.DataTable = dataTable;
                this.DgvTableInfo = dgvTableInfo;
                this.SpName = spName;
                this.FunName = funName;
                this.OutputPramName = OutName;
            }
        }

        private string GetCSharpType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "int": return "int";
                case "bigint": return "long";
                case "bit": return "bool";
                case "varchar": return "string";
                case "datetime": return "DateTime";
                case "decimal": return "decimal";
                default: return "string";
            }
        }

        public string CreateDBConnection(string ServerName, string DBName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"public static string ConnectionString = \"Data Source={ServerName}; Initial Catalog={DBName}; Integrated Security=true;\";");


            return sb.ToString();
        }

        public string AddNewRecord(DbMethodParams M)
        {
            // ملحوظه بعد ما تنشي الكود لازم اسم البرامتير يكون مطابق بالاسم الي انت ضايفه في الداتا بيز


            StringBuilder sb = new StringBuilder();

            // توليد Signature
            string parameters = string.Join(", ", M.DataTable.AsEnumerable().Select(r =>
            {
                string colName = r["Columns_Name"].ToString();
                string sqlType = r["Columns_Type"].ToString().ToLower();
                string csharpType = GetCSharpType(sqlType);

                string paramName = char.ToLower(colName[0]) + colName.Substring(1);
                return $"{csharpType} {paramName}";
            }));

            sb.AppendLine($"public static int {M.FunName}({parameters})");
            sb.AppendLine("{");
            sb.AppendLine($"    int? newID = null;");
            sb.AppendLine($"    using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("    {");
            sb.AppendLine($"        using (SqlCommand command = new SqlCommand(\"{M.SpName}\", connection))");
            sb.AppendLine("        {");
            sb.AppendLine("            command.CommandType = CommandType.StoredProcedure;");

            // إضافة Parameters
            foreach (DataRow row in M.DataTable.Rows)
            {
                string colName = row["Columns_Name"].ToString();
                string paramName = char.ToLower(colName[0]) + colName.Substring(1);
                sb.AppendLine($"            command.Parameters.AddWithValue(\"@{colName}\", {paramName});");
            }

            // Output Parameter عام لكل الجداول
            sb.AppendLine($"            command.Parameters.Add(\"{M.OutputPramName}\", SqlDbType.Int).Direction = ParameterDirection.Output;");

            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Open();");
            sb.AppendLine("                command.ExecuteNonQuery();");
            sb.AppendLine($"                object result = command.Parameters[\"{M.OutputPramName}\"].Value;");
            sb.AppendLine("                if (result != null && result != DBNull.Value && int.TryParse(result.ToString(), out int id))");
            sb.AppendLine($"                    newID = id;");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                clsLogException.LogException($\"Exception: {ex.Message}\", EventLogEntryType.Error);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("    return newID ?? -1;");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public string DeleteRecord(DbMethodParams M)
        {
            StringBuilder sb = new StringBuilder();

            string ColName = M.DgvTableInfo.CurrentRow.Cells[1].Value.ToString();

            sb.AppendLine($"static public bool {M.FunName}(int {ColName})");
            sb.AppendLine("{");
            sb.AppendLine("    bool isDeleted = false;");
            sb.AppendLine("    using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("    {");
            sb.AppendLine($"        using (SqlCommand command = new SqlCommand(\"{M.SpName}\", connection))");
            sb.AppendLine("        {");
            sb.AppendLine("            command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine($"            command.Parameters.AddWithValue(\"@{ColName}\", {ColName});");
            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Open();");
            sb.AppendLine("                isDeleted = (command.ExecuteNonQuery() > 0);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception e)");
            sb.AppendLine("            {");
            sb.AppendLine("                clsLogException.LogException($\"Exception: {e.Message}\", EventLogEntryType.Error);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    return isDeleted;");
            sb.AppendLine("}");






            return sb.ToString();
        }

        public string UpdateRecord(DbMethodParams M)
        {
            StringBuilder sb = new StringBuilder();

            // اسم الجدول من DataGridView
            string tableName = M.DgvTableInfo.CurrentRow.Cells[2].Value.ToString();

            // توليد Signature مع تحويل أنواع SQL → C#
            string parametersSignature = string.Join(", ", M.DataTable.AsEnumerable().Select(r =>
            {
                string sqlType = r["Columns_Type"].ToString().ToLower();
                string colName = r["Columns_Name"].ToString();
                string csharpType = GetCSharpType(sqlType);


                // تحويل اسم البراميتر إلى camelCase
                string paramName = char.ToLower(colName[0]) + colName.Substring(1);
                return $"{csharpType} {paramName}";
            }));

            sb.AppendLine($"static public bool {M.FunName}({parametersSignature})");
            sb.AppendLine("{");
            sb.AppendLine("    bool isUpdated = false;");
            sb.AppendLine("    using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("    {");
            sb.AppendLine($"        using (SqlCommand command = new SqlCommand(\"{M.SpName}\", connection))");
            sb.AppendLine("        {");
            sb.AppendLine("            command.CommandType = CommandType.StoredProcedure;");

            // إضافة Parameters
            foreach (DataRow row in M.DataTable.Rows)
            {
                string colName = row["Columns_Name"].ToString();
                string paramName = char.ToLower(colName[0]) + colName.Substring(1);
                sb.AppendLine($"            command.Parameters.AddWithValue(\"@{colName}\", {paramName});");
            }

            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Open();");
            sb.AppendLine("                command.ExecuteNonQuery();");
            sb.AppendLine("                isUpdated = (command.ExecuteNonQuery() > 0);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception e)");
            sb.AppendLine("            {");
            sb.AppendLine("                clsLogException.LogException($\"Exception: {e.Message}\", EventLogEntryType.Error);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    return isUpdated;");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public string IsExists(DbMethodParams M)
        {
            StringBuilder sb = new StringBuilder();

            // توليد Signature مع تحويل أنواع SQL → C#
            string parametersSignature = string.Join(", ", M.DataTable.AsEnumerable().Select(r =>
            {
                string sqlType = r["Columns_Type"].ToString().ToLower();
                string colName = r["Columns_Name"].ToString();
                string csharpType = GetCSharpType(sqlType);

                // تحويل اسم البراميتر إلى camelCase
                string paramName = char.ToLower(colName[0]) + colName.Substring(1);
                return $"{csharpType} {paramName}";
            }));

            // توقيع الدالة
            sb.AppendLine($"public static bool {M.FunName}({parametersSignature})");
            sb.AppendLine("{");
            sb.AppendLine("    bool isFound = false;");
            sb.AppendLine("    using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("    {");
            sb.AppendLine($"        using (SqlCommand command = new SqlCommand(\"{M.SpName}\", connection))");
            sb.AppendLine("        {");
            sb.AppendLine("            command.CommandType = CommandType.StoredProcedure;");

            // إضافة Parameters
            foreach (DataRow row in M.DataTable.Rows)
            {
                string colName = row["Columns_Name"].ToString();
                string paramName = char.ToLower(colName[0]) + colName.Substring(1);
                sb.AppendLine($"            command.Parameters.AddWithValue(\"@{colName}\", {paramName});");
            }

            sb.AppendLine();
            sb.AppendLine("            SqlParameter returnValue = new SqlParameter();");
            sb.AppendLine("            returnValue.Direction = ParameterDirection.ReturnValue;");
            sb.AppendLine("            command.Parameters.Add(returnValue);");
            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Open();");
            sb.AppendLine("                command.ExecuteNonQuery();");
            sb.AppendLine("                isFound = (Convert.ToInt32(returnValue.Value) > 0);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception e)");
            sb.AppendLine("            {");
            sb.AppendLine("                clsLogException.LogException($\"Exception: {e.Message}\", EventLogEntryType.Error);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    return isFound;");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public string GetAll(DbMethodParams M)
        {
            StringBuilder sb = new StringBuilder();

            // اسم الدالة هو FunName
            sb.AppendLine($"public static DataTable {M.FunName}()");
            sb.AppendLine("{");
            sb.AppendLine("    DataTable dt = new DataTable();");
            sb.AppendLine($"    using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            sb.AppendLine("    {");
            sb.AppendLine($"        using (SqlCommand command = new SqlCommand(\"{M.SpName}\", connection))");
            sb.AppendLine("        {");
            sb.AppendLine("            command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                connection.Open();");
            sb.AppendLine("                using (SqlDataReader reader = command.ExecuteReader())");
            sb.AppendLine("                {");
            sb.AppendLine("                    if (reader.HasRows)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        dt.Load(reader);");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                clsLogException.LogException($\"Exception: {ex.Message}\", EventLogEntryType.Error);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("    return dt;");
            sb.AppendLine("}");

            return sb.ToString();
        }



        ///////////// ضيف اي ميثود خاصه بطبقة الداتا اكسس فقط اذا حبيت تضيف طبقه اخري فالافضل انك تعمل كلاس مخصص 


    }

}
