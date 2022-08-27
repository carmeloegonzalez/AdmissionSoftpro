using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdmissionSoftpro
{
    public partial class frmStart : Form
    {
        string serverName = (string)Registry.GetValue("HKEY_CURRENT_USER\\Spa", "Servidor", "No connection");
        string password = (string)Registry.GetValue("HKEY_CURRENT_USER\\Spa", "PWD", "No connection");
        string dataBase = (string)Registry.GetValue("HKEY_CURRENT_USER\\Spa", "DB", "No connection");
        string connection = string.Empty;
        int seg = 0;
        int maxSeg = 5;
        bool pass = true;
        public frmStart()
        {
            InitializeComponent();
        }


        private void frmStart_Load(object sender, EventArgs e)
        {
            connection = $"Data Source={serverName}; Initial Catalog={dataBase}; user id = sa; password = {password}";
            pbCounter.Maximum = maxSeg;
            this.ShowInTaskbar = false;
            this.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            niNotify.Visible = true;
            niNotify.ShowBalloonTip(3000, "Admission Softpro", $"Saliendo del sistema", ToolTipIcon.Info);
            this.ShowInTaskbar = false;
            this.Visible = false;
            Application.Exit();

        }

        private void niNotify_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;

        }

        private void frmStart_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnExit.Enabled)
            {
                e.Cancel = true;
            }
        }

        private void tmTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                seg++;
                pbCounter.Value = seg;
                if (seg >= maxSeg)
                {
                    tmTimer.Enabled = false;
                    SearchPreingresos();
                    SearchPresupuestoWhatsApp();
                    seg = 0;
                    tmTimer.Enabled = true;
                    pass = true;

                }
            }
            catch (Exception)
            {
            }
        }

        private void SearchPreingresos()
        {

            using (SqlConnection objConn = new SqlConnection(connection))
            {
                try
                {
                    pass = false;

                    objConn.Open();
                    SqlTransaction objTrans = null;

                    SqlDataAdapter da = new SqlDataAdapter($"Select TOP(20) Cli_EncPresupuesto.* " +
                                                           $"FROM Cli_EncPresupuesto " +
                                                           $"Where Preingreso = 'True' " +
                                                           $" ", objConn);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        try
                        {

                            SqlCommand objCmd1;

                            // Start a local transaction.
                            objTrans = objConn.BeginTransaction();
                            // Must assign both transaction object and connection
                            // to Command object for a pending local transaction
                            objCmd1 = objConn.CreateCommand();
                            objCmd1.Connection = objConn;
                            objCmd1.Transaction = objTrans;
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                objCmd1.CommandText = $"UPDATE Cli_EncPresupuesto SET Preingreso = 'false' Where PresupuestoID = '{ds.Tables[0].Rows[i]["PresupuestoID"].ToString()}'";
                                objCmd1.ExecuteNonQuery();

                                niNotify.Visible = true;
                                niNotify.ShowBalloonTip(3000, "Solicitar preingreso", $"{ds.Tables[0].Rows[i]["NombreP"].ToString()}", ToolTipIcon.Info);
                            }
                            objTrans.Commit();

                        }
                        catch (Exception)
                        {
                            objTrans.Rollback();
                        }

                    }
                }

                catch (Exception ex)
                {

                }
                finally
                {
                    label1.Text = (int.Parse(label1.Text) + 1).ToString();
                    Application.DoEvents();
                    objConn.Close();
                }
            }
        }
        private void SearchPresupuestoWhatsApp()
        {


                using (SqlConnection objConn = new SqlConnection(connection))
                {
                try
                {
                    pass = false;
                    SqlTransaction objTrans = null;

                    objConn.Open();
                    SqlDataAdapter da = new SqlDataAdapter($"Select TOP(20) Cli_EncPresupuesto.* " +
                                                           $"FROM Cli_EncPresupuesto " +
                                                           $"Where EnviarWhatsApp = 'True' " +
                                                           $" ", objConn);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    try
                    {
                        SqlCommand objCmd1;

                        // Start a local transaction.
                        objTrans = objConn.BeginTransaction();
                        // Must assign both transaction object and connection
                        // to Command object for a pending local transaction
                        objCmd1 = objConn.CreateCommand();
                        objCmd1.Connection = objConn;
                        objCmd1.Transaction = objTrans;
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            objCmd1.CommandText = $"UPDATE Cli_EncPresupuesto SET EnviarWhatsApp = 'false' Where PresupuestoID = '{ds.Tables[0].Rows[i]["PresupuestoID"].ToString()}'";
                            objCmd1.ExecuteNonQuery();

                            niNotify.Visible = true;
                            niNotify.ShowBalloonTip(3000, "Enviar presupuesto", $"{ds.Tables[0].Rows[i]["NombreP"].ToString()}", ToolTipIcon.Info);
                        }
                        objTrans.Commit();

                    }
                    catch (Exception)
                    {
                        objTrans.Rollback();
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    label2.Text = (int.Parse(label2.Text) + 1).ToString();
                    Application.DoEvents();
                    objConn.Close();
                }
            }

        }
    }
}
