using Newtonsoft.Json;
using PGTesterApp.RestSample.models;
using PGTesterApp.RestSample.models.bill;
using PGTesterApp.RestSample.models.reverse;
using PGTesterApp.RestSample.models.settle;
using PGTesterApp.RestSample.models.verify;
using PGTesterApp.RestSample.Moldels;
using PGTesterApp.RestSample.Moldels.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PGTesterApp.RestSample
{
    public class IPGResetService
    {
        private const string REST_URL = "https://ipgrest.asanpardakht.ir/";
        //private const string REST_URL = "http://localhost:57261";
        public async Task<TResult> Token<TRequest, TResult>(TRequest request)
            where TRequest : ITokenCommand
            where TResult : class, ITokenVm, new()

        {
            var refid = string.Empty;

            var client = new HttpClient
            {
                BaseAddress = new Uri(REST_URL),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", System.Configuration.ConfigurationManager.AppSettings["MerchantUsd"]);
            client.DefaultRequestHeaders.Add("pwd", System.Configuration.ConfigurationManager.AppSettings["MerchantPass"]);
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                var responseMessage = (await client.PostAsync($"v1/Token", content, new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        refid = await responseMessage.Content.ReadAsStringAsync();
                        return new TResult { RefId = JsonConvert.DeserializeObject<string>(refid), ResCode = 0 };
                    case 489:
                        return new TResult { ResCode = 489, ResMessage = "duplicate local invoice id" };
                    case 484:
                        return new TResult { ResCode = 484, ResMessage = "internal error for other reasons" };
                    case 486:
                        return new TResult { ResCode = 486, ResMessage = "amount is not in range" };
                    case 504:
                        return new TResult { ResCode = 504, ResMessage = responseMessage.ReasonPhrase };
                    default:
                        return new TResult { ResCode = (int)responseMessage.StatusCode, ResMessage = "unknown" };
                }
            }
            catch (TaskCanceledException)
            {
                return new TResult { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = "Gateway Timeout" };
            }
            catch (Exception ex)
            {
                return new TResult { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = ex.Message };
            }
        }

        public async Task<VerifyVm> VerifyTrx(VerifyCommand verifyCommand)
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri(REST_URL),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", System.Configuration.ConfigurationManager.AppSettings["MerchantUsd"]);
            client.DefaultRequestHeaders.Add("pwd", System.Configuration.ConfigurationManager.AppSettings["MerchantPass"]);
            var content = new StringContent(JsonConvert.SerializeObject(verifyCommand), Encoding.UTF8, "application/json");

            try
            {
                var responseMessage = (await client.PostAsync($"v1/Verify", content, new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        return new VerifyVm() { ResCode = 0, ResMessage = "verification succeeded" };
                    case 400:
                        return new VerifyVm() { ResCode = 400, ResMessage = "bad request" };
                    case 401:
                        return new VerifyVm() { ResCode = 401, ResMessage = "unauthorized. probably wrong or unsent header(s)" };
                    case 471:
                        return new VerifyVm() { ResCode = 471, ResMessage = "original transaction was failed" };
                    case 472:
                        return new VerifyVm() { ResCode = 472, ResMessage = "transaction already requested for verification" };
                    case 473:
                        return new VerifyVm() { ResCode = 473, ResMessage = "transaction already requested for reconcilation" };
                    case 474:
                        return new VerifyVm() { ResCode = 474, ResMessage = "transaction already requested for reversal" };
                    case 475:
                        return new VerifyVm() { ResCode = 475, ResMessage = "transaction already listed for reconcilation" };
                    case 476:
                        return new VerifyVm() { ResCode = 476, ResMessage = "transaction already listed for reversal" };
                    case 477:
                        return new VerifyVm() { ResCode = 477, ResMessage = "identity not trusted to proceed" };
                    case 478:
                        return new VerifyVm() { ResCode = 478, ResMessage = "verification already cancelled" };
                    case 571:
                        return new VerifyVm() { ResCode = 571, ResMessage = "not yet processed" };
                    case 572:
                        return new VerifyVm() { ResCode = 572, ResMessage = "transaction status undetermined" };
                    case 573:
                        return new VerifyVm() { ResCode = 573, ResMessage = "unable to request for verification due to an internal error" };
                    default:
                        return new VerifyVm() { ResCode = (int)responseMessage.StatusCode, ResMessage = responseMessage.ReasonPhrase };
                }
            }
            catch (TaskCanceledException)
            {
                return new VerifyVm() { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = "Gateway Timeout" };
            }
        }

        public async Task<SettleVm> SettleTrx(SettleCommand settleCommand)
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri(REST_URL),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", System.Configuration.ConfigurationManager.AppSettings["MerchantUsd"]);
            client.DefaultRequestHeaders.Add("pwd", System.Configuration.ConfigurationManager.AppSettings["MerchantPass"]);
            var content = new StringContent(JsonConvert.SerializeObject(settleCommand), Encoding.UTF8, "application/json");

            try
            {
                var responseMessage = (await client.PostAsync($"v1/Settlement", content, new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        return new SettleVm() { ResCode = 0, ResMessage = "settlement request succeeded" };
                    case 400:
                        return new SettleVm() { ResCode = 400, ResMessage = "bad request" };
                    case 401:
                        return new SettleVm() { ResCode = 401, ResMessage = "unauthorized. probably wrong or unsent header(s)" };
                    case 471:
                        return new SettleVm() { ResCode = 471, ResMessage = "original transaction was failed" };
                    case 472:
                        return new SettleVm() { ResCode = 472, ResMessage = "transaction not verified" };
                    case 473:
                        return new SettleVm() { ResCode = 473, ResMessage = "transaction already requested for reversal" };
                    case 474:
                        return new SettleVm() { ResCode = 474, ResMessage = "transaction already requested for reconcilation" };
                    case 475:
                        return new SettleVm() { ResCode = 475, ResMessage = "transaction already listed for reversal" };
                    case 476:
                        return new SettleVm() { ResCode = 476, ResMessage = "transaction already listed for reconcilation" };
                    case 477:
                        return new SettleVm() { ResCode = 477, ResMessage = "identity not trusted to proceed" };
                    case 478:
                        return new SettleVm() { ResCode = 478, ResMessage = "verification already cancelled" };
                    case 571:
                        return new SettleVm() { ResCode = 571, ResMessage = "not yet processed" };
                    case 572:
                        return new SettleVm() { ResCode = 572, ResMessage = "transaction status undetermined" };
                    case 573:
                        return new SettleVm() { ResCode = 573, ResMessage = "unable to request for reconcilation due to an internal error" };
                    default:
                        return new SettleVm() { ResCode = (int)responseMessage.StatusCode, ResMessage = responseMessage.ReasonPhrase };
                }
            }
            catch (TaskCanceledException)
            {
                return new SettleVm() { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = "Gateway Timeout" };
            }
        }

        public async Task<ReverseVm> ReverseTrx(ReverseCommand reverseCommand)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(REST_URL),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", System.Configuration.ConfigurationManager.AppSettings["MerchantUsd"]);
            client.DefaultRequestHeaders.Add("pwd", System.Configuration.ConfigurationManager.AppSettings["MerchantPass"]);
            var content = new StringContent(JsonConvert.SerializeObject(reverseCommand), Encoding.UTF8, "application/json");

            try
            {
                var responseMessage = (await client.PostAsync($"v1/Reverse", content,
                                       new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        return new ReverseVm() { ResCode = 0, ResMessage = "reversing  request succeeded" };
                    case 400:
                        return new ReverseVm() { ResCode = 400, ResMessage = "bad request" };
                    case 401:
                        return new ReverseVm() { ResCode = 401, ResMessage = "unauthorized. probably wrong or unsent header(s)" };
                    case 471:
                        return new ReverseVm() { ResCode = 471, ResMessage = "original transaction was failed" };
                    case 472:
                        return new ReverseVm() { ResCode = 472, ResMessage = "cant reverse a verified transaction" };
                    case 473:
                        return new ReverseVm() { ResCode = 473, ResMessage = "transaction already requested for reversal" };
                    case 474:
                        return new ReverseVm() { ResCode = 474, ResMessage = "transaction already requested for reconcilation" };
                    case 475:
                        return new ReverseVm() { ResCode = 475, ResMessage = "transaction already listed for reversal" };
                    case 476:
                        return new ReverseVm() { ResCode = 476, ResMessage = "transaction already listed for reconcilation" };
                    case 477:
                        return new ReverseVm() { ResCode = 477, ResMessage = "identity not trusted to proceed" };

                    case 571:
                        return new ReverseVm() { ResCode = 571, ResMessage = "not yet processed" };
                    case 572:
                        return new ReverseVm() { ResCode = 572, ResMessage = "transaction status undetermined" };
                    case 573:
                        return new ReverseVm() { ResCode = 573, ResMessage = "unable to request for reversal  due to an internal error" };
                    default:
                        return new ReverseVm() { ResCode = (int)responseMessage.StatusCode, ResMessage = responseMessage.ReasonPhrase };
                }
            }
            catch (TaskCanceledException)
            {
                return new ReverseVm() { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = "Gateway Timeout" };
            }
        }

        public async Task<PaymentResultVm> TranResult(int merchantConfigId, long localInvoiceId)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(REST_URL),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", System.Configuration.ConfigurationManager.AppSettings["MerchantUsd"]);
            client.DefaultRequestHeaders.Add("pwd", System.Configuration.ConfigurationManager.AppSettings["MerchantPass"]);
            PaymentResultVm paymentResultVm;
            try
            {
                var responseMessage = (await client.GetAsync($"v1/TranResult?LocalInvoiceId={localInvoiceId}" +
                                                             $"&MerchantConfigurationId={merchantConfigId}",
                                                              new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        var content = await responseMessage.Content.ReadAsStringAsync();

                        paymentResultVm = JsonConvert.DeserializeObject<PaymentResultVm>(content);
                        paymentResultVm.ResCode = 0;
                        paymentResultVm.ResMessage = responseMessage.ReasonPhrase;
                        return paymentResultVm;
                    case 472:
                        return new PaymentResultVm() { ResCode = 472, ResMessage = "no records found" };
                    case 408:
                        return new PaymentResultVm() { ResCode = 471, ResMessage = "identity not trusted to proceed" };
                    case 504:
                        return new PaymentResultVm() { ResCode = 401, ResMessage = "unauthorized. probably wrong or unsent header(s)" };

                    case 571:
                        return new PaymentResultVm() { ResCode = 571, ResMessage = "error in processing" };
                    default:
                        return new PaymentResultVm() { ResCode = (short)responseMessage.StatusCode, ResMessage = responseMessage.ReasonPhrase };
                }
            }
            catch (TaskCanceledException)
            {
                return new PaymentResultVm() { ResCode = 408, ResMessage = "Timeout By WebPay Application" };
            }
            catch (Exception ex)
            {
                return new PaymentResultVm() { ResCode = 500, ResMessage = ex.Message };
            }

        }
    }
}