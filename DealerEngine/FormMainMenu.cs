using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace DealerEngine;

public partial class FormMainMenu : Form
{
    private const int MARGIN = 10;

    private Panel panelTopMenuBar;
    private Panel panelBottomMenuBar;
    private Panel panelStagedInvoiceTotals;

    private Button buttonImport;
    private Button buttonAddDealerToQueue;
    private Button buttonGenerate;

    private GroupBox gbInvoiceDate;
    private DateTimePicker dateTimePicker;

    private DataGridView dgvStagedInvoices;

    private Label labelVehicleCountAllDealers;
    private Label labelVehicleCountAllDealersValue;
    private Label labelTotalDueAllDealers;
    private Label labelTotalDueAllDealersValue;

    // Constructor
    public FormMainMenu()
    {
        InitializeComponent();

        DrawForm();
    }



    // Draw form controls.
    private void DrawForm()
    {
        SuspendLayout();
        
        Controls.Clear();

        Size buttonSize = new Size(150, 40);

        Icon = Properties.Resources.icon_dealerengine;
        Text = $"{Application.ProductName} {Application.ProductVersion}";

        // Top menu bar
        panelTopMenuBar = new Panel
        {
            Size = new Size(
                width: ClientSize.Width,
                height: buttonSize.Height + MARGIN * 2),
            Location = new Point(0, 0),
            Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
            BackColor = Color.Transparent
        };

        buttonImport = new Button
        {
            Size = buttonSize,
            Location = new Point(MARGIN, MARGIN),
            Anchor = (AnchorStyles.Top | AnchorStyles.Left),
            TabIndex = 0,
            Text = "Import from File(s)",
            Image = Properties.Resources.img_import,
            TextImageRelation = TextImageRelation.ImageBeforeText,
        };
        buttonImport.Click += new EventHandler(buttonImport_Click);
        panelTopMenuBar.Controls.Add(buttonImport);

        buttonAddDealerToQueue = new Button
        {
            Enabled = false,
            Size = buttonSize,
            Location = new Point(buttonImport.Right + MARGIN, MARGIN),
            Anchor = (AnchorStyles.Top | AnchorStyles.Left),
            TabIndex = 1,
            Text = "Add Dealer to Queue",
            Image = Properties.Resources.img_new,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        panelTopMenuBar.Controls.Add(buttonAddDealerToQueue);
        
        Controls.Add(panelTopMenuBar);

        // Bottom menu bar
        panelBottomMenuBar = new Panel
        {
            Size = new Size(
                width: ClientSize.Width,
                height: buttonSize.Height + MARGIN * 2),
            Location = new Point(0, ClientSize.Height - (buttonSize.Height + MARGIN * 2)),
            Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
            BackColor = Color.Transparent
        };

        gbInvoiceDate = new GroupBox
        {
            Size = new Size(100, buttonSize.Height),
            Location = new Point(MARGIN, MARGIN),
            Anchor = (AnchorStyles.Bottom | AnchorStyles.Left),
            TabStop = false,
            Text = "Invoice Date"
        };
        panelBottomMenuBar.Controls.Add(gbInvoiceDate);

        dateTimePicker = new DateTimePicker
        {
            Size = new Size(gbInvoiceDate.Width - MARGIN * 2, 20),
            Location = new Point(MARGIN, Convert.ToInt32(MARGIN * 1.5)),
            TabIndex = 2,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "MM/dd/yyyy"
        };
        gbInvoiceDate.Controls.Add(dateTimePicker);

        // Staged invoice totals
        panelStagedInvoiceTotals = new Panel()
        {
            //Enabled = false
        };

        labelVehicleCountAllDealers = new Label
        {
            Size = new Size(120, 20),
            Location = new Point(0, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Vehicle Count"
        };
        panelStagedInvoiceTotals.Controls.Add(labelVehicleCountAllDealers);

        labelTotalDueAllDealers = new Label
        {
            Size = new Size(120, 20),
            Location = new Point(0, labelVehicleCountAllDealers.Bottom),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Total Sales"
        };
        panelStagedInvoiceTotals.Controls.Add(labelTotalDueAllDealers);

        labelVehicleCountAllDealersValue = new Label
        {
            Size = new Size(120, 20),
            Location = new Point(labelVehicleCountAllDealers.Right, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = ""
        };
        panelStagedInvoiceTotals.Controls.Add(labelVehicleCountAllDealersValue);

        labelTotalDueAllDealersValue = new Label
        {
            Size = new Size(120, 20),
            Location = new Point(labelVehicleCountAllDealersValue.Left, labelVehicleCountAllDealersValue.Bottom),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = ""
        };
        panelStagedInvoiceTotals.Controls.Add(labelTotalDueAllDealersValue);

        panelStagedInvoiceTotals.Size = new Size(
            width: labelTotalDueAllDealersValue.Right, 
            height: labelTotalDueAllDealersValue.Bottom);

        panelStagedInvoiceTotals.Location = new Point(
            x: (panelBottomMenuBar.Width / 2) - (panelStagedInvoiceTotals.Width / 2),
            y: (panelBottomMenuBar.Height / 2) - (panelStagedInvoiceTotals.Height / 2));

        panelBottomMenuBar.Controls.Add(panelStagedInvoiceTotals);

        // Generate invoices button
        buttonGenerate = new Button
        {
            Size = buttonSize,
            Location = new Point(panelBottomMenuBar.Right - buttonSize.Width - MARGIN, MARGIN),
            Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
            TabIndex = 3,
            Text = "Generate",
            Image = Properties.Resources.img_download,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Enabled = false
        };
        buttonGenerate.Click += new EventHandler(buttonGenerate_Click);
        panelBottomMenuBar.Controls.Add(buttonGenerate);

        Controls.Add(panelBottomMenuBar);

        // Staged invoices
        dgvStagedInvoices = new DataGridView
        {
            Size = new Size(
                width: ClientSize.Width - MARGIN * 2,
                height: panelBottomMenuBar.Top - panelTopMenuBar.Bottom),
            Location = new Point(MARGIN, panelTopMenuBar.Bottom),
            Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            AutoGenerateColumns = false,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AllowUserToResizeColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        // Disable header highlighting
        dgvStagedInvoices.EnableHeadersVisualStyles = false;
        dgvStagedInvoices.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgvStagedInvoices.ColumnHeadersDefaultCellStyle.BackColor;

        // Staged invoice columns
        DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Name",
            HeaderText = "Name",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        };
        colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        DataGridViewTextBoxColumn colWorkOrderCount = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "WorkOrderCount",
            HeaderText = "# Work Orders",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        };
        colWorkOrderCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        DataGridViewTextBoxColumn colVehicleCount = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "VehicleCount",
            HeaderText = "# Vehicles",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        };
        colVehicleCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        DataGridViewTextBoxColumn colTotalOTLCharges = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TotalOTLCharges",
            HeaderText = "On-the-lot Charges",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        };
        colTotalOTLCharges.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colTotalOTLCharges.DefaultCellStyle.Format = "c";

        DataGridViewTextBoxColumn colTotalMonthlyCharges = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TotalMonthlyCharges",
            HeaderText = "Monthly Charges",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        };
        colTotalMonthlyCharges.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colTotalMonthlyCharges.DefaultCellStyle.Format = "c";

        DataGridViewTextBoxColumn colTotalDue = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TotalInvoiceAmount",
            HeaderText = "Total Charges",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        };
        colTotalDue.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colTotalDue.DefaultCellStyle.Format = "c";

        dgvStagedInvoices.Columns.AddRange(
        [
            colName,
            colWorkOrderCount,
            colVehicleCount,
            colTotalOTLCharges,
            colTotalMonthlyCharges,
            colTotalDue
        ]);

        Controls.Add(dgvStagedInvoices);

        ResumeLayout();

        updateStagedInvoices();
    }

    // Updates the Staged Invoices displayed in the DataGridView.
    private void updateStagedInvoices()
    {
        SuspendLayout();

        var staged = Dealer.StagedDealers;

        dgvStagedInvoices.DataSource = staged;
        dgvStagedInvoices.Refresh();

        labelVehicleCountAllDealersValue.Text = Dealer.GetVehicleCountAllStagedDealers().ToString();
        labelTotalDueAllDealersValue.Text = Dealer.GetTotalDueAllStagedDealers().ToString("c");

        buttonGenerate.Enabled = staged.Length > 0;

        ResumeLayout();
    }

    // Handles clicking the Import button.
    // TODO: invoke progress bar to stop repeated redrawing of dgv
    private void buttonImport_Click(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog
        {
            Filter = "CSV|*.csv",
            Multiselect = true
        };

        if (ofd.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        foreach (string file in ofd.FileNames)
        {
            try
            {
                WorkOrder.ImportVehiclesFromFile(file);
            }

            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(
                    $"An error occurred while importing \"{Path.GetFileName(file)}\".{Environment.NewLine}{Environment.NewLine}{ex.Message}{Environment.NewLine}{Environment.NewLine}Vehicles may be missing from this invoice.{Environment.NewLine}{Environment.NewLine}Do you want to continue processing remaining dealers?",
                    "Hey, listen!",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                    );

                if (result == DialogResult.Yes)
                {
                    continue;
                }

                else break;
            }

            finally
            {
                updateStagedInvoices();
            }
        }
    }

    // Handles clicking the Generate button.
    private void buttonGenerate_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog fbd = new FolderBrowserDialog
        {
            Description = "Destination where Invoices should be saved."
        };

        if (fbd.ShowDialog() == DialogResult.OK)
        {
            foreach (Dealer d in Dealer.StagedDealers)
            {
                d.GenerateInvoice(fbd.SelectedPath, dateTimePicker.Value);
            }

            MessageBox.Show("All queued dealers have been processed.");
        }
    }
}
