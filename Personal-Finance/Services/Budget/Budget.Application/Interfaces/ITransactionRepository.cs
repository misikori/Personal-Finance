using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget.Domain.Entities;

namespace Budget.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
    }
}
