using System.Net;

namespace MK.Service.HelperServices.EmailSevrices.Templates
{
    public static class FanRequestAvailableTemplate
    {
        public static string Generate(string userName, string fanRequestName, string imageUrl, string storeUrl)
        {
            var safeUserName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(userName) ? "there" : userName.Trim());
            var safeName = WebUtility.HtmlEncode(fanRequestName ?? string.Empty);
            var safeImageUrl = WebUtility.HtmlEncode(imageUrl ?? string.Empty);
            var safeStoreUrl = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(storeUrl) ? "#" : storeUrl);
            var year = DateTime.UtcNow.Year;

            var imageBlock = string.IsNullOrWhiteSpace(imageUrl)
                ? string.Empty
                : $"""
                   <tr>
                     <td align="center" style="padding:0 0 18px 0;">
                       <img src="{safeImageUrl}" alt="{safeName}" width="320" style="display:block;width:100%;max-width:320px;height:auto;border-radius:10px;border:1px solid #e3e6ea;" />
                     </td>
                   </tr>
                   """;

            return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>Your Fan Request is Now Available</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f3f4f6;font-family:Arial,Helvetica,sans-serif;">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:#f3f4f6;">
                The Fan Request you supported, {safeName}, is now available on MK Galácticos.
              </div>
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f3f4f6;padding:24px 12px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="600" cellpadding="0" cellspacing="0" border="0" style="width:100%;max-width:600px;background-color:#ffffff;border-radius:12px;overflow:hidden;">

                      <tr>
                        <td align="center" style="background-color:#0b1f3a;padding:26px 24px;">
                          <div style="color:#c8a24a;font-size:13px;letter-spacing:3px;text-transform:uppercase;">MK Galácticos</div>
                          <div style="color:#ffffff;font-size:24px;font-weight:bold;margin-top:8px;">Your Fan Request is Live!</div>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:30px 32px 10px 32px;color:#1f2937;font-size:16px;line-height:24px;">
                          <p style="margin:0 0 14px 0;">Hi {safeUserName},</p>
                          <p style="margin:0 0 14px 0;">
                            Great news! A Fan Request you supported with your like has officially made it to our store.
                            Thank you for helping the community bring it to life.
                          </p>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:10px 32px 0 32px;">
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f9fafb;border:1px solid #e5e7eb;border-radius:10px;">
                            <tr>
                              <td style="padding:20px;">
                                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0">
                                  {imageBlock}
                                  <tr>
                                    <td align="center" style="font-size:12px;letter-spacing:2px;text-transform:uppercase;color:#6b7280;padding-bottom:6px;">Now available</td>
                                  </tr>
                                  <tr>
                                    <td align="center" style="font-size:20px;font-weight:bold;color:#0b1f3a;line-height:28px;">{safeName}</td>
                                  </tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      <tr>
                        <td align="center" style="padding:28px 32px 10px 32px;">
                          <a href="{safeStoreUrl}" target="_blank" style="display:inline-block;background-color:#c8a24a;color:#0b1f3a;text-decoration:none;font-weight:bold;font-size:16px;padding:14px 36px;border-radius:8px;">
                            Visit the Store
                          </a>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding:14px 32px 30px 32px;color:#6b7280;font-size:14px;line-height:22px;" align="center">
                          Keep supporting Fan Requests &mdash; the next one could be yours.
                        </td>
                      </tr>

                      <tr>
                        <td align="center" style="background-color:#f9fafb;border-top:1px solid #e5e7eb;padding:18px 24px;color:#9ca3af;font-size:12px;line-height:18px;">
                          You are receiving this email because you liked this Fan Request on MK Galácticos.<br />
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
