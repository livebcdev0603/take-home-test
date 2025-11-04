using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Applications.WebApi.Controllers
{
    [Route("/loans")]
    [ApiController]
    public class LoanManagementController : ControllerBase
    {
        private readonly LoanDbContext _context;

        public LoanManagementController(LoanDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Loan>> CreateLoan([FromBody] CreateLoanDto dto)
        {
            if (dto.Amount <= 0)
            {
                return BadRequest("Loan amount must be greater than zero.");
            }

            var loan = new Loan
            {
                Amount = dto.Amount,
                CurrentBalance = dto.Amount,
                ApplicantName = dto.ApplicantName,
                Status = "active"
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            return loan;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetAllLoans()
        {
            return await _context.Loans.ToListAsync();
        }

        [HttpPost("{id}/payment")]
        public async Task<ActionResult<Loan>> MakePayment(int id, [FromBody] PaymentDto payment)
        {
            if (payment.Amount <= 0)
            {
                return BadRequest("Payment amount must be greater than zero.");
            }

            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            if (loan.CurrentBalance < payment.Amount)
            {
                return BadRequest("Payment amount exceeds current balance.");
            }

            loan.CurrentBalance -= payment.Amount;

            if (loan.CurrentBalance == 0)
            {
                loan.Status = "paid";
            }

            await _context.SaveChangesAsync();

            return loan;
        }
    }
}