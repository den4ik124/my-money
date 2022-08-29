using Microsoft.Extensions.Configuration;
using System;

namespace Notebook.Data.Seed.Interfaces
{
    public interface ISeeder
    {
        void Seed(IServiceProvider serviceProvider, IConfiguration configuration);
    }
}