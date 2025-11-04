using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.Models;

namespace Fundo.Applications.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=localhost;Database=LoanManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;";

            services.AddDbContext<LoanDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
                context.Database.EnsureCreated();
                SeedData(context);
            }

            app.UseRouting();
            app.UseCors("AllowFrontend");
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private void SeedData(LoanDbContext context)
        {
            if (!context.Loans.Any())
            {
                context.Loans.AddRange(
                    new Loan
                    {
                        Amount = 25000.00m,
                        CurrentBalance = 18750.00m,
                        ApplicantName = "John Doe",
                        Status = "active"
                    },
                    new Loan
                    {
                        Amount = 15000.00m,
                        CurrentBalance = 0m,
                        ApplicantName = "Jane Smith",
                        Status = "paid"
                    },
                    new Loan
                    {
                        Amount = 50000.00m,
                        CurrentBalance = 32500.00m,
                        ApplicantName = "Robert Johnson",
                        Status = "active"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
