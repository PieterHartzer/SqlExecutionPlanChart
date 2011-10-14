using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Text;

namespace SqlExecutionPlan
{
    public partial class Default : System.Web.UI.Page
    {
        #region PRIVATES
        /// <summary>
        /// Connection string for the database connection
        /// </summary>
        private const string CONNETIONSTRING = "Data Source=SERVER;Initial Catalog=DATABASE;User ID=USERNAME;Password=PASSWORD";
        
        /// <summary>
        /// The format string for the JavaScript data
        /// </summary>
        private const string DATAROW = "[{{v:'{0}', f:'{1}'}}, '{2}', '{3}'],";

        /// <summary>
        /// Template for the display values in the chart nodes
        /// </summary>
        private const string TEMPLATE = "{0} ({1})<br /><small><strong>Rows:</strong>{2}<br /><strong>Executes:</strong>{3}<br /><strong>Total Subtree Cost:</strong>{4}<br /></small>";
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            // Hide the controls
            fldLegend.Visible = false;
            lblError.Visible = false;
        }

        protected void btnExecute_Click(object sender, EventArgs e)
        {
            StringBuilder sqlBuilder = new StringBuilder();

            // Add the SQL that will return the execution plan
            sqlBuilder.Append("SET STATISTICS PROFILE ON;");
            // Add the SQL supplied
            sqlBuilder.AppendLine(txtSql.Text);
            // Add the SQL that will stop future execution plans
            sqlBuilder.AppendLine(";SET STATISTICS PROFILE OFF;");

            using (SqlConnection sqlCon = new SqlConnection(CONNETIONSTRING))
            {
                try
                {
                    sqlCon.Open();
                    using (SqlCommand sqlCmd = new SqlCommand(sqlBuilder.ToString(), sqlCon))
                    {
                        SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                        StringBuilder jsBuilder = new StringBuilder();

                        // Add the JS's first part (1/3)
                        jsBuilder.AppendLine(@" function drawVisualization() {
                                            // Create and populate the data table.
                                            var data = new google.visualization.DataTable();
                                            data.addColumn('string', 'Id');
                                            data.addColumn('string', 'Parent');
                                            data.addColumn('string', 'ToolTip');
                                            data.addRows([");

                        //  while (sqlReader.Read())
                        //  {
                        //      Process executed SQL here
                        //  }

                        // Jump to the next result - execution plan
                        sqlReader.NextResult();

                        // Now read and build the chart
                        while (sqlReader.Read())
                        {
                            // Get the values
                            int parent = sqlReader.GetInt32(sqlReader.GetOrdinal("Parent"));
                            int id = sqlReader.GetInt32(sqlReader.GetOrdinal("NodeId"));
                            string physicalOperation = sqlReader.IsDBNull(sqlReader.GetOrdinal("PhysicalOp")) ? sqlReader.GetString(sqlReader.GetOrdinal("Type")) : sqlReader.GetString(sqlReader.GetOrdinal("PhysicalOp"));
                            string logicalOperation = sqlReader.IsDBNull(sqlReader.GetOrdinal("LogicalOp")) ? sqlReader.GetString(sqlReader.GetOrdinal("Type")) : sqlReader.GetString(sqlReader.GetOrdinal("LogicalOp"));
                            long rows = sqlReader.GetInt64(sqlReader.GetOrdinal("Rows"));
                            long executes = sqlReader.GetInt64(sqlReader.GetOrdinal("Executes"));
                            float totalSubTreeCost = sqlReader.GetFloat(sqlReader.GetOrdinal("TotalSubTreeCost"));

                            // Build the string
                            jsBuilder.AppendFormat(DATAROW,
                                id,
                                string.Format(TEMPLATE,
                                    physicalOperation,
                                    logicalOperation,
                                    rows,
                                    executes,
                                    totalSubTreeCost),
                                (parent != 0) ? parent.ToString() : string.Empty, // Main node should have an empty parent for Google Charts
                                "");
                            jsBuilder.AppendLine();
                        }

                        // Remove the last comma
                        jsBuilder.Remove(jsBuilder.Length - 1, 1);

                        // Add the JS's last part (3/3)
                        jsBuilder.AppendLine(@"]);
                                            // Create and draw the visualization.
                                            new google.visualization.OrgChart(document.getElementById('chartDiv')).
                                                draw(data, 
                                                            {
                                                                allowHtml: true
                                                            }
                                                    );
                                                
                                            }
                                            google.setOnLoadCallback(drawVisualization);");

                        // Put the JS in the page
                        RegisterJavaScript(jsBuilder.ToString());

                        // Clean up
                        sqlReader.Close();
                        sqlCon.Close();
                    }

                    // Show the controls
                    fldLegend.Visible = false;
                }
                catch (Exception ex)
                {
                    DisplayError(ex.Message);
                }

                // Show the controls
                fldLegend.Visible = true;
            }
        }

        /// <summary>
        /// Registers the JavaScript into the page
        /// </summary>
        /// <param name="js">The script to register</param>
        private void RegisterJavaScript(string js)
        {
            if (!Page.ClientScript.IsClientScriptBlockRegistered("CHART"))
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CHART", js, true);
            }
        }

        /// <summary>
        /// Display an error message
        /// </summary>
        /// <param name="message">The message to display</param>
        private void DisplayError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }
    }
}