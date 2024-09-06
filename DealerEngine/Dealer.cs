using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DealerEngine;

/// <summary>
/// Data structure to store dealer info including monthly charges and charges for vehicles serviced.
/// </summary>
internal partial class Dealer : IComparable<Dealer>
{
    private static SortedSet<Dealer> _savedDealers;

    private string _name;
    private string[] _address;



    // Static constructor
    static Dealer()
    {
        LoadDealerConfigs();
    }

    /// <summary>
    /// Initializes a new instance of the Dealer class.
    /// </summary>
    /// <param name="name">The dealer's name.</param>
    public Dealer(string name)
    {
        Name = name;
        _address = [];
        MonthlyCharges = new List<Charge>();
        WorkOrders = new SortedDictionary<DateTime, WorkOrder>();
        Active = true;
        Queued = false;
    }


    
    // Implement IComparable
    public int CompareTo(Dealer other)
    {
        return Name.ToLower().CompareTo(other.Name.ToLower());
    }

    // Override ToString()
    public override string ToString()
    {
        return Name;
    }



    /// <summary>
    /// A collection of dealers which are marked as active and queued for invoicing.
    /// </summary>
    public static Dealer[] QueuedDealers
    {
        get => _savedDealers.Where(x => x.Active && x.Queued).ToArray();
    }

    /// <summary>
    /// A collection of dealers which are marked as active and not queued for invoicing.
    /// </summary>
    public static Dealer[] UnQueuedDealers
    {
        get => _savedDealers.Where(x => x.Active && !x.Queued).ToArray();
    }

    /// <summary>
    /// The total amount due from all queued dealers.
    /// Includes on-the-lot charges (from work orders) and monthly charges.
    /// </summary>
    public static decimal QueuedDealersTotalDue
    {
        get => QueuedDealers.Sum(x => x.TotalInvoiceAmount);
    }

    /// <summary>
    /// The total number of vehicles processed from all queued dealers.
    /// </summary>
    public static int QueuedDealersVehicleCount
    {
        get  => QueuedDealers.Sum(x => x.VehicleCount);
    }

    /// <summary>
    /// The dealer's business name.
    /// </summary>
    /// <exception cref="ArgumentException">Name already exists or does not contain any valid characters.</exception>
    public string Name
    {
        get => _name;
        set
        {
            // Remove any excess whitespace.
            string trimmed = value?.Trim();

            // If a valid name has not been entered, raise exception.
            if (string.IsNullOrEmpty(trimmed))
            {
                throw new ArgumentException("Dealer name does not contain any valid characters.");
            }

            // If name has not changed, do nothing.
            else if (trimmed == _name) return;

            // If a duplicate dealer was found, raise exception.
            else if (_savedDealers is not null && _savedDealers.Any(dealer => dealer.Name.ToLower() == trimmed.ToLower()))
            {
                throw new ArgumentException($"A dealer named {trimmed} already exists.");
            }

            else _name = trimmed;
        }
    }

    /// <summary>
    /// The dealer's address.
    /// Max 3 lines. Any extra lines will be truncated.
    /// </summary>
    public string[] Address
    {
        get => _address.ToArray();
        set => _address = value?.Where(x => !string.IsNullOrEmpty(x.Trim())).Take(3).ToArray() ?? [];
    }

    /// <summary>
    /// List of monthly charges that should be billed to the dealer.
    /// </summary>
    public List<Charge> MonthlyCharges { get; }

    /// <summary>
    /// Vehicles which have been processed for this dealer.
    /// </summary>
    [JsonIgnore]
    public SortedDictionary<DateTime, WorkOrder> WorkOrders { get; }

    /// <summary>
    /// Dealer is an active client.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Return the total amount due for the dealer.
    /// </summary>
    [JsonIgnore]
    public decimal TotalInvoiceAmount
    {
        get => TotalMonthlyCharges + TotalOTLCharges;
    }

    /// <summary>
    /// Returns the sum of all active monthly charges. 
    /// Monthly charges which are a function of the number of vehicles processed will be calculated.
    /// </summary>
    [JsonIgnore]
    public decimal TotalMonthlyCharges
    {
        get => MonthlyCharges.Where(x => x.Enabled).Sum(y => CalculateMonthlyCharge(y));
    }

    /// <summary>
    /// Returns the sum of all the on-the-lot charges. (Charges from vehicles in work orders)
    /// </summary>
    [JsonIgnore]
    public decimal TotalOTLCharges
    {
        get => WorkOrders.Sum(x => x.Value.Vehicles.Sum(y => y.Price));
    }

    /// <summary>
    /// Returns the total number of work orders for this dealer.
    /// </summary>
    [JsonIgnore]
    public int WorkOrderCount
    {
        get => WorkOrders.Count;
    }

    /// <summary>
    /// Returns the total number of vehicles within this dealer's workorders.
    /// </summary>
    [JsonIgnore]
    public int VehicleCount
    {
        get => WorkOrders.Sum(x => x.Value.Vehicles.Length);
    }

    /// <summary>
    /// Flag to signal whether an invoice should be generated for this dealer.
    /// </summary>
    [JsonIgnore]
    public bool Queued { get; set; }

    /// <summary>
    /// Return the dealer's name without any whitespace or non-alphanumeric characters for use as a filename.
    /// </summary>
    [JsonIgnore]
    public string FileName
    { 
        get =>  Regex.Replace(_name.Replace(' ', '_'), @"\W", ""); 
    }



    /// <summary>
    /// Create a new dealer and save its configuration.
    /// </summary>
    /// <param name="name">The business name of the dealership.</param>
    public static void AddDealer(string name)
    {
        AddDealer(new Dealer(name));
    }

    /// <summary>
    /// Save a dealer's configuration.
    /// </summary>
    /// <param name="dealer">The dealer to add.</param>
    public static void AddDealer(Dealer dealer)
    {
        _savedDealers.Add(dealer);
    }

    /// <summary>
    /// Look up a Dealer by name in the saved configs.
    /// </summary>
    /// <param name="name">The name of the Dealer to look up.</param>
    /// <returns>The requested Dealer if it exists, otherwise null.</returns>
    public static Dealer LookupDealer(string name)
    {
        return _savedDealers.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Write the dealer configs to disk at <see cref="Settings.DEALER_CONFIG_PATH"/>.
    /// </summary>
    public static void SaveDealerConfigs()
    {
        try
        {
            File.WriteAllText(
                Settings.DEALER_CONFIG_PATH,
                JsonConvert.SerializeObject(_savedDealers, Formatting.Indented)
                );
        }

        catch (Exception e)
        {
            // TODO: log failure to save dealer configs
        }
    }


    /// <summary>
    /// Remove the dealer from the list of saved dealers and delete its config file.
    /// </summary>
    public void Delete()
    {
        _savedDealers.Remove(this);
    }

    /// <summary>
    /// Returns the amount due for a monthly charge.
    /// If the monthly charge is a function of the number of vehicles serviced, the 
    /// Work Orders for this dealer will be used to calculate the returned value.
    /// </summary>
    /// <param name="c">The charge to calculate.</param>
    /// <returns>The amount due.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Charge is of an invalid type.</exception>
    public decimal CalculateMonthlyCharge(Charge c)
    {
        return c.Type switch
        {
            Charge.ChargeType.FIXED     => c.Price,
            Charge.ChargeType.USED      => c.Price * WorkOrders.Sum(x => x.Value.Vehicles.Where(y => y.Cond == Vehicle.Condition.Used).Count()),
            Charge.ChargeType.NEW       => c.Price * WorkOrders.Sum(x => x.Value.Vehicles.Where(y => y.Cond == Vehicle.Condition.New).Count()),
            Charge.ChargeType.VEHICLE   => c.Price * WorkOrders.Sum(x => x.Value.Vehicles.Count()),
            _ => throw new ArgumentOutOfRangeException("Invalid ChargeType"),
        };
    }

    /// <summary>
    /// Load saved dealer config from disk at <see cref="Settings.DEALER_CONFIG_PATH"/>.
    /// </summary>
    private static void LoadDealerConfigs()
    {
        if (File.Exists(Settings.DEALER_CONFIG_PATH))
        {
            try
            {
                _savedDealers = JsonConvert.DeserializeObject<SortedSet<Dealer>>(File.ReadAllText(Settings.DEALER_CONFIG_PATH));
            }

            catch (Exception e)
            {
                _savedDealers = new SortedSet<Dealer>();
                return;
                // TODO: log failure to load existing dealer config
            }
        }

        else _savedDealers = new SortedSet<Dealer>();

        SaveDealerConfigs();
    }
}
