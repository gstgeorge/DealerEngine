using GenericParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace DealerEngine;

/// <summary>
/// Data structure to hold a collection of vehicles which were serviced on a particular day.
/// </summary>
internal class WorkOrder
{
    private SortedSet<Vehicle> _vehicles { get; } = new SortedSet<Vehicle>();
        


    /// <summary>
    /// The sum of the charges for each vehicle on this work order.
    /// </summary>
    public decimal Total { get => Vehicles.Sum(x => x.Price); }

    /// <summary>
    /// Vehicles belonging to the Work Order.
    /// </summary>
    public Vehicle[] Vehicles { get => _vehicles.ToArray(); }



    /// <summary>
    /// Parse one or more files, adding each vehicle to the dealer's work orders.
    /// </summary>
    /// <param name="path">The path of the file to process.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void ImportVehiclesFromFile(string path)
    {
        try
        {
            // Process the file based on the filetype
            switch (Path.GetExtension(path).ToLower())
            {
                case ".csv":
                    ProcessCSV(path);
                    break;

                // TODO: add xlsx support

                default: // File is not of a supported format.
                    throw new InvalidOperationException("File is not a supported type.");
            }
        }

        catch (Exception e)
        {
            throw e;
        }
    }

    /// <summary>
    /// Process a CSV file of vehicle data, adding each vehicle to the dealer's work orders.
    /// </summary>
    /// <param name="path">The path of the file to process.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ProcessCSV(string path)
    {
        DataTable dt = new DataTable();

        // Parse file as DataTable
        using (GenericParserAdapter parser = new GenericParserAdapter(path)) // TODO: handle file in use
        {
            try
            {
                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                parser.SkipStartingDataRows = 0;
                parser.MaxBufferSize = 4096;
                parser.MaxRows = 1000;
                dt = parser.GetDataTable();
            }

            catch (Exception e)
            {
                throw new InvalidOperationException($"Parsing error.", e);
            }
        }

        // Identify the provider based on the number of columns, and set the appropriate column names.
        string
            dateCol,
            dealerNameCol,
            stockCol,
            conditionCol,
            priceCol,
            vinCol = null,
            yearCol = null,
            makeCol = null,
            modelCol = null,
            descCol = null;

        switch (dt.Columns.Count)
        {
            case 21: // Autouplink Tech
                dateCol = "Service Date/Time";
                dealerNameCol = "Dealer Name";
                stockCol = "Stock Number";
                conditionCol = "Vehicle Stock Type";
                priceCol = "Service Type Price";
                vinCol = "VIN";
                yearCol = "Model Year";
                makeCol = "Make";
                modelCol = "Model";
                break;

            case 7: // Custom Template
                dateCol = "Service Date";
                dealerNameCol = "Dealer";
                stockCol = "Stock Number";
                conditionCol = "Stock Type";
                priceCol = "Price";
                descCol = "Description";
                break;

            default:
                throw new InvalidOperationException("Data source is unsupported or cannot be determined.");
        }

        // Process each vehicle
        foreach (DataRow row in dt.Rows)
        {
            Vehicle v = new Vehicle(
                row[stockCol].ToString(),
                row[conditionCol].ToString(),
                decimal.Parse(row[priceCol].ToString()),
                vinCol   == null ? null : row[vinCol].ToString(),
                yearCol  == null ? null : Convert.ToUInt16(row[yearCol]),
                makeCol  == null ? null : row[makeCol].ToString(),
                modelCol == null ? null : row[modelCol].ToString(),
                descCol  == null ? null : row[descCol].ToString()
                );

            DateTime date = DateTime.Parse(row[dateCol].ToString());

            try
            {
                AddVehicleToWorkOrder(row[dealerNameCol].ToString(), date, v);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to add vehicle to work order. {e.Message}", e);
            }
        }
    }

    /// <summary>
    /// Add a vehicle to the work order of a specified dealer.
    /// </summary>
    /// <param name="dealerName">The dealer's name.</param>
    /// <param name="date">The date of the workorder.</param>
    /// <param name="vehicle">The vehicle to add.</param>
    /// <exception cref="ArgumentException">Dealer has not been previously configured.</exception>
    private static void AddVehicleToWorkOrder(string dealerName, DateTime date, Vehicle vehicle)
    {
        // Lookup dealer config.
        // If dealer config does not exist, raise exception.
        var dealer = Dealer.SavedDealers.FirstOrDefault(x => x.Name == dealerName) 
            ?? throw new ArgumentException($"No dealer configuration found.");

        // If there is no work order for the given date, create one.
        if (dealer.WorkOrders.ContainsKey(date.Date) == false)
        {
            dealer.WorkOrders.Add(date.Date, new WorkOrder());
        }

        // Add the vehicle to the workorder
        // TODO: If the vehicle already exists on the workorder, log this occurrence 
        if (dealer.WorkOrders[date.Date]._vehicles.Add(vehicle) == false)
        {
            return;
        }

        dealer.Staged = true;
    }
}
