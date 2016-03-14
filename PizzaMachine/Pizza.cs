namespace PizzaMachine
{
    /// <summary>
    /// Represents a pizza. Nham.
    /// </summary>
    public class Pizza
    {
        /// <summary>
        /// Pizza unique identification.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Pizza price.
        /// </summary>
        public decimal Price { get; private set; }

		/// <summary>
		/// Constructor that sets all values.
		/// </summary>
		/// <param name="id">Pizza id.</param>
		/// <param name="price">Pizza price.</param>
        public Pizza(int id, decimal price)
        {
            this.Id = id;
            this.Price = price;
        }
    }
}
