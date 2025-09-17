using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AsanPardakhtTest
{
    class Program
    {
        // Replace with your actual AsanPardakht credentials
        private const string USERNAME = "your_username";
        private const string PASSWORD = "your_password";
        private const int MERCHANT_CONFIG_ID = 334966; // Replace with your actual merchant config ID
        private const string CALLBACK_URL = "https://your-domain.com/callback"; // Replace with your callback URL

        static async Task Main(string[] args)
        {
            Console.WriteLine("🧪 Testing AsanPardakht Gateway with Official C# Parameters");
            Console.WriteLine("=" + new string('=', 60));

            try
            {
                // Step 1: Get token from AsanPardakht
                string token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("❌ Could not get token, stopping test");
                    return;
                }

                // Step 2: Create HTML file for testing
                CreatePaymentTestPage(token);

                Console.WriteLine("✅ Test completed! Check the generated HTML file.");
                Console.WriteLine("📁 Upload AsanPardakhtTest.html to your web server and access it via browser");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static async Task<string> GetTokenAsync()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // Step 1: Get server time
                    string serverTime = await GetServerTimeAsync(client);
                    Console.WriteLine($"✅ Server time: {serverTime}");

                    // Step 2: Prepare token request (exactly like official C# sample)
                    int invoiceId = 12345;
                    long amount = 1330000; // 133 Toman (same as Postman example)

                    var tokenRequest = new
                    {
                        serviceTypeId = 1,
                        merchantConfigurationId = MERCHANT_CONFIG_ID,
                        localInvoiceId = invoiceId,
                        amountInRials = amount,
                        localDate = serverTime,
                        callbackURL = $"{CALLBACK_URL}?invoiceID={invoiceId}",
                        paymentId = "0",
                        settlementPortions = new object[0] // Empty array
                    };

                    string json = JsonConvert.SerializeObject(tokenRequest, Formatting.Indented);
                    Console.WriteLine($"📤 Sending token request...");
                    Console.WriteLine($"Data: {json}");

                    // Set headers
                    client.DefaultRequestHeaders.Add("usr", USERNAME);
                    client.DefaultRequestHeaders.Add("pwd", PASSWORD);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://ipgrest.asanpardakht.ir/v1/Token", content);

                    string responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📥 Response status: {response.StatusCode}");
                    Console.WriteLine($"📥 Response: {responseText}");

                    if (response.IsSuccessStatusCode)
                    {
                        string token = responseText.Trim().Trim('"');
                        Console.WriteLine($"✅ Token received: {token}");
                        return token;
                    }
                    else
                    {
                        Console.WriteLine($"❌ Token request failed: {response.StatusCode} - {responseText}");
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error getting token: {ex.Message}");
                    return string.Empty;
                }
            }
        }

        static async Task<string> GetServerTimeAsync(HttpClient client)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
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

        static void CreatePaymentTestPage(string token)
        {
            string mobileNumber = "09123456789"; // Optional test mobile number

            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>AsanPardakht Test Payment - C# Generated</title>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .info {{ background: #f0f8ff; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .form-container {{ background: #fff3cd; padding: 20px; border-radius: 8px; border: 2px solid #ffc107; }}
        button {{ background: #28a745; color: white; padding: 15px 30px; font-size: 16px; border: none; border-radius: 5px; cursor: pointer; }}
        button:hover {{ background: #218838; }}
        ul {{ text-align: left; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h2>🧪 AsanPardakht Gateway Test (C# Generated)</h2>
        
        <div class=""info"">
            <h3>📋 Test Information:</h3>
            <ul>
                <li><strong>Token:</strong> {token}</li>
                <li><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                <li><strong>IP Check:</strong> This request came from your VPS</li>
                <li><strong>Parameters:</strong> Exactly matching official C# sample</li>
            </ul>
        </div>
        
        <div class=""form-container"">
            <h3>🚀 Payment Form (Official AsanPardakht Format)</h3>
            <p>This form will be submitted exactly like the official C# sample:</p>
            
            <form id=""paymentForm"" action=""https://asan.shaparak.ir"" method=""POST"">
                <input type=""hidden"" name=""RefId"" value=""{token}"">
                <input type=""hidden"" name=""mobileap"" value=""{mobileNumber}"">
                <button type=""submit"">🔗 Go to AsanPardakht Gateway</button>
            </form>
            
            <h4>📤 Form Data Being Sent:</h4>
            <ul>
                <li><strong>URL:</strong> https://asan.shaparak.ir</li>
                <li><strong>Method:</strong> POST</li>
                <li><strong>RefId:</strong> {token}</li>
                <li><strong>mobileap:</strong> {mobileNumber}</li>
            </ul>
        </div>
        
        <div class=""info"">
            <h3>🎯 Expected Results:</h3>
            <ul>
                <li>✅ <strong>Success:</strong> You should see AsanPardakht payment page</li>
                <li>❌ <strong>""Page cannot be displayed"":</strong> Indicates gateway/configuration issue</li>
                <li>🔍 <strong>Error message:</strong> Will help identify the specific problem</li>
            </ul>
            <p><strong>Note:</strong> This test eliminates any framework-related issues since it uses pure HTML form submission exactly like the official C# sample.</p>
        </div>
    </div>
    
    <script>
        // Optional: Auto-submit after 5 seconds
        setTimeout(function() {{
            if (confirm('Auto-submit to AsanPardakht gateway?')) {{
                document.getElementById('paymentForm').submit();
            }}
        }}, 5000);
    </script>
</body>
</html>";

            File.WriteAllText("AsanPardakhtTest.html", html, Encoding.UTF8);
            Console.WriteLine("✅ Test HTML file created: AsanPardakhtTest.html");
            Console.WriteLine("📁 Upload this file to your web server and access it via browser");
        }
    }
}
