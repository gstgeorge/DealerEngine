using System;
using System.Drawing;
using System.Windows.Forms;

namespace DealerEngine;

public partial class FormAddDealerToQueue : Form
{
    private const int MARGIN = 10;
    
    Panel panelBottomMenuBar;
    CheckedListBox clbUnqueuedDealers;

    public FormAddDealerToQueue()
    {
        InitializeComponent();

        DrawForm();
    }



    // Draw form controls
    private void DrawForm()
    {
        SuspendLayout();

        Controls.Clear();

        Text = "Select Dealer(s) to add to the Invoice Queue";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ControlBox = false;

        Size buttonSize = new Size(90, 30);

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
        Button cancelButton = new Button
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
        CancelButton = cancelButton;
        panelBottomMenuBar.Controls.Add(cancelButton);

        // Accept button
        Button acceptButton = new Button
        {
            Size = buttonSize,
            Location = new Point(
                x: cancelButton.Left - MARGIN - buttonSize.Width,
                y: MARGIN),
            Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
            TabIndex = 1,
            Text = "Accept",
            DialogResult = DialogResult.OK
        };
        acceptButton.Click += new EventHandler(acceptButton_Click);
        AcceptButton = acceptButton;
        panelBottomMenuBar.Controls.Add(acceptButton);

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
    private void acceptButton_Click(object sender, EventArgs e)
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
