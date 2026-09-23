using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoiceAsync(Order order, string languageCode);
    }
}
