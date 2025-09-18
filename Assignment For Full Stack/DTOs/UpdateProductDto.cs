namespace Assignment_For_Full_Stack.DTOs
{
    public class UpdateProductDto
    {
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public string Category { get; set; }
        public byte[] Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountRate { get; set; }
    }

}
