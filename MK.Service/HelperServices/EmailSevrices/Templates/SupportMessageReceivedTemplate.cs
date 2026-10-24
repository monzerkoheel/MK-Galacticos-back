using System.Net;

namespace MK.Service.HelperServices.EmailSevrices
{
    /// <summary>
    /// English confirmation email sent to a user right after he contacts the support team.
    /// The name and the message are user input, so they are HTML-encoded.
    /// </summary>
    public static class SupportMessageReceivedTemplate
    {
        public static string Generate(string userName, string userMessage)
        {
            var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(userName) ? "there" : userName.Trim());

            var safeMessage = WebUtility.HtmlEncode(userMessage ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", "<br />");

            var year = DateTime.UtcNow.Year;

            return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>We Received Your Message</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f3f4f6;font-family:Arial,Helvetica,sans-serif;">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:#f3f4f6;">
                Thanks for contacting MK Galácticos. A member of our support team will get back to you soon.
              </div>
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f3f4f6;padding:24px 12px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="600" cellpadding="0" cellspacing="0" border="0" style="width:100%;max-width:600px;background-color:#ffffff;border-radius:12px;overflow:hidden;">

                      <tr>
                        <td align="center" style="background-color:#0b1f3a;padding:26px 24px;">
                          <div style="color:#c8a24a;font-size:13px;letter-spacing:3px;text-transform:uppercase;">MK Galácticos</div>
                          <div style="color:#ffffff;font-size:24px;font-weight:bold;margin-top:8px;">We Received Your Message</div>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:30px 32px 10px 32px;color:#1f2937;font-size:16px;line-height:24px;">
                          <p style="margin:0 0 14px 0;">Hi {safeName},</p>
                          <p style="margin:0 0 14px 0;">
                            Thank you for reaching out to us. We have received your message and a member of our
                            support team will contact you soon.
                          </p>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:6px 32px 0 32px;">
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f9fafb;border:1px solid #e5e7eb;border-left:4px solid #c8a24a;border-radius:8px;">
                            <tr>
                              <td style="padding:16px 18px;">
                                <div style="font-size:12px;letter-spacing:2px;text-transform:uppercase;color:#6b7280;padding-bottom:8px;">Your message</div>
                                <div style="font-size:15px;line-height:22px;color:#374151;word-break:break-word;">{safeMessage}</div>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:24px 32px 30px 32px;color:#6b7280;font-size:14px;line-height:22px;">
                          There is no need to send the same message again. If you have anything to add,
                          simply reply to this email or contact us through the website.
                        </td>
                      </tr>

                      <tr>
                        <td align="center" style="background-color:#f9fafb;border-top:1px solid #e5e7eb;padding:18px 24px;color:#9ca3af;font-size:12px;line-height:18px;">
                          You are receiving this email because this address was used to contact MK Galácticos support.<br />
                          &copy; {year} MK Galácticos. All rights reserved.
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
        }
    }
}
