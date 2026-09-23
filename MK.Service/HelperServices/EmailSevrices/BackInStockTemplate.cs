using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.EmailSevrices
{
    public static class BackInStockTemplate
    {
        public static string Generate(
            string productName,
            string imageUrl,
            string productUrl)
        {
            return $"""
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">

                    <title>Back in Stock</title>
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
                                                margin:0 0 15px;
                                                color:#FFFFFF;
                                                font-size:24px;
                                                text-align:center;
                                            ">
                                                Back in Stock
                                            </h2>

                                            <p style="
                                                margin:0 0 25px;
                                                color:#CCCCCC;
                                                font-size:16px;
                                                line-height:1.6;
                                                text-align:center;
                                            ">
                                                Good news! The product you were waiting for is
                                                back in stock.
                                            </p>

                                            <!-- Product Image -->
                                            <table
                                                width="100%"
                                                cellpadding="0"
                                                cellspacing="0"
                                                border="0"
                                            >
                                                <tr>
                                                    <td align="center">

                                                        <img
                                                            src="{imageUrl}"
                                                            alt="{productName}"
                                                            width="260"
                                                            style="
                                                                display:block;
                                                                width:260px;
                                                                max-width:100%;
                                                                height:auto;
                                                                border-radius:12px;
                                                                border:1px solid #292929;
                                                            "
                                                        >

                                                    </td>
                                                </tr>
                                            </table>

                                            <!-- Product Name -->
                                            <h3 style="
                                                margin:25px 0 10px;
                                                color:#FFFFFF;
                                                font-size:21px;
                                                text-align:center;
                                            ">
                                                {productName}
                                            </h3>

                                            <p style="
                                                margin:0 0 30px;
                                                color:#999999;
                                                font-size:14px;
                                                line-height:1.6;
                                                text-align:center;
                                            ">
                                                Don't miss your chance to get it before it sells out again.
                                            </p>

                                            <!-- CTA -->
                                            <table
                                                width="100%"
                                                cellpadding="0"
                                                cellspacing="0"
                                                border="0"
                                            >
                                                <tr>
                                                    <td align="center">

                                                        <a
                                                            href="{productUrl}"
                                                            style="
                                                                display:inline-block;
                                                                padding:14px 32px;
                                                                background-color:#D4AF37;
                                                                color:#0A0A0A;
                                                                text-decoration:none;
                                                                font-size:15px;
                                                                font-weight:bold;
                                                                border-radius:10px;
                                                            "
                                                        >
                                                            SHOP NOW
                                                        </a>

                                                    </td>
                                                </tr>
                                            </table>

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
