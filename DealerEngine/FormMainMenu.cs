using DealerEngine.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DealerEngine;

public partial class FormMainMenu : Form
{
    private const int MARGIN = 10;
    private readonly Size buttonSize = new Size(150, 40);

    private Panel panelTopMenuBar;
    private Panel panelBottomMenuBar;
    private Panel panelQueuedDealerInvoiceTotals;

    private Button buttonImport;
    private Button buttonAddDealerToQueue;
    private Button buttonGenerate;
    private Button buttonClearQueue;

    private GroupBox gbInvoiceDate;
    private DateTimePicker dateTimePicker;

    private DataGridView dgvQueuedDealers;

    private Label labelVehicleCountAllQueuedDealers;
    private Label labelVehicleCountAllQueuedDealersValue;
    private Label labelTotalDueAllQueuedDealers;
    private Label labelTotalDueAllQueuedDealersValue;

    // Constructor
    public FormMainMenu()
    {
        InitializeComponent();

        DrawForm();
    }



    // Draw form controls.
    private void DrawForm()
    {
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(buttonSize.Width * 5 + MARGIN * 6, 600);
        MinimumSize = Size;
        BackColor = Color.FromArgb(240, 240, 240);
        Icon = Properties.Resources.icon_dealerengine;
        Text = $"{Application.ProductName} {Application.ProductVersion}";

        SuspendLayout();
                
        Controls.Clear();

        #region top menu bar

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

        // Import button
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

        // Add dealer to queue button
        buttonAddDealerToQueue = new Button
        {
            Size = buttonSize,
            Location = new Point(buttonImport.Right + MARGIN, MARGIN),
            Anchor = (AnchorStyles.Top | AnchorStyles.Left),
            TabIndex = 1,
            Text = "Add Dealer to Queue",
            Image = Properties.Resources.img_new,
            TextImageRelation = TextImageRelation.ImageBeforeText
        };
        buttonAddDealerToQueue.Click += new EventHandler(buttonAddDealerToQueue_Click);
        panelTopMenuBar.Controls.Add(buttonAddDealerToQueue);

        // Clear queue button
        buttonClearQueue = new Button
        {
            Size = buttonSize,
            Location = new Point(buttonAddDealerToQueue.Right + MARGIN, MARGIN),
            Anchor = (AnchorStyles.Top | AnchorStyles.Left),
            TabIndex = 2,
            Text = "Clear Queue",
            Image = Resources.img_remove,
            TextImageRelation = TextImageRelation.ImageBeforeText,
        };
        buttonClearQueue.Click += new EventHandler(buttonClearQueue_Click);
        panelTopMenuBar.Controls.Add(buttonClearQueue);

        Controls.Add(panelTopMenuBar);

        #endregion

        #region bottom menu bar

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
            TabIndex = 3,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "MM/dd/yyyy"
        };
        gbInvoiceDate.Controls.Add(dateTimePicker);

        // Queued dealer invoice totals
        panelQueuedDealerInvoiceTotals = new Panel()
        {
            //Enabled = false
        };

        labelVehicleCountAllQueuedDealers = new Label
        {
            Size = new Size(120, 20),
            Location = new Point(0, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Vehicle Count"
        };
        panelQueuedDealerInvoiceTotals.Controls.Add(labelVehicleCountAllQueuedDealers);

        labelTotalDueAllQueuedDealers = new Label
        {
            Size = new Size(120, 20),
            Location = new Point(0, labelVehicleCountAllQueuedDealers.Bottom),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Total Sales"
        };
        panelQueuedDealerInvoiceTotals.Controls.Add(labelTotalDueAllQueuedDealers);

        labelVehicleCountAllQueuedDealersValue = new Label
        {
            Size = new Size(120, 20),
            Location = new Point(labelVehicleCountAllQueuedDealers.Right, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = ""
        };
        panelQueuedDealerInvoiceTotals.Controls.Add(labelVehicleCountAllQueuedDealersValue);

        labelTotalDueAllQueuedDealersValue = new Label
        {
            Size = new Size(120, 20),
            Location = new Point(labelVehicleCountAllQueuedDealersValue.Left, labelVehicleCountAllQueuedDealersValue.Bottom),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = ""
        };
        panelQueuedDealerInvoiceTotals.Controls.Add(labelTotalDueAllQueuedDealersValue);

        panelQueuedDealerInvoiceTotals.Size = new Size(
            width: labelTotalDueAllQueuedDealersValue.Right, 
            height: labelTotalDueAllQueuedDealersValue.Bottom);

        panelQueuedDealerInvoiceTotals.Location = new Point(
            x: (panelBottomMenuBar.Width / 2) - (panelQueuedDealerInvoiceTotals.Width / 2),
            y: (panelBottomMenuBar.Height / 2) - (panelQueuedDealerInvoiceTotals.Height / 2));

        panelBottomMenuBar.Controls.Add(panelQueuedDealerInvoiceTotals);

        // Generate invoices button
        buttonGenerate = new Button
        {
            Size = buttonSize,
            Location = new Point(panelBottomMenuBar.Right - buttonSize.Width - MARGIN, MARGIN),
            Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
            TabIndex = 4,
            Text = "Generate",
            Image = Properties.Resources.img_download,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Enabled = false
        };
        buttonGenerate.Click += new EventHandler(buttonGenerate_Click);
        panelBottomMenuBar.Controls.Add(buttonGenerate);

        Controls.Add(panelBottomMenuBar);

        #endregion

        #region dealer queue

        // Queued dealers
        dgvQueuedDealers = new DataGridView
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
        dgvQueuedDealers.EnableHeadersVisualStyles = false;
        dgvQueuedDealers.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgvQueuedDealers.ColumnHeadersDefaultCellStyle.BackColor;

        // Queued dealer columns
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

        dgvQueuedDealers.Columns.AddRange(
        [
            colName,
            colWorkOrderCount,
            colVehicleCount,
            colTotalOTLCharges,
            colTotalMonthlyCharges,
            colTotalDue
        ]);

        Controls.Add(dgvQueuedDealers);

        #endregion
        
        ResumeLayout();

        updateQueuedDealers();
    }

    // Updates the Queued Dealers displayed in the DataGridView,
    // and the totals in the bottom panel.
    private void updateQueuedDealers()
    {
        SuspendLayout();

        var queuedDealers = Dealer.QueuedDealers;

        dgvQueuedDealers.DataSource = queuedDealers;
        dgvQueuedDealers.Refresh();

        labelVehicleCountAllQueuedDealersValue.Text = Dealer.QueuedDealersVehicleCount.ToString();
        labelTotalDueAllQueuedDealersValue.Text = Dealer.QueuedDealersTotalDue.ToString("c");

        bool hasQueuedDealers = queuedDealers.Length > 0;

        buttonClearQueue.Enabled = hasQueuedDealers;
        buttonGenerate.Enabled = hasQueuedDealers;

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
                updateQueuedDealers();
            }
        }
    }

    // Handles clicking the Add Dealer to Queue button.
    private void buttonAddDealerToQueue_Click(object sender, EventArgs e)
    {
        using Form addDealer = new FormAddDealerToQueue();

        if (addDealer.ShowDialog() == DialogResult.OK)
        {
            updateQueuedDealers();
        }

    }

    // Handles clicking the Clear Queue button.
    private void buttonClearQueue_Click(object sender, EventArgs e)
    {
        foreach (Dealer d in Dealer.QueuedDealers)
        {
            d.WorkOrders.Clear();
            d.Queued = false;
        }

        updateQueuedDealers();
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
            foreach (Dealer d in Dealer.QueuedDealers)
            {
                d.GenerateInvoice(fbd.SelectedPath, dateTimePicker.Value);
            }

            MessageBox.Show("All queued dealers have been processed.");
        }
    }
}
