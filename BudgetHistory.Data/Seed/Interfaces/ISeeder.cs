using BudgetHistory.Abstractions.Interfaces.Data;
using Microsoft.Extensions.Configuration;
using System;

namespace BudgetHistory.Data.Seed.Interfaces
{
    public interface ISeeder
    {
        void Seed(IServiceProvider serviceProvider, IConfiguration configuration, IUnitOfWork unitOfWork);
    }
}