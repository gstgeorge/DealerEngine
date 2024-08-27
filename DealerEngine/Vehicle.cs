using System;
using System.Text.RegularExpressions;

namespace DealerEngine;

/// <summary>
/// Stores vehicle data for use in creating a line item on a work summary report.
/// </summary>
internal record class Vehicle : IComparable<Vehicle>
{
    /// <summary>
    /// The vehicle's condition.
    /// </summary>
    public enum Condition { Used, New };


    // Private Fields
    private string _vin;
    private string _stock;
    private UInt32? _refno;
    private Condition _cond;
    private ushort? _year;
    private string _make;
    private string _model;
    private string _description;
    private decimal _price;



    /// <summary>
    /// Initializes a new instance of the Vehicle class.
    /// </summary>
    /// <param name="vin">The vehicle's VIN number.</param>
    /// <param name="stock">The vehicle's stock number.</param>
    /// <param name="condition">The vehicle's condition.</param>
    /// <param name="year">The vehicle's model year.</param>
    /// <param name="make">The vehicle's manufacturer.</param>
    /// <param name="model">The vehicle's model name.</param>
    /// <param name="refno">An optional reference number.</param>
    public Vehicle(
        string stock, 
        Condition condition, 
        decimal price, 
        string vin=null, 
        ushort? year = null, 
        string make = null, 
        string model = null, 
        string desc = null, 
        uint? refno = null
        )
    {
        if (string.IsNullOrEmpty(stock))
        {
            throw new ArgumentNullException("Vehicle does not have a stock number");
        }

        _stock = stock;
        _cond = condition;
        _price = price;
        _vin = vin;
        _year = year;
        _make = make;
        _model = model;
        _description = desc;
        _refno = refno;
    }

    /// <summary>
    /// Initializes a new instance of the Vehicle class.
    /// </summary>
    /// <param name="vin">The vehicle's VIN number.</param>
    /// <param name="stock">The vehicle's stock number.</param>
    /// <param name="condition">The vehicle's condition.</param>
    /// <param name="year">The vehicle's model year.</param>
    /// <param name="make">The vehicle's manufacturer.</param>
    /// <param name="model">The vehicle's model name.</param>
    /// <param name="refno">An optional reference number.</param>
    public Vehicle(
        string stock,
        string condition,
        decimal price,
        string vin = null,
        ushort? year = null,
        string make = null,
        string model = null,
        string desc = null,
        uint? refno = null
        ) : this(stock, ParseCondition(condition), price, vin, year, make, model, desc, refno)
    { }



    /// <summary>
    /// The vehicle's VIN number.
    /// </summary>
    public string Vin { get => _vin; }

    /// <summary>
    /// The vehicle's stock number.
    /// </summary>
    public string Stock { get => _stock; }

    /// <summary>
    /// An optional reference number that may be supplied by the vendor.
    /// </summary>
    public uint? RefNo { get => _refno; }

    /// <summary>
    /// The vehicle's condition.
    /// </summary>
    public Condition Cond { get => _cond; }

    /// <summary>
    /// The amount billed to the dealer to process this vehicle.
    /// </summary>
    public decimal Price { get => _price; }

    /// <summary>
    /// The vehicle's line item description to be printed on a work summary report.
    /// </summary>
    public string Description { get => _description ?? string.Join(" ", _year, _make, _model).Trim(); }

    

    // Implement IComparable interface
    public int CompareTo(Vehicle other)
    {
        if (other == null) { throw new ArgumentNullException(); }

        // If the vehicles are of like condition, sort based on stock number.
        else if (_cond.Equals(other.Cond))
        {
            return _stock.ToLower().CompareTo(other._stock.ToLower());
        }

        // If the vehicles are not of like condition, place the Used vehicle first.
        else return _cond.CompareTo(other._cond);
    }



    // Override ToString
    public override string ToString()
    {
        return $"{_stock}: {Description}";
    }

    // Parse string to find vehicle condition.
    private static Condition ParseCondition(string s)
    {
        if (Regex.IsMatch(s, @"^(?:cpo|certified|used)?[ -]?(?:pre)?[ -]?(?:owned)?$", RegexOptions.IgnoreCase))
        {
            return Condition.Used;
        }

        else if (Regex.IsMatch(s, @"new", RegexOptions.IgnoreCase))
        {
            return Condition.New;
        }

        else throw new ArgumentException($"{s} is not recognized as a vehicle condition.");
    }
}
