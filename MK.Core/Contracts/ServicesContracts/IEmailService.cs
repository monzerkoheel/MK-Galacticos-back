using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IEmailService
    {
        Task SendEmailAsync(
                    string to,
                    string subject,
                    string body,
                    byte[]? attachment = null,
                    string? attachmentName = null);
    }
}
