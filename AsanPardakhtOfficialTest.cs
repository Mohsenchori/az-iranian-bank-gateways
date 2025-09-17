using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AsanPardakhtOfficialTest
{
    // Models based on official sample
    public class SaleCommand
    {
        public int serviceTypeId { get; set; }
        public int merchantConfigurationId { get; set; }
        public int localInvoiceId { get; set; }
        public ulong amountInRials { get; set; }
        public string localDate { get; set; }
        public string callbackURL { get; set; }
        public string paymentId { get; set; } = "0";
        public object[] settlementPortions { get; set; } = new object[0];

        public SaleCommand(int merchantConfigId, int serviceType, int invoiceId, ulong amount, string callback, string paymentId)
        {
            this.merchantConfigurationId = merchantConfigId;
            this.serviceTypeId = serviceType;
            this.localInvoiceId = invoiceId;
            this.amountInRials = amount;
            this.callbackURL = callback;
            this.paymentId = paymentId;
        }
    }

    public class SaleTokenVm
    {
        public int ResCode { get; set; }
        public string ResMessage { get; set; }
        public string RefId { get; set; }
    }

    public enum ServiceTypeEnum
    {
        Sale = 1
    }

    class Program
    {
        // Replace with your actual AsanPardakht credentials
        private const string USERNAME = "frosh5543603";
        private const string PASSWORD = "PrV5734Cg";
        private const int MERCHANT_CONFIG_ID = 334966;
        private const string CALLBACK_URL = "https://rojafon.com/callback";

        static async Task Main(string[] args)
        {
            Console.WriteLine("🏛️ Official AsanPardakht C# Sample Test");
            Console.WriteLine("=" + new string('=', 50));

            try
            {
                // Test parameters (from official sample)
                int localInvoiceID = 12345;
                ulong amountInRials = 1330000; // 133 Toman

                // Step 1: Get server time
                string serverTime = await GetServerTimeAsync();
                Console.WriteLine($"✅ Server time: {serverTime}");

                // Step 2: Create payment token (exactly like official sample)
                var paymentToken = new SaleCommand(
                    MERCHANT_CONFIG_ID,
                    (int)ServiceTypeEnum.Sale,
                    localInvoiceID,
                    amountInRials,
                    $"{CALLBACK_URL}?invoiceID={localInvoiceID}",
                    string.Empty
                );

                // Add server time to the command
                paymentToken.localDate = serverTime;

                Console.WriteLine("📤 Creating payment token...");
                var result = await GetTokenAsync(paymentToken);

                if (result.ResCode == 0)
                {
                    Console.WriteLine($"✅ Token received: {result.RefId}");
                    
                    // Step 3: Create HTML file for payment redirect (like official sample)
                    CreatePaymentPage(result.RefId, "09123456789");
                    
                    Console.WriteLine("✅ Payment page created: AsanPardakhtOfficial.html");
                    Console.WriteLine("📁 Upload this file to test the payment gateway");
                }
                else
                {
                    Console.WriteLine($"❌ Token request failed:");
                    Console.WriteLine($"   Code: {result.ResCode}");
                    Console.WriteLine($"   Message: {result.ResMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static async Task<SaleTokenVm> GetTokenAsync(SaleCommand command)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // Exactly like official sample IPGResetService
                    string json = JsonConvert.SerializeObject(command, Formatting.Indented);
                    Console.WriteLine($"Request data: {json}");

                    // Set headers (like official sample)
                    client.DefaultRequestHeaders.Add("usr", USERNAME);
                    client.DefaultRequestHeaders.Add("pwd", PASSWORD);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://ipgrest.asanpardakht.ir/v1/Token", content);

                    string responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📥 Response: {responseText}");

                    if (response.IsSuccessStatusCode)
                    {
                        // Try to parse as JSON first
                        try
                        {
                            var tokenResult = JsonConvert.DeserializeObject<SaleTokenVm>(responseText);
                            return tokenResult;
                        }
                        catch
                        {
                            // If JSON parsing fails, treat as simple token string
                            return new SaleTokenVm 
                            { 
                                ResCode = 0, 
                                RefId = responseText.Trim().Trim('"'),
                                ResMessage = "Success"
                            };
                        }
                    }
                    else
                    {
                        return new SaleTokenVm 
                        { 
                            ResCode = (int)response.StatusCode,
                            ResMessage = responseText
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error in token request: {ex.Message}");
                    return new SaleTokenVm 
                    { 
                        ResCode = -1,
                        ResMessage = ex.Message
                    };
                }
            }
        }

        static async Task<string> GetServerTimeAsync()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("usr", USERNAME);
                    client.DefaultRequestHeaders.Add("pwd", PASSWORD);

                    var response = await client.GetAsync("https://ipgrest.asanpardakht.ir/v1/Time");
                    if (response.IsSuccessStatusCode)
                    {
                        string timeText = await response.Content.ReadAsStringAsync();
                        return timeText.Trim().Trim('"');
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Could not get server time, using local time");
                        return DateTime.Now.ToString("yyyyMMdd HHmmss");
                    }
                }
                catch
                {
                    Console.WriteLine($"⚠️ Could not get server time, using local time");
                    return DateTime.Now.ToString("yyyyMMdd HHmmss");
                }
            }
        }

        static void CreatePaymentPage(string refId, string mobileNumber)
        {
            // Exactly like official sample: NameValueCollection with RefId and mobileap
            var nvc = new NameValueCollection();
            nvc.Add("RefId", refId);
            nvc.Add("mobileap", mobileNumber);

            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>AsanPardakht Official Sample Test</title>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .info {{ background: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .form-container {{ background: #fff3cd; padding: 20px; border-radius: 8px; border: 2px solid #ffc107; }}
        button {{ background: #007bff; color: white; padding: 15px 30px; font-size: 16px; border: none; border-radius: 5px; cursor: pointer; }}
        button:hover {{ background: #0056b3; }}
        ul {{ text-align: left; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h2>🏛️ AsanPardakht Official Sample Test</h2>
        
        <div class=""info"">
            <h3>📋 Official Sample Information:</h3>
            <ul>
                <li><strong>Source:</strong> Official AsanPardakht C# Sample</li>
                <li><strong>RefId (Token):</strong> {refId}</li>
                <li><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                <li><strong>Method:</strong> Exactly like startpayment.aspx.cs</li>
            </ul>
        </div>
        
        <div class=""form-container"">
            <h3>🚀 Payment Form (Official AsanPardakht)</h3>
            <p>This form uses the exact same parameters as the official C# sample:</p>
            
            <form id=""paymentForm"" action=""https://asan.shaparak.ir"" method=""POST"">
                <input type=""hidden"" name=""RefId"" value=""{refId}"">
                <input type=""hidden"" name=""mobileap"" value=""{mobileNumber}"">
                <button type=""submit"">🔗 Pay with AsanPardakht</button>
            </form>
            
            <h4>📤 Form Data (from NameValueCollection):</h4>
            <ul>
                <li><strong>Action:</strong> https://asan.shaparak.ir</li>
                <li><strong>Method:</strong> POST</li>
                <li><strong>RefId:</strong> {refId}</li>
                <li><strong>mobileap:</strong> {mobileNumber}</li>
            </ul>
        </div>
    </div>
    
    <script>
        console.log('Official AsanPardakht Sample Test');
        console.log('RefId:', '{refId}');
        console.log('Mobile:', '{mobileNumber}');
    </script>
</body>
</html>";

            File.WriteAllText("AsanPardakhtOfficial.html", html, Encoding.UTF8);
        }
    }
}
