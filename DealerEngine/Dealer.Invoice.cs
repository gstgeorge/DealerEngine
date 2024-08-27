using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DealerEngine;

internal partial class Dealer
{
    /// <summary>
    /// Generate an invoice for this dealer.
    /// </summary>
    /// <param name="outputDir">The path where the file should be created.</param>
    /// <param name="reportDate">The date to display on the invoice</param>
    public void GenerateInvoice(string outputDir, DateTime reportDate)
    {
        Invoice.Generate(this, outputDir, reportDate);
    }


    // Private implementation of GenerateInvoice
    private static class Invoice
    {
        private const double MARGIN          = 40;

        private const double LINE_HEIGHT     = 15;
        private const double LG_LINE_HEIGHT  = 18;
        private const double SM_LINE_HEIGHT  = 12;

        private const double SPACER      = LINE_HEIGHT;
        private const double SM_SPACER   = LG_LINE_HEIGHT / 2;

        private const int CORNER_RADIUS = 3;

        private readonly static XFont fontStd           = new XFont("Arial", 10);
        private readonly static XFont fontBold          = new XFont("Arial", 10, XFontStyle.Bold);
        private readonly static XFont fontHeader        = new XFont("Arial", 13);
        private readonly static XFont fontHeaderBold    = new XFont("Arial", 13, XFontStyle.Bold);
        private readonly static XFont fontSmall         = new XFont("Arial", 8);

        private static double marginL;
        private static double marginR;
        private static double marginT;
        private static double marginB;

        private static double yPos;
        private static double colSize;

        private static Dealer dealer;

        private static PdfDocument doc;
        private static XGraphics gfx;

        private static XBrush accentColorBrush;


        /// <summary>
        /// Generate an invoice
        /// </summary>
        /// <param name="dealer">The dealer to generate an invoice for</param>
        /// <param name="outputDir">The path where the file should be created</param>
        /// <param name="reportDate">The date to display on the invoice</param>
        public static void Generate(Dealer dealer, string outputDir, DateTime reportDate)
        {
            Invoice.dealer = dealer;

            string fileName = $"{Invoice.dealer.FileName}_{reportDate:yyyy-MM-dd}";

            doc = new PdfDocument(Path.Combine(outputDir, fileName + ".pdf")); //TODO: handle ioex when file exists and is open
            doc.Info.Title = $"{Settings.BusinessName} Invoice".Trim();

            PdfPage page = doc.AddPage();
            page.Size = PdfSharp.PageSize.Letter;

            gfx = XGraphics.FromPdfPage(page);

            accentColorBrush = new XSolidBrush(
                XColor.FromArgb(
                    Settings.InvoiceAccentColor.R, 
                    Settings.InvoiceAccentColor.G, 
                    Settings.InvoiceAccentColor.B
                ));

            marginL = MARGIN;
            marginR = gfx.PageSize.Width - MARGIN;
            marginT = MARGIN;
            marginB = gfx.PageSize.Height - MARGIN - LINE_HEIGHT; // Page num will print directly below this

            yPos = marginT;
            colSize = (marginR - marginL) / 10;

            // Print the invoice containing all charges
            PrintMainHeader(
                reportDate,
                dealer.TotalInvoiceAmount, 
                isWorkSummary: false
                );

            PrintInvoiceLineItems();

            // Print each work summary
            foreach (KeyValuePair<DateTime, WorkOrder> kvp in Invoice.dealer.WorkOrders)
            {
                AddPage();
                PrintMainHeader(kvp.Key, kvp.Value.Total, isWorkSummary: true);
                PrintWorkSummaryLineItems(kvp.Value);
            }

            PrintPageNumbers();

            doc.Close();
        }

        /// <summary>
        /// Prints the page header and sets <see cref="yPos"/> to the next line.
        /// </summary>
        /// <param name="date">The date to print on this page.</param>
        /// <param name="total">The total amount billed for this page.</param>
        /// <param name="isWorkSummary">Page is a Work Summary, and not an Invoice.</param>
        private static void PrintMainHeader(DateTime date, decimal total, bool isWorkSummary = false)
        {
            /**
             * PRINT FIRST LINE OF HEADER
             * 
             *  |----------------------------------------------------------|
             *  |   --------------     CONTACT LINE 1                      |
             *  |  |              |    CONTACT LINE 2           INVOICE    |
             *  |  |     LOGO     |    CONTACT LINE 3             OR       |
             *  |  |              |    CONTACT LINE 4         WORK SUMMARY |
             *  |   --------------     CONTACT LINE 5                      |
             *  |                                                          |
             *  |                            ...                           |
             */

            double sectionHeight = LG_LINE_HEIGHT + (SM_LINE_HEIGHT * 5);

            if (Settings.DebugGrid)
            {
                for (double i = marginL; i <= marginR; i += colSize)
                {
                    gfx.DrawLine(XPens.LightGray, i, marginT, i, marginB + LG_LINE_HEIGHT);
                }
                gfx.DrawLine(XPens.LightGray, marginL, yPos, marginR, yPos);
                gfx.DrawLine(XPens.LightGray, marginL, yPos + sectionHeight, marginR, yPos + sectionHeight);
            }

            double xPosContactInfo = marginL;
            double logoAreaWidth = colSize * 3;

            // If a company logo has been set, display that image
            // Set the X Position of the company contact info to the right of the logo
            if (Settings.Logo is not null)
            {
                XImage logo = XImage.FromFile(Settings.Logo);

                double logoAspectRatio = logo.Size.Width / logo.Size.Height;

                // Will the width of the logo exceed its boundary?
                bool scaleLogo = sectionHeight * logoAspectRatio > logoAreaWidth;

                // Get the dimensions of the logo
                double logoHeight = scaleLogo ? logoAreaWidth / logoAspectRatio : sectionHeight;
                double logoWidth = scaleLogo ? logoAreaWidth : logoHeight * logoAspectRatio;

                gfx.DrawImage(logo, marginL, marginT + ((sectionHeight - logoHeight) / 2), logoWidth, logoHeight);

                // Move X position to the right of the logo in order to print contact info
                xPosContactInfo = marginL + logoWidth + SPACER;
            }

            // Print the business name and contact info to the right of the logo
            if (!string.IsNullOrEmpty(Settings.BusinessName))
            {
                // Print the name of the business
                gfx.DrawString(
                    Settings.BusinessName, 
                    fontBold, 
                    XBrushes.Black,
                    new XRect(xPosContactInfo, yPos, logoAreaWidth, LINE_HEIGHT), 
                    XStringFormats.CenterLeft);

                yPos += LINE_HEIGHT;
            }

            foreach (string s in Settings.BusinessContactInfo)
            {
                // Print the address/contact info of the business
                gfx.DrawString(
                    s, 
                    fontSmall, 
                    XBrushes.Black,
                    new XRect(xPosContactInfo, yPos, logoAreaWidth - SPACER, SM_LINE_HEIGHT), 
                    XStringFormats.CenterLeft);

                yPos += SM_LINE_HEIGHT;
            }

            // Print an appropriate page title based on whether this is a Work Summary or an Invoice
            yPos = marginT;

            if (isWorkSummary)
            {
                XFont titleFont = new XFont("Arial", 28, XFontStyle.Bold);

                gfx.DrawString(
                    "WORK", 
                    titleFont, 
                    accentColorBrush,
                    new XRect(marginL + colSize * 7, yPos, colSize * 3, sectionHeight / 2), 
                    XStringFormats.CenterRight);

                yPos += sectionHeight / 2;

                gfx.DrawString(
                    "SUMMARY", 
                    titleFont, 
                    accentColorBrush,
                    new XRect(marginL + colSize * 7, yPos, colSize * 3, sectionHeight / 2), 
                    XStringFormats.CenterRight);
            }

            else // is invoice
            {
                XFont titleFont = new XFont("Arial", 38, XFontStyle.Bold);

                gfx.DrawString(
                    "INVOICE", 
                    titleFont, 
                    accentColorBrush,
                    new XRect(marginL + colSize * 7, yPos, colSize * 3, sectionHeight), 
                    XStringFormats.CenterRight);
            }

            // Set the Y Position to below the first line of the header
            yPos = marginT + sectionHeight + SPACER;


            /**
             * PRINT SECOND LINE OF HEADER
             * 
             * |                           ...                                 |
             * |    ________________           __________        __________    |
             * |   |    BILL TO:    |         |   TERMS  |      |   DATE   |   |
             * |   |----------------|         |----------|      |----------|   |
             * |   |  DEALER NAME   |         |  NET XX  |      |MM/DD/YYYY|   |
             * |   |                |          ----------        ----------    |
             * |   |  DEALER ADDR   |          ----------------------------    |
             * |   |                |         |         PLEASE PAY         |   |
             * |   |                |         |----------------------------|   |
             * |   |                |         |           $TOTAL           |   |
             * |    ----------------           ----------------------------    |
             * |                           ...                                 |
             */

            sectionHeight = LG_LINE_HEIGHT * 4.5 + SM_SPACER;

            if (Settings.DebugGrid)
            {
                gfx.DrawLine(XPens.LightGray, marginL, yPos, marginR, yPos);
                gfx.DrawLine(XPens.LightGray, marginL, yPos + sectionHeight, marginR, yPos + sectionHeight);
            }

            double xPosBillTo   = marginL;
            double widthBillTo  = colSize * 4;
            double xPosDate     = colSize * 8 + marginL;
            double widthDate    = colSize * 2;
            double xPosTerms    = colSize * 5 + marginL;
            double widthTerms   = colSize * 2;
            double xPosTotal    = xPosTerms;
            double widthTotal   = colSize * 5;

            XRect rect;

            // Print the "Bill To" header and bounding box
            rect = new XRect(xPosBillTo, yPos, widthBillTo, LG_LINE_HEIGHT);
            gfx.DrawRoundedRectangle(accentColorBrush, rect, new XSize(CORNER_RADIUS, CORNER_RADIUS));
            gfx.DrawString(isWorkSummary ? "Client" : "Bill To", fontHeader, XBrushes.White, rect, XStringFormats.Center);
            gfx.DrawRoundedRectangle(XPens.Black, xPosBillTo, yPos, widthBillTo, sectionHeight, CORNER_RADIUS, CORNER_RADIUS);

            // If this is the invoice page, print the "Terms" header and bounding box
            if (!isWorkSummary && Settings.InvoiceTerms is not null)
            {
                rect = new XRect(xPosTerms, yPos, widthTerms, LG_LINE_HEIGHT);
                gfx.DrawRoundedRectangle(accentColorBrush, rect, new XSize(CORNER_RADIUS, CORNER_RADIUS));
                gfx.DrawString("Terms", fontHeader, XBrushes.White, rect, XStringFormats.Center);
                gfx.DrawRoundedRectangle(XPens.Black, xPosTerms, yPos, widthTerms, LG_LINE_HEIGHT * 2, CORNER_RADIUS, CORNER_RADIUS);
            }

            // Print the "Date" header and bounding box
            rect = new XRect(xPosDate, yPos, widthDate, LG_LINE_HEIGHT);
            gfx.DrawRoundedRectangle(accentColorBrush, rect, new XSize(CORNER_RADIUS, CORNER_RADIUS));
            gfx.DrawString("Date", fontHeader, XBrushes.White, rect, XStringFormats.Center);
            gfx.DrawRoundedRectangle(XPens.Black, xPosDate, yPos, widthDate, LG_LINE_HEIGHT * 2, CORNER_RADIUS, CORNER_RADIUS);

            yPos += LG_LINE_HEIGHT;

            // Print the dealer's name and address inside the "Bill To" box
            double dealerContactHeight = LG_LINE_HEIGHT + SM_SPACER + (dealer.Address.Length * SM_LINE_HEIGHT);
            double yPosDealerContact = yPos + ((sectionHeight - LG_LINE_HEIGHT - dealerContactHeight) / 2);
            
            gfx.DrawString(
                dealer.Name, 
                fontHeader,
                XBrushes.Black,
                new XRect(xPosBillTo, yPosDealerContact, widthBillTo, LG_LINE_HEIGHT), 
                XStringFormats.Center
                );

            yPosDealerContact += LG_LINE_HEIGHT + (SM_SPACER / 2);

            foreach (string s in dealer.Address)
            {
                gfx.DrawString(
                    s,
                    fontSmall, 
                    XBrushes.Black,
                    new XRect(xPosBillTo, yPosDealerContact, widthBillTo, SM_LINE_HEIGHT), 
                    XStringFormats.Center
                    );

                yPosDealerContact += SM_LINE_HEIGHT;
            }

            // If this is the invoice page, print the payment terms
            if (!isWorkSummary && Settings.InvoiceTerms is not null)
            {
                rect = new XRect(xPosTerms, yPos, widthTerms, LG_LINE_HEIGHT);
                gfx.DrawString(
                    Settings.InvoiceTerms, 
                    fontHeader,
                    XBrushes.Black, 
                    rect, 
                    XStringFormats.Center
                    );
            }

            // Print the date
            rect = new XRect(xPosDate, yPos, widthDate, LG_LINE_HEIGHT);
            gfx.DrawString($"{date:MM/dd/yyyy}", fontHeader, XBrushes.Black, rect, XStringFormats.Center);

            yPos += LG_LINE_HEIGHT + SM_SPACER;

            // Print total header and bounding box
            rect = new XRect(xPosTotal, yPos, widthTotal, LG_LINE_HEIGHT);

            gfx.DrawRoundedRectangle(accentColorBrush, rect, new XSize(CORNER_RADIUS, CORNER_RADIUS));

            gfx.DrawString(
                isWorkSummary ? "TOTAL" : "PLEASE PAY", 
                fontHeaderBold, 
                XBrushes.White, 
                rect, 
                XStringFormats.Center);

            gfx.DrawRoundedRectangle(XPens.Black, xPosTotal, yPos, widthTotal, LG_LINE_HEIGHT * 2.5, CORNER_RADIUS, CORNER_RADIUS);

            yPos += LG_LINE_HEIGHT;

            // Print the total
            gfx.DrawString(
                total.ToString("C"),
                fontHeaderBold, 
                XBrushes.Black,
                new XRect(xPosTotal, yPos, widthTotal, LG_LINE_HEIGHT * 1.5), 
                XStringFormats.Center
                );

            // Set Y Position to below the main header
            yPos += (LG_LINE_HEIGHT * 1.5) + SPACER;
        }

        /// <summary>
        /// Print the header row background color and bounding box for invoice/work summary content.
        /// </summary>
        private static void PrintContentHeader()
        {
            // Print the background for the header row
            gfx.DrawRoundedRectangle(
                accentColorBrush,
                new XRect(marginL, yPos, colSize * 10, LG_LINE_HEIGHT),
                new XSize(CORNER_RADIUS, CORNER_RADIUS)
                );

            // Print the bounding box for all invoice line items on this page
            gfx.DrawRoundedRectangle(
                XPens.Black,
                new XRect(
                    marginT,
                    yPos,
                    colSize * 10,
                    (Math.Floor((marginB - yPos - LG_LINE_HEIGHT) / LINE_HEIGHT) * LINE_HEIGHT) + LG_LINE_HEIGHT
                    ),
                new XSize(CORNER_RADIUS, CORNER_RADIUS)
                );
        }

        /// <summary>
        /// Generates line items for the invoice page.
        /// </summary>
        private static void PrintInvoiceLineItems()
        {
            PrintInvoiceLineItem(printHeader: true);

            // Print all Work Orders as line items
            foreach (KeyValuePair<DateTime, WorkOrder> kvp in dealer.WorkOrders)
            {
                PrintInvoiceLineItem($"On-the-lot Services ({kvp.Key:MM/dd})", kvp.Value.Total);
            }

            // Print all enabled monthly charges
            foreach (Charge c in dealer.MonthlyCharges.Where(x => x.Enabled))
            {
                if (yPos + LINE_HEIGHT > marginB)
                {
                    AddPage();
                    PrintInvoiceLineItem(printHeader: true);
                }

                PrintInvoiceLineItem(c.Name, dealer.CalculateMonthlyCharge(c));
            }
        }

        /// <summary>
        /// Prints a line item for the invoice page
        /// </summary>
        /// <param name="description">The description of the line item.</param>
        /// <param name="price">The amount to charge for the line item.</param>
        /// <param name="printHeader">Print the header row and bounding box.</param>
        /// <exception cref="ArgumentException">No line item charge has been specified, and the header flag is not set.</exception>
        private static void PrintInvoiceLineItem(string description=null, decimal price=0, bool printHeader=false)
        {
            // If no line item has been supplied, and this is not a header, throw exception
            if (description == null && !printHeader) throw new ArgumentException("No line item charge has been specified, and the header flag is not set.");

            double xPosDesc     = marginL;
            double widthDesc    = colSize * 8;
            double xPosPrice    = xPosDesc + widthDesc;
            double widthPrice   = colSize * 2;

            if (printHeader)
            {
                PrintContentHeader();

                // Description header
                gfx.DrawString(
                    "Description",
                    fontHeader,
                    XBrushes.White,
                    new XRect(xPosDesc, yPos, widthDesc, LG_LINE_HEIGHT),
                    XStringFormats.Center
                    );

                // Price header
                gfx.DrawString(
                    "Price",
                    fontHeader,
                    XBrushes.White,
                    new XRect(xPosPrice, yPos, widthPrice, LG_LINE_HEIGHT),
                    XStringFormats.Center
                    );
            }

            else 
            {
                // Description
                gfx.DrawString(
                    description,
                    fontStd, 
                    XBrushes.Black,
                    new XRect(xPosDesc, yPos, widthDesc, LINE_HEIGHT), 
                    XStringFormats.Center
                    );

                // Price
                gfx.DrawString(
                    price.ToString("C"),
                    fontStd,
                    XBrushes.Black,
                    new XRect(xPosPrice, yPos, widthPrice - colSize * 0.6, LINE_HEIGHT),
                    XStringFormats.CenterRight
                    );
            }

            // Set Y position to the next line
            yPos += printHeader ? LG_LINE_HEIGHT : LINE_HEIGHT;
        }
    
        /// <summary>
        /// Generates line items for Work Summaries.
        /// </summary>
        /// <param name="w">The Work Summary to print.</param>
        private static void PrintWorkSummaryLineItems(WorkOrder w)
        {
            PrintWorkSummaryLineItem(printHeader: true);
            
            // Print each vehicle as a line item
            foreach (Vehicle v in w.Vehicles)
            {
                if (yPos + LINE_HEIGHT > marginB)
                {
                    AddPage();
                    PrintWorkSummaryLineItem(printHeader: true);
                }

                PrintWorkSummaryLineItem(v);
            }
        }

        /// <summary>
        /// Prints a line item for Work Summary pages.
        /// </summary>
        /// <param name="v">The vehicle listed in this line item.</param>
        /// <param name="printHeader">Print the header row and bounding box.</param>
        /// <exception cref="ArgumentException">No vehicle has been specified, and the header flag is not set.</exception>
        private static void PrintWorkSummaryLineItem(Vehicle v=null, bool printHeader=false)
        {
            // If no line item has been supplied, and this is not a header, throw exception
            if (v == null && !printHeader) throw new ArgumentException("No vehicle has been specified, and the header flag is not set.");

            double xPosStock    = marginL;
            double widthStock   = colSize * 1.5;
            double xPosCond     = xPosStock + widthStock;
            double widthCond    = colSize * 1.5;
            double xPosDesc     = xPosCond + widthCond;
            double widthDesc    = colSize * 5;
            double xPosPrice    = xPosDesc + widthDesc;
            double widthPrice   = colSize * 2;

            if (printHeader)
            {
                PrintContentHeader();

                // Stock header
                gfx.DrawString(
                    "Stock #",
                    fontHeader,
                    XBrushes.White,
                    new XRect(xPosStock, yPos, widthStock, LG_LINE_HEIGHT),
                    XStringFormats.Center
                    );

                // Condition header
                gfx.DrawString(
                    "Condition",
                    fontHeader,
                    XBrushes.White,
                    new XRect(xPosCond, yPos, widthCond, LG_LINE_HEIGHT),
                    XStringFormats.Center
                    );

                // Description header
                gfx.DrawString(
                    "Description",
                    fontHeader,
                    XBrushes.White,
                    new XRect(xPosDesc, yPos, widthDesc, LG_LINE_HEIGHT),
                    XStringFormats.Center
                    );

                // Price header
                gfx.DrawString(
                    "Price",
                    fontHeader,
                    XBrushes.White,
                    new XRect(xPosPrice, yPos, widthPrice, LG_LINE_HEIGHT),
                    XStringFormats.Center
                    );
            }

            else
            {
                // Stock
                gfx.DrawString(
                    v.Stock,
                    fontStd,
                    XBrushes.Black,
                    new XRect(xPosStock, yPos, widthStock, LINE_HEIGHT),
                    XStringFormats.Center
                    );

                // Condition
                gfx.DrawString(
                    v.Cond.ToString(),
                    fontStd,
                    XBrushes.Black,
                    new XRect(xPosCond, yPos, widthCond, LINE_HEIGHT),
                    XStringFormats.Center
                    );

                // Description
                gfx.DrawString(
                    v.Description,
                    fontStd,
                    XBrushes.Black,
                    new XRect(xPosDesc, yPos, widthDesc, LINE_HEIGHT),
                    XStringFormats.Center
                    );

                // Price
                gfx.DrawString(
                    v.Price.ToString("C"),
                    fontStd,
                    XBrushes.Black,
                    new XRect(xPosPrice, yPos, widthPrice - colSize * 0.6, LINE_HEIGHT),
                    XStringFormats.CenterRight
                    );
            }

            // Set Y position to the next line
            yPos += printHeader ? LG_LINE_HEIGHT : LINE_HEIGHT;
        }

        /// <summary>
        /// Adds a page to the current document, and resets the Y Position to the top margin.
        /// </summary>
        private static void AddPage()
        {
            gfx.Dispose();
            gfx = XGraphics.FromPdfPage(doc.AddPage());
            yPos = marginT;
        }

        /// <summary>
        /// Print the page numbers below the bottom margin of each page in the document.
        /// </summary>
        private static void PrintPageNumbers()
        {
            int pageCount = doc.PageCount;
            int pageNum = 1;
            
            gfx.Dispose();

            foreach (PdfPage p in doc.Pages)
            {
                using XGraphics g = XGraphics.FromPdfPage(p);
                
                g.DrawString(
                    $"Page {pageNum++} of {pageCount}",
                    fontStd,
                    XBrushes.Black,
                    new XRect(marginL, marginB, marginR - marginL, LINE_HEIGHT),
                    XStringFormats.BottomCenter
                    );
            }
        }
    }
}
