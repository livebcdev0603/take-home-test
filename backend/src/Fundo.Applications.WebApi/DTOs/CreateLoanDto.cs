namespace Fundo.Applications.WebApi.DTOs
{
    public class CreateLoanDto
    {
        public decimal Amount { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
    }
}

