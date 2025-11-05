using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests : IClassFixture<WebApplicationFactory<Fundo.Applications.WebApi.ProgramMarker>>
    {
        private readonly WebApplicationFactory<Fundo.Applications.WebApi.ProgramMarker> _factory;

        public LoanManagementControllerTests(WebApplicationFactory<Fundo.Applications.WebApi.ProgramMarker> factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient()
        {
            // Create a unique database name for each test method
            var databaseName = $"TestDb_{Guid.NewGuid()}";
            
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<LoanDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Remove the DbContext service itself
                    var dbContextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(LoanDbContext));
                    if (dbContextDescriptor != null)
                    {
                        services.Remove(dbContextDescriptor);
                    }

                    // Add the in-memory database with a unique name per test
                    services.AddDbContext<LoanDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(databaseName);
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
            using var client = CreateClient();
            var response = await client.GetAsync("/loans");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnCreated()
        {
            using var client = CreateClient();
            var loanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Test User"
            };

            var response = await client.PostAsJsonAsync("/loans", loanDto);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetLoanById_ShouldReturnNotFound_WhenLoanDoesNotExist()
        {
            using var client = CreateClient();
            var response = await client.GetAsync("/loans/999");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task MakePayment_ShouldUpdateBalance()
        {
            using var client = CreateClient();
            var loanDto = new CreateLoanDto
            {
                Amount = 10000m,
                ApplicantName = "Test User"
            };

            var createResponse = await client.PostAsJsonAsync("/loans", loanDto);
            createResponse.EnsureSuccessStatusCode();
            var loan = await createResponse.Content.ReadFromJsonAsync<Fundo.Applications.WebApi.Models.Loan>();

            var paymentDto = new PaymentDto { Amount = 2000m };
            var paymentResponse = await client.PostAsJsonAsync($"/loans/{loan.Id}/payment", paymentDto);

            Assert.Equal(System.Net.HttpStatusCode.OK, paymentResponse.StatusCode);
            var updatedLoan = await paymentResponse.Content.ReadFromJsonAsync<Fundo.Applications.WebApi.Models.Loan>();
            Assert.Equal(8000m, updatedLoan.CurrentBalance);
        }
    }
}
