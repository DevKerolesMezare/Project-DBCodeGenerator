using Guna.UI2.WinForms;
using Project_DBCodeGenerator.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_DBCodeGenerator
{
    public partial class frmMain : Form
    {

        private clsServerInfo _Server = new clsServerInfo();
        private clsDALCode _Code = new clsDALCode();

        private clsDatabaseInfo _Database;



        public frmMain()
        {
            InitializeComponent();
            _Database = new clsDatabaseInfo(cbServerName.Text,cbDBName.Text);
        }

        // Pram
        private clsDALCode.DbMethodParams Pram()
        {
            clsDALCode.DbMethodParams @params = new clsDALCode.DbMethodParams(GetSelectedColumns() , dgvTableInfo , txtSPName.Text , txtFunName.Text, txtOutputName.Text);

            return @params; 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Load Server names in to ComboBox
            cbServerName.DataSource =  _Server.GetServerNameFromPC();

            //// Load DB names in to ComboBox
            if (cbServerName.SelectedItem != null)
            {
                cbDBName.DataSource =  _Server.GetDBNamesFromServer(cbServerName.Text);
            }

        }
      

        private void cbDBName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Database.DBName = cbDBName.Text;
            dgvAllTables.DataSource = _Database.GetTablesName();
        }

        private void btnClosse_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnMaximize_Click(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Normal
    ? FormWindowState.Maximized
    : FormWindowState.Normal;
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void dgvAllTables_SelectionChanged(object sender, EventArgs e)
        {
            string TableName= dgvAllTables.CurrentRow.Cells[2].Value.ToString();

            dgvTableInfo.DataSource = _Database.GetTableColumnsWithType(TableName);
        }



        // Get Selected Database Columns    
        private DataTable GetSelectedColumns()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Columns_Name");
            dt.Columns.Add("Columns_Type");

            foreach (DataGridViewRow row in dgvTableInfo.Rows)
            {
                // قراءة قيمة الـ CheckBox بشكل مختصر وواضح
                bool isChecked = row.Cells[0].Value is bool value && value;

                if (isChecked)
                {
                    dt.Rows.Add(
                        row.Cells[1].Value?.ToString(),
                        row.Cells[2].Value?.ToString()
                    );
                }
            }

            return dt;
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
          txtCode.Text = _Code.AddNewRecord(Pram());
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            txtCode.Text =  _Code.UpdateRecord(Pram());

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            txtCode.Text =  _Code.DeleteRecord(Pram());

        }

        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            txtCode.Text = _Code.GetAll(Pram());
        }

        private void btnIsExists_Click(object sender, EventArgs e)
        {
            txtCode.Text =  _Code.IsExists(Pram());
        }

        private void btnConnection_Click(object sender, EventArgs e)
        {
            txtConnectionString.Text  = _Code.CreateDBConnection(cbServerName.Text, cbDBName.Text);
        }


    }
}
