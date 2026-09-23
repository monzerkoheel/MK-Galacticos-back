using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.EmailSevrices
{
    public static class VerifyEmailTemplate
    {
        public static string Generate(string otp)
        {
            return $"""
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset="UTF-8">
                        <meta name="viewport" content="width=device-width, initial-scale=1.0">

                        <title>Verify Your Email</title>
                    </head>

                    <body style="
                        margin:0;
                        padding:0;
                        background-color:#0A0A0A;
                        font-family:Arial, Helvetica, sans-serif;
                    ">

                        <table width="100%" cellpadding="0" cellspacing="0" border="0">
                            <tr>
                                <td align="center" style="padding:40px 20px;">

                                    <table
                                        width="600"
                                        cellpadding="0"
                                        cellspacing="0"
                                        border="0"
                                        style="
                                            max-width:600px;
                                            width:100%;
                                            background-color:#151515;
                                            border-radius:16px;
                                            overflow:hidden;
                                        "
                                    >

                                        <!-- Header -->
                                        <tr>
                                            <td align="center" style="padding:35px 30px 20px;">

                                                <h1 style="
                                                    margin:0;
                                                    color:#D4AF37;
                                                    font-size:28px;
                                                    letter-spacing:2px;
                                                ">
                                                    MK GALÁCTICOS
                                                </h1>

                                            </td>
                                        </tr>

                                        <!-- Content -->
                                        <tr>
                                            <td style="padding:20px 40px 40px;">

                                                <h2 style="
                                                    margin:0 0 20px;
                                                    color:#FFFFFF;
                                                    font-size:24px;
                                                    text-align:center;
                                                ">
                                                    Verify Your Email
                                                </h2>

                                                <p style="
                                                    margin:0 0 15px;
                                                    color:#CCCCCC;
                                                    font-size:16px;
                                                    line-height:1.6;
                                                    text-align:center;
                                                ">
                                                    Thanks for creating your account with MK Galácticos.
                                                </p>

                                                <p style="
                                                    margin:0 0 30px;
                                                    color:#CCCCCC;
                                                    font-size:16px;
                                                    line-height:1.6;
                                                    text-align:center;
                                                ">
                                                    Use the verification code below to complete your registration.
                                                </p>

                                                <!-- OTP -->
                                                <table
                                                    width="100%"
                                                    cellpadding="0"
                                                    cellspacing="0"
                                                    border="0"
                                                >
                                                    <tr>
                                                        <td align="center">

                                                            <div style="
                                                                display:inline-block;
                                                                padding:18px 30px;
                                                                background-color:#0A0A0A;
                                                                border:1px solid #D4AF37;
                                                                border-radius:12px;
                                                                color:#D4AF37;
                                                                font-size:32px;
                                                                font-weight:bold;
                                                                letter-spacing:8px;
                                                            ">
                                                                {otp[..3]} {otp[3..]}
                                                            </div>

                                                        </td>
                                                    </tr>
                                                </table>

                                                <p style="
                                                    margin:25px 0 0;
                                                    color:#999999;
                                                    font-size:14px;
                                                    text-align:center;
                                                ">
                                                    This code expires in 5 minutes.
                                                </p>

                                                <p style="
                                                    margin:25px 0 0;
                                                    color:#777777;
                                                    font-size:13px;
                                                    line-height:1.6;
                                                    text-align:center;
                                                ">
                                                    If you didn't create this account,
                                                    you can safely ignore this email.
                                                </p>

                                            </td>
                                        </tr>

                                        <!-- Footer -->
                                        <tr>
                                            <td
                                                align="center"
                                                style="
                                                    padding:20px;
                                                    border-top:1px solid #292929;
                                                "
                                            >

                                                <p style="
                                                    margin:0;
                                                    color:#D4AF37;
                                                    font-size:13px;
                                                ">
                                                    MK Galácticos Support Team
                                                </p>

                                                <p style="
                                                    margin:8px 0 0;
                                                    color:#666666;
                                                    font-size:12px;
                                                ">
                                                    © MK Galácticos
                                                </p>

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
