namespace PartsUnlimited.Shared
{
    public class TaxService
{
    static public decimal CalculateTax(decimal taxable, string postalCode)
    {
        return taxable * (decimal).1;
    }
}
}