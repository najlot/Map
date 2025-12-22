namespace Najlot.Map.Tests.TestTypes;

internal class Invoice
{
	public Guid Id { get; set; }
	public string Recipient { get; set; } = string.Empty;
	public double Amount { get; set; }
}

internal class InvoiceModel
{
	public Guid Id { get; set; }
	public string Recipient { get; set; } = string.Empty;
	public double Amount { get; set; }
	public bool IsPaid { get; set; }
}
