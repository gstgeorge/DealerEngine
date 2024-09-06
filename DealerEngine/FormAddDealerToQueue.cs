using System;
using System.Drawing;
using System.Windows.Forms;

namespace DealerEngine;

public partial class FormAddDealerToQueue : Form
{
    private const int MARGIN = 10;
    private readonly Size buttonSize = new Size(90, 30);

    private CheckedListBox clbUnqueuedDealers;
    private Panel panelBottomMenuBar;
    private Button buttonCancel;
    private Button buttonAccept;

    public FormAddDealerToQueue()
    {
        InitializeComponent();

        DrawForm();
    }



    // Init form
    private void DrawForm()
    {
        SuspendLayout();

        Controls.Clear();
        
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Size = new Size(300, 400);
        MinimumSize = Size;
        ControlBox = false;
        Text = "Select Dealer(s) to add to the Invoice Queue";
        
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

        // Cancel button
        buttonCancel = new Button
        {
            Size = buttonSize,
            Location = new Point(
                x: panelBottomMenuBar.Right - buttonSize.Width - MARGIN,
                y: MARGIN),
            Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
            TabIndex = 2,
            Text = "Cancel",
            DialogResult = DialogResult.Cancel
        };
        CancelButton = buttonCancel;
        panelBottomMenuBar.Controls.Add(buttonCancel);

        // Accept button
        buttonAccept = new Button
        {
            Size = buttonSize,
            Location = new Point(
                x: buttonCancel.Left - MARGIN - buttonSize.Width,
                y: MARGIN),
            Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
            TabIndex = 1,
            Text = "Add",
            DialogResult = DialogResult.OK
        };
        buttonAccept.Click += buttonAccept_Click;
        AcceptButton = buttonAccept;
        panelBottomMenuBar.Controls.Add(buttonAccept);

        Controls.Add(panelBottomMenuBar);

        // Dealer list
        clbUnqueuedDealers = new CheckedListBox
        {
            Size = new Size(
                width: ClientSize.Width - MARGIN * 2,
                height: panelBottomMenuBar.Top),
            Location = new Point(
                x: MARGIN,
                y: MARGIN),
            Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom),
            CheckOnClick = true,
        };
        clbUnqueuedDealers.Items.AddRange(Dealer.UnQueuedDealers);
        Controls.Add(clbUnqueuedDealers);
        
        ResumeLayout();
    }

    // Handle clicking the Accept button
    private void buttonAccept_Click(object sender, EventArgs e)
    {
        foreach (var item in clbUnqueuedDealers.CheckedItems)
        {
            if (item is not Dealer) throw new ArgumentException("Item is not a Dealer.");

            (item as Dealer).Queued = true;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
