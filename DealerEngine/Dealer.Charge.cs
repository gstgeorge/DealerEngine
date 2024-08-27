namespace DealerEngine;

internal partial class Dealer
{
    /// <summary>
    /// Data structure to hold info on charges billed to the dealer which occur on a monthly or per-vehicle-serviced basis.
    /// </summary>
    internal class Charge
    {
        /// <summary>
        /// Type of monthly charge.
        /// </summary>
        public enum ChargeType
        {
            /// <summary>
            /// Charge should appear as-is. This is a fixed monthly charge.
            /// </summary>
            FIXED,

            /// <summary>
            /// Charge should be multiplied by the total number of Used vehicles processed.
            /// </summary>
            USED,

            /// <summary>
            /// Charge should be multiplied by the total number of New vehicles processed.
            /// </summary>
            NEW,

            /// <summary>
            /// Charge should be multiplied by the total number of vehicles processed.
            /// </summary>
            VEHICLE
        }



        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">The name of the charge</param>
        /// <param name="type">The type of the charge</param>
        /// <param name="price">The charge amount</param>
        /// <param name="enabled">Apply this charge to future invoices</param>
        public Charge(string name, ChargeType type, decimal price, bool enabled=true)
        {
            Name = name;
            Type = type;
            Price = price;
            Enabled = enabled;
        }



        /// <summary>
        /// The charge's line item description.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of charge.
        /// </summary>
        public ChargeType Type { get; set; }

        /// <summary>
        /// The charge amount.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Should the charge be included when an invoice for this dealer is generated?
        /// </summary>
        public bool Enabled { get; set; }
    }
}
