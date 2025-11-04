using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests : IClassFixture<WebApplicationFactory<Fundo.Applications.WebApi.Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Fundo.Applications.WebApi.Startup> _factory;

        public LoanManagementControllerTests(WebApplicationFactory<Fundo.Applications.WebApi.Startup> factory)
        {
            _factory = factory;
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<LoanDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<LoanDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetAllLoans_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/loans");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnCreated()
        {
            var loanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Test User"
            };

            var response = await _client.PostAsJsonAsync("/loans", loanDto);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetLoanById_ShouldReturnNotFound_WhenLoanDoesNotExist()
        {
            var response = await _client.GetAsync("/loans/999");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task MakePayment_ShouldUpdateBalance()
        {
            var loanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Test User"
            };

            var createResponse = await _client.PostAsJsonAsync("/loans", loanDto);
            var loan = await createResponse.Content.ReadFromJsonAsync<Fundo.Applications.WebApi.Models.Loan>();

            var paymentDto = new PaymentDto { Amount = 2000m };
            var paymentResponse = await _client.PostAsJsonAsync($"/loans/{loan.Id}/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.OK, paymentResponse.StatusCode);
            var updatedLoan = await paymentResponse.Content.ReadFromJsonAsync<Fundo.Applications.WebApi.Models.Loan>();
            Assert.Equal(8000m, updatedLoan.CurrentBalance);
        }
    }
}
