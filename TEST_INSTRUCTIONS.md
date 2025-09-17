# AsanPardakht C# Test Instructions

## 🎯 Purpose
This C# console application tests AsanPardakht gateway using their exact official sample code. This will help determine if the issue is with your framework implementation or with AsanPardakht itself.

## 📋 Prerequisites
- .NET 6.0 or later installed on your VPS
- Your VPS IP must be whitelisted with AsanPardakht

## 🚀 Setup Instructions

### 1. Upload Files to Your VPS
Upload these files to your VPS server:
- `AsanPardakhtTest.cs`
- `AsanPardakhtTest.csproj`

### 2. Update Credentials
Edit `AsanPardakhtTest.cs` and replace:
```csharp
private const string USERNAME = "your_username";           // ← Your real username
private const string PASSWORD = "your_password";           // ← Your real password  
private const int MERCHANT_CONFIG_ID = 334966;            // ← Your real merchant config ID
private const string CALLBACK_URL = "https://your-domain.com/callback"; // ← Your real callback URL
```

### 3. Run the Test
On your VPS, execute:
```bash
# Navigate to the directory containing the files
cd /path/to/your/files

# Restore packages and run
dotnet run
```

## 📤 What It Does

1. **Gets Server Time** from AsanPardakht API
2. **Requests Token** using official parameters:
   - serviceTypeId: 1
   - Amount: 1,330,000 Rials (133 Toman)
   - settlementPortions: [] (required field)
3. **Creates HTML file** with exact form submission matching official C# sample
4. **Generates `AsanPardakhtTest.html`** file

## 🧪 Testing Process

1. **Run the C# application** on your VPS
2. **Upload the generated `AsanPardakhtTest.html`** to your web server
3. **Access the HTML file** via browser
4. **Click the button** to submit to AsanPardakht gateway

## 🎯 Expected Results

### ✅ Success Scenario
- You see the AsanPardakht payment page
- This means your credentials and gateway are working
- The issue was likely in the framework implementation

### ❌ Failure Scenario  
- You get "Page cannot be displayed" error
- This means the issue is with AsanPardakht configuration/credentials
- Check with AsanPardakht support about:
  - IP whitelisting
  - Merchant configuration
  - Test vs Production environment

## 🔍 Troubleshooting

### If Token Request Fails:
- Check credentials (username/password)
- Verify IP is whitelisted
- Check merchant configuration ID

### If Gateway Redirect Fails:
- Verify the HTML form parameters match exactly
- Check browser network tab for actual POST data
- Compare with working AsanPardakht examples

## 📞 Next Steps

Based on the results:

1. **If C# test works**: The issue is in your Python framework
2. **If C# test fails**: Contact AsanPardakht support with:
   - Your VPS IP address
   - Merchant configuration ID
   - The exact error you're seeing
   - Token response (if successful)

This test eliminates any framework complexity and uses pure HTTP requests exactly like AsanPardakht's official sample!
