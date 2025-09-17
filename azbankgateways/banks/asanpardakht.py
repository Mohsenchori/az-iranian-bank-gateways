import logging
import requests
from azbankgateways.banks import BaseBank

# Create a named logger for azbankgateways
logger = logging.getLogger('azbankgateways.banks.asanpardakht')
from azbankgateways.exceptions import (
    BankGatewayConnectionError,
    BankGatewayRejectPayment,
    SettingDoesNotExist,
)
from azbankgateways.models import BankType, CurrencyEnum, PaymentStatus
from azbankgateways.utils import get_json


class AsanPardakht(BaseBank):
    _merchant_configuration_id = None
    _username = None
    _password = None

    def __init__(self, **kwargs):
        logger.info("AsanPardakht bank initialization started")
        super(AsanPardakht, self).__init__(**kwargs)

        self.set_gateway_currency(CurrencyEnum.IRR)
        self._token_api_url = "https://ipgrest.asanpardakht.ir/v1/Token"
        # Note: If you get gateway errors, try these alternative URLs:
        # self._payment_url = "https://ipay.asanpardakht.ir/"
        # self._payment_url = "https://asan.shaparak.ir/pay/"
        self._payment_url = "https://asan.shaparak.ir"
        self._verify_api_url = "https://ipgrest.asanpardakht.ir/v1/Verify"
        logger.info("AsanPardakht bank initialization completed")

    def get_bank_type(self):
        logger.debug("AsanPardakht get_bank_type called, returning ASANPARDAKHT")
        return BankType.ASANPARDAKHT

    def set_default_settings(self):
        required_settings = ["MERCHANT_CONFIGURATION_ID", "USERNAME", "PASSWORD"]
        for item in required_settings:
            if item not in self.default_setting_kwargs:
                raise SettingDoesNotExist(f"{item} is not set in settings.")

            setattr(self, f"_{item.lower()}", self.default_setting_kwargs[item])

    def get_pay_data(self):
        # Get the base callback URL
        callback_url = self._get_gateway_callback_url()
        
        # Add invoice parameter to callback URL (as per official examples)
        separator = '&' if '?' in callback_url else '?'
        enhanced_callback_url = f"{callback_url}{separator}invoice={self.get_tracking_code()}"
        
        data = {
            "serviceTypeId": 1,
            "merchantConfigurationId": self._merchant_configuration_id,
            "localInvoiceId": self.get_tracking_code(),
            "amountInRials": self.get_gateway_amount(),
            "localDate": self._get_local_date(),
            "callbackURL": enhanced_callback_url,
            "paymentId": "0",  # According to official examples, this should be "0"
            "settlementPortions": []  # Required field from official Postman collection
        }
        logger.debug(f"Enhanced callback URL: {enhanced_callback_url}")
        return data

    def prepare_pay(self):
        super(AsanPardakht, self).prepare_pay()

    def pay(self):
        logger.info("AsanPardakht pay method called")
        super(AsanPardakht, self).pay()
        data = self.get_pay_data()
        logger.debug(f"AsanPardakht pay data: {data}")
        headers = {

            "usr": self._username,
            "pwd": self._password,
        }

        logger.info(f"Sending token request to: {self._token_api_url}")
        token = self._send_request(self._token_api_url, data, headers, as_json=False)
        print('||||||||||||||||||||||||||||||||||||||||',token)
        logger.info(f"Received token: {token}")
        
        if token and token.strip():
            # Clean the token (remove any extra whitespace or quotes)
            clean_token = token.strip()
            if clean_token.startswith('"') and clean_token.endswith('"'):
                clean_token = clean_token[1:-1]
            
            self._set_reference_number(clean_token)
            logger.info(f"Clean token set as reference number: {clean_token}")
            
            # Log the final payment details
            payment_url = self._get_gateway_payment_url_parameter()
            payment_params = self._get_gateway_payment_parameter()
            logger.info(f"Payment URL: {payment_url}")
            logger.info(f"Payment parameters: {payment_params}")
        else:
            status_text = "Failed to retrieve token from Asan Pardakht"
            self._set_transaction_status_text(status_text)
            logger.critical(status_text)
            raise BankGatewayRejectPayment(self.get_transaction_status_text())

    def _get_status_text(self, status):
        status_codes = {
            "0": "Transaction successful",
            "1": "Issuer declined transaction",
            "2": "Transaction already confirmed",
            "3": "Invalid merchant",
            "4": "Card captured",
            "5": "Transaction not processed",
            "6": "Error occurred",
            "12": "Invalid transaction",
            "13": "Incorrect correction amount",
            "14": "Invalid card number",
            "15": "Invalid issuer",
            "16": "Transaction approved, update track 3",
            "19": "Resend transaction",
            "23": "Invalid commission amount",
            "25": "Original transaction not found",
            "30": "Message format error",
            "31": "Merchant not supported by switch",
            "33": "Card expiration date exceeded",
            "34": "Transaction not successfully completed",
            "36": "Card restricted",
            "38": "Too many incorrect PIN entries",
            "39": "Credit card account not found",
            "40": "Requested operation not supported",
            "41": "Lost card, capture card",
            "43": "Stolen card, capture card",
            "51": "Insufficient funds",
            "54": "Card expiration date exceeded",
            "55": "Invalid card PIN",
            "57": "Transaction not allowed",
            "61": "Transaction amount exceeds limit",
            "63": "Security violation",
            "65": "Too many transaction attempts",
            "75": "Too many incorrect PIN attempts",
            "77": "Invalid transaction date",
            "78": "Card inactive",
            "79": "Invalid linked account",
            "80": "Transaction unsuccessful",
            "84": "Card issuer not responding",
            "86": "Transaction destination in off sign mode",
            "90": "Card issuer performing end-of-day operations",
            "92": "No routing to destination",
            "94": "Duplicate transaction",
            "96": "System error occurred",
            "97": "Issuer or acquirer performing key change"
        }
        return status_codes.get(status, "Unknown error")

    def _get_gateway_payment_url_parameter(self):
        # AsanPardakht official documentation specifies https://asan.shaparak.ir
        # Note: The official docs don't include trailing slash
        return "https://asan.shaparak.ir"

    def _get_gateway_payment_method_parameter(self):
        # AsanPardakht requires POST method according to official examples
        return "POST"

    def _get_gateway_payment_parameter(self):
        # According to official C# example, AsanPardakht only needs RefId and mobileap
        # The token parameter is NOT needed despite what support said
        token = self.get_reference_number()
        params = {
            "RefId": token,  # This is the token we received from the Token API
        }
        # Add mobile number if available (optional parameter)
        mobile_number = getattr(self, 'mobile_number', None)
        if mobile_number:
            params["mobileap"] = mobile_number
        else:
            # Add empty mobile parameter as per official implementation
            params["mobileap"] = "09016251030"
        
        logger.debug(f"Payment parameters: {params}")
        logger.info(f"Final redirect will be POST to https://asan.shaparak.ir with RefId: {token}")
        return params

    def prepare_verify_from_gateway(self):
        logger.info("AsanPardakht prepare_verify_from_gateway called")
        super(AsanPardakht, self).prepare_verify_from_gateway()
        request = self.get_request()
        
        # AsanPardakht sends back the payment through callback URL with invoice parameter
        # We need to call TranResult API to get the actual transaction details
        invoice_id = request.GET.get("invoice") or request.POST.get("invoice")
        
        if invoice_id:
            logger.debug(f"Received callback with invoice ID: {invoice_id}")
            # Call TranResult API to get transaction details
            try:
                tran_result = self._get_transaction_result(invoice_id)
                if tran_result and 'payGateTranID' in tran_result:
                    self._set_reference_number(tran_result['payGateTranID'])
                    logger.info(f"Transaction result: {tran_result}")
                    
                    # Check if transaction was successful
                    if tran_result.get('respCode') == '0' or tran_result.get('resCode') == '0':
                        logger.info("Payment successful according to TranResult")
                        self._bank.extra_information = f"TranResult={tran_result}"
                        self._bank.save()
                    else:
                        logger.error(f"Payment failed according to TranResult: {tran_result}")
                        self._set_payment_status(PaymentStatus.CANCEL_BY_USER)
                else:
                    logger.error("Failed to get transaction result")
                    self._set_payment_status(PaymentStatus.ERROR)
            except Exception as e:
                logger.exception(f"Error getting transaction result: {e}")
                self._set_payment_status(PaymentStatus.ERROR)
        else:
            # Fallback to old method if invoice parameter not found
            ref_id = request.POST.get("RefId") or self.get_tracking_code()
            res_code = request.POST.get("ResCode")
            logger.debug(f"Fallback: Received callback - RefId: {ref_id}, ResCode: {res_code}")
            self._set_reference_number(ref_id)
            self._set_bank_record()
            if res_code == "0":
                logger.info("Payment successful - ResCode is 0")
                self._bank.extra_information = f"ResCode={res_code}, RefId={ref_id}"
                self._bank.save()
            else:
                logger.error(f"Payment failed with ResCode: {res_code}")
                self._set_payment_status(PaymentStatus.CANCEL_BY_USER)

    def verify_from_gateway(self, request):
        super(AsanPardakht, self).verify_from_gateway(request)

    def get_verify_data(self):
        data = {
            "merchantConfigurationId": self._merchant_configuration_id,
            "payGateTranId": self.get_reference_number(),
        }
        return data

    def prepare_verify(self, tracking_code):
        super(AsanPardakht, self).prepare_verify(tracking_code)

    def verify(self, transaction_code):
        super(AsanPardakht, self).verify(transaction_code)
        data = self.get_verify_data()
        headers = {

            "usr": self._username,
            "pwd": self._password,
        }
        response_json = self._send_request(self._verify_api_url, data, headers)
        if response_json.get("IsSuccess"):
            self._set_payment_status(PaymentStatus.COMPLETE)
        else:
            self._set_payment_status(PaymentStatus.CANCEL_BY_USER)
            logger.error("Asan Pardakht verification failed.")

    def _send_request(self, api_url, data, headers, as_json=True):
        logger.debug(f"Sending request to {api_url} with data: {data}")
        try:
            response = requests.post(api_url, json=data, headers=headers, timeout=10)
            logger.debug(f"Response status code: {response.status_code}")
            
            # Log response content for debugging
            if response.status_code != 200:
                logger.warning(f"Non-200 response: {response.status_code} - {response.text}")
            
            response.raise_for_status()
            logger.debug(f"Request successful, status code: {response.status_code}")
        except requests.Timeout:
            logger.exception(f"Asan Pardakht gateway timeout: {data}")
            raise BankGatewayConnectionError("AsanPardakht gateway timeout")
        except requests.ConnectionError:
            logger.exception(f"Asan Pardakht gateway connection error: {data}")
            raise BankGatewayConnectionError("AsanPardakht gateway connection error")
        except requests.HTTPError as e:
            error_msg = f"HTTP error occurred: {e} - Response: {response.text if 'response' in locals() else 'No response'}"
            logger.exception(error_msg)
            
            # Handle specific server errors with more helpful messages
            if response.status_code == 507:
                raise BankGatewayConnectionError("AsanPardakht server storage error (507). Please try again later.")
            elif response.status_code >= 500:
                raise BankGatewayConnectionError(f"AsanPardakht server error ({response.status_code}). Please try again later.")
            else:
                raise BankGatewayConnectionError(f"AsanPardakht API error: {error_msg}")

        if as_json:
            response_json = get_json(response)
            logger.debug(f"Response JSON: {response_json}")
            self._set_transaction_status_text(response_json.get("Message"))
            return response_json
        else:
            logger.debug(f"Response text: {response.text}")
            return response.text  

    def _get_transaction_result(self, invoice_id):
        """
        Call AsanPardakht TranResult API to get transaction details
        Based on official PHP example
        """
        url = f"https://ipgrest.asanpardakht.ir/v1/TranResult"
        params = {
            'merchantConfigurationId': self._merchant_configuration_id,
            'localInvoiceId': invoice_id
        }
        headers = {
            'usr': self._username,
            'pwd': self._password
        }
        
        logger.debug(f"Getting transaction result for invoice: {invoice_id}")
        try:
            response = requests.get(url, params=params, headers=headers, timeout=10)
            logger.debug(f"TranResult response status: {response.status_code}")
            
            if response.status_code == 200:
                result = response.json()
                logger.debug(f"TranResult response: {result}")
                return result
            else:
                logger.error(f"TranResult failed: {response.status_code} - {response.text}")
                return None
                
        except Exception as e:
            logger.exception(f"Error calling TranResult API: {e}")
            return None

    def _get_local_date(self):
        """
        Get current date/time in the exact format required by AsanPardakht
        Format: YYYYMMDD HHMMSS (without quotes)
        Example from official docs: 20250917 112849
        """
        url = 'https://ipgrest.asanpardakht.ir/v1/Time'
        headers = {
            'usr': self._username,  
            'pwd': self._password 
        }

        logger.debug(f"Getting server time from: {url}")
        try:
            response = requests.get(url, headers=headers, timeout=10)
            
            if response.status_code == 200:
                server_time = response.text.strip()
                # Remove quotes if they exist (API sometimes returns with quotes)
                if server_time.startswith('"') and server_time.endswith('"'):
                    server_time = server_time[1:-1]
                logger.debug(f"Server time received: {server_time}")
                return server_time
            else:
                error_msg = f"Failed to retrieve server time: {response.status_code}, {response.text}"
                logger.error(error_msg)
                raise Exception(error_msg)
                
        except Exception as e:
            logger.error(f"Error getting server time: {e}")
            # Fallback to local time in the correct format if server time fails
            from datetime import datetime
            local_time = datetime.now().strftime('%Y%m%d %H%M%S')
            logger.warning(f"Using local time as fallback: {local_time}")
            return local_time
