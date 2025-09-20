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
        
        # Initialize payment verification tracking
        self._payment_verified = None
        self._settlement_data = None
        
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
        
        # Check if we have PayGateTranID in POST data (direct callback from AsanPardakht)
        pay_gate_tran_id = request.POST.get("PayGateTranID")
        
        if invoice_id:
            logger.debug(f"Received callback with invoice ID: {invoice_id}")
            
            # Find bank record by tracking code (invoice_id should match tracking_code)
            try:
                from azbankgateways.models import Bank
                self._bank = Bank.objects.get(
                    tracking_code=invoice_id,
                    bank_type=self.get_bank_type()
                )
                logger.debug(f"Found bank record with tracking code: {invoice_id}")
                # Set the tracking code so it's available for callback URL generation
                self._set_tracking_code(self._bank.tracking_code)
            except Bank.DoesNotExist:
                logger.error(f"Bank record not found for invoice ID: {invoice_id}")
                # Create a minimal bank record to avoid crashes
                from azbankgateways.models import Bank
                self._bank = Bank.objects.create(
                    tracking_code=invoice_id,
                    bank_type=self.get_bank_type(),
                    status=PaymentStatus.ERROR,
                    amount=0
                )
                # Set the tracking code for this case too
                self._set_tracking_code(invoice_id)
                return
            
            # If we have PayGateTranID from callback, use it directly
            if pay_gate_tran_id:
                logger.info(f"Using PayGateTranID from callback: {pay_gate_tran_id}")
                self._set_reference_number(pay_gate_tran_id)
                self._bank.extra_information = f"PayGateTranID={pay_gate_tran_id}"
                self._bank.save()
                self._payment_verified = True
                return
            
            # Call TranResult API to get transaction details
            try:
                tran_result = self._get_transaction_result(invoice_id)
                if tran_result and 'payGateTranID' in tran_result:
                    self._set_reference_number(tran_result['payGateTranID'])
                    logger.info(f"Transaction result: {tran_result}")
                    
                    # Debug: Print all keys in the response
                    print(f"=== TRANSRESULT KEYS: {list(tran_result.keys())} ===")
                    logger.info(f"=== TRANSRESULT KEYS: {list(tran_result.keys())} ===")
                    
                    # Extract and log card number if available
                    card_number = tran_result.get('cardNumber')
                    print(f"=== CARD NUMBER VALUE: '{card_number}' (type: {type(card_number)}) ===")
                    
                    if card_number and card_number != "string" and card_number.strip():
                        print(f"=== CARD NUMBER FROM TRANSRESULT: {card_number} ===")
                        logger.info(f"Card Number: {card_number}")
                        # Save card number to bank record
                        card_info = f"CardNumber={card_number}"
                        if self._bank.extra_information:
                            self._bank.extra_information += f", {card_info}"
                        else:
                            self._bank.extra_information = card_info
                    else:
                        print(f"=== NO VALID CARD NUMBER FOUND - Value was: '{card_number}' ===")
                        logger.warning(f"No valid card number found in TranResult - Value was: '{card_number}'")
                    
                    # Check if transaction was successful
                    # AsanPardakht TranResult API uses serviceStatusCode: '1' for successful payments
                    service_status = tran_result.get('serviceStatusCode')
                    if service_status == '1':
                        logger.info("+++++++Payment successful according to TranResult - serviceStatusCode: 1")
                        # Don't set payment status here - let base class handle state transitions
                        if not self._bank.extra_information:
                            self._bank.extra_information = f"TranResult={tran_result}"
                        else:
                            self._bank.extra_information += f", TranResult={tran_result}"
                        self._bank.save()
                        
                        # Store settlement data for later use
                        self._settlement_data = tran_result
                        self._payment_verified = True
                    else:
                        logger.error(f"Payment failed according to TranResult - serviceStatusCode: {service_status}")
                        self._payment_verified = False
                else:
                    logger.error("Failed to get transaction result")
                    self._payment_verified = False
            except Exception as e:
                logger.exception(f"Error getting transaction result: {e}")
                self._payment_verified = False
        else:
            # Fallback to old method if invoice parameter not found
            ref_id = request.POST.get("RefId") or self.get_tracking_code()
            res_code = request.POST.get("ResCode")
            logger.debug(f"Fallback: Received callback - RefId: {ref_id}, ResCode: {res_code}")
            self._set_reference_number(ref_id)
            self._set_bank_record()
            if self._bank is None:
                logger.error("Could not find bank record in fallback method")
                return
                
            if res_code == "0":
                logger.info("Payment successful - ResCode is 0")
                self._bank.extra_information = f"ResCode={res_code}, RefId={ref_id}"
                self._bank.save()
                self._payment_verified = True
            else:
                logger.error(f"Payment failed with ResCode: {res_code}")
                self._payment_verified = False

    def verify_from_gateway(self, request):
        super(AsanPardakht, self).verify_from_gateway(request)
        
        # After base class sets RETURN_FROM_BANK status, set the final status and settle
        if hasattr(self, '_payment_verified') and self._payment_verified:
            logger.info("Setting final payment status to COMPLETE and settling")
            self._set_payment_status(PaymentStatus.COMPLETE)
            
            # Check if we have PayGateTranID directly from callback
            if hasattr(self._bank, 'extra_information') and self._bank.extra_information and 'PayGateTranID=' in self._bank.extra_information:
                logger.info("Using PayGateTranID from callback, calling verify then settlement")
                # Extract PayGateTranID from extra_information
                try:
                    pay_gate_tran_id = self._bank.extra_information.split('PayGateTranID=')[1]
                    logger.debug(f"Extracted PayGateTranID: {pay_gate_tran_id}")
                    
                    # IMPORTANT: Must verify first before settlement (AsanPardakht requirement)
                    verify_success = self._verify_transaction_with_id(pay_gate_tran_id)
                    if verify_success:
                        # Now settle after verification
                        settlement_data = {'payGateTranID': pay_gate_tran_id}
                        settlement_result = self._settle_payment(settlement_data)
                        if settlement_result:
                            logger.info("Payment successfully verified and settled with PayGateTranID from callback")
                        else:
                            logger.warning("Settlement failed with PayGateTranID from callback after verification")
                    else:
                        logger.error("Verification failed for PayGateTranID from callback")
                except Exception as e:
                    logger.error(f"Error processing PayGateTranID for verification and settlement: {e}")
                return
            
            # IMPORTANT: Must call Verify API before Settlement API (AsanPardakht requirement)
            verify_success = self._verify_transaction()
            if verify_success:
                # Now settle the payment after verification
                if hasattr(self, '_settlement_data'):
                    settlement_result = self._settle_payment(self._settlement_data)
                else:
                    settlement_result = self._settle_payment_fallback()
                    
                if settlement_result:
                    logger.info("Payment successfully verified and settled with AsanPardakht")
                else:
                    logger.warning("Settlement failed after verification - transaction may be reversed automatically")
            else:
                logger.error("Payment verification failed - cannot proceed with settlement")
        elif hasattr(self, '_payment_verified') and not self._payment_verified:
            logger.info("Setting final payment status to CANCEL_BY_USER")
            self._set_payment_status(PaymentStatus.CANCEL_BY_USER)

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
        
        # If we already processed the payment via TranResult API in prepare_verify_from_gateway,
        # skip the standard verification to avoid overriding the correct status
        if hasattr(self._bank, 'extra_information') and self._bank.extra_information and ('TranResult=' in self._bank.extra_information or 'PayGateTranID=' in self._bank.extra_information):
            logger.info("Payment already verified via TranResult API or callback data, skipping standard verification")
            return
            
        # Standard verification using AsanPardakht Verify API (fallback)
        verify_success = self._verify_transaction()
        if verify_success:
            self._set_payment_status(PaymentStatus.COMPLETE)
            
            # IMPORTANT: Call settlement API to finalize the transaction after verification
            settlement_result = self._settle_payment_fallback()
            if settlement_result:
                logger.info("Payment successfully verified and settled via Verify API")
            else:
                logger.warning("Settlement failed after verification via Verify API - transaction may be reversed")
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
                
                # Debug: Print all response data
                print(f"=== FULL TRANSRESULT RESPONSE: {result} ===")
                
                # Extract and log card number from TranResult
                if 'cardNumber' in result:
                    card_number = result['cardNumber']
                    print(f"=== CARD NUMBER FROM TRANSRESULT API: '{card_number}' (type: {type(card_number)}) ===")
                    logger.info(f"Card Number extracted from TranResult: {card_number}")
                else:
                    print(f"=== NO 'cardNumber' KEY IN TRANSRESULT RESPONSE ===")
                    logger.warning("No 'cardNumber' key found in TranResult response")
                
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

    def _verify_transaction(self):
        """
        Call AsanPardakht Verify API to verify the transaction before settlement
        This is required by AsanPardakht before calling Settlement API
        """
        url = 'https://ipgrest.asanpardakht.ir/v1/Verify'
        
        # Get reference number and validate it's a numeric transaction ID
        ref_number = self.get_reference_number()
        try:
            transaction_id = int(ref_number)
        except (ValueError, TypeError):
            logger.error(f"Invalid transaction ID for verification: {ref_number} (not numeric)")
            return False
        
        data = {
            'merchantConfigurationId': int(self._merchant_configuration_id),
            'payGateTranId': transaction_id
        }
        headers = {
            'usr': self._username,
            'pwd': self._password,
            'Content-Type': 'application/json'
        }
        
        logger.debug(f"Verifying transaction: {transaction_id}")
        logger.debug(f"Verify request data: {data}")
        
        try:
            response = requests.post(url, json=data, headers=headers, timeout=10)
            logger.debug(f"Verify response status: {response.status_code}")
            logger.debug(f"Verify response text: '{response.text}'")
            
            if response.status_code == 200:
                # Handle empty response (AsanPardakht sometimes returns empty body for successful verification)
                if not response.text or response.text.strip() == '':
                    logger.info("Transaction verification successful (empty response indicates success)")
                    return True
                
                try:
                    result = response.json()
                    logger.debug(f"Verify response: {result}")
                    
                    # Check if verification was successful
                    if result.get('IsSuccess') == True:
                        logger.info("Transaction verification successful")
                        return True
                    else:
                        logger.error(f"Transaction verification failed: {result}")
                        return False
                except ValueError as e:
                    # If JSON parsing fails but status is 200, consider it successful
                    logger.warning(f"JSON parsing failed for verification response, but status 200 - treating as success: {e}")
                    return True
            else:
                logger.error(f"Verify request failed: {response.status_code} - {response.text}")
                return False
                
        except Exception as e:
            logger.exception(f"Error calling Verify API: {e}")
            return False

    def _verify_transaction_with_id(self, transaction_id):
        """
        Call AsanPardakht Verify API to verify a specific transaction ID before settlement
        This is required by AsanPardakht before calling Settlement API
        """
        url = 'https://ipgrest.asanpardakht.ir/v1/Verify'
        
        # Validate the transaction ID is numeric
        try:
            transaction_id = int(transaction_id)
        except (ValueError, TypeError):
            logger.error(f"Invalid transaction ID for verification: {transaction_id} (not numeric)")
            return False
        
        data = {
            'merchantConfigurationId': int(self._merchant_configuration_id),
            'payGateTranId': transaction_id
        }
        headers = {
            'usr': self._username,
            'pwd': self._password,
            'Content-Type': 'application/json'
        }
        
        logger.debug(f"Verifying specific transaction: {transaction_id}")
        logger.debug(f"Verify request data: {data}")
        
        try:
            response = requests.post(url, json=data, headers=headers, timeout=10)
            logger.debug(f"Verify response status: {response.status_code}")
            logger.debug(f"Verify response text: '{response.text}'")
            
            if response.status_code == 200:
                # Handle empty response (AsanPardakht sometimes returns empty body for successful verification)
                if not response.text or response.text.strip() == '':
                    logger.info(f"Transaction {transaction_id} verification successful (empty response indicates success)")
                    return True
                
                try:
                    result = response.json()
                    logger.debug(f"Verify response: {result}")
                    
                    # Check if verification was successful
                    if result.get('IsSuccess') == True:
                        logger.info(f"Transaction {transaction_id} verification successful")
                        return True
                    else:
                        logger.error(f"Transaction {transaction_id} verification failed: {result}")
                        return False
                except ValueError as e:
                    # If JSON parsing fails but status is 200, consider it successful
                    logger.warning(f"JSON parsing failed for verification response, but status 200 - treating as success: {e}")
                    return True
            else:
                logger.error(f"Verify request failed: {response.status_code} - {response.text}")
                return False
                
        except Exception as e:
            logger.exception(f"Error calling Verify API for transaction {transaction_id}: {e}")
            return False

    def _settle_payment(self, tran_result):
        """
        Call AsanPardakht Settlement API to finalize the transaction
        This must be called after successful payment to prevent automatic reversal
        
        Official API: POST https://ipgrest.asanpardakht.ir/v1/Settlement
        Body: {"merchantConfigurationId": 0, "payGateTranId": 0}
        """
        url = 'https://ipgrest.asanpardakht.ir/v1/Settlement'
        
        # Use the exact API specification - only merchantConfigurationId and payGateTranId
        data = {
            'merchantConfigurationId': int(self._merchant_configuration_id),
            'payGateTranId': int(tran_result.get('payGateTranID'))
        }
        headers = {
            'usr': self._username,
            'pwd': self._password,
            'Content-Type': 'application/json'
        }
        
        logger.debug(f"Settling payment for transaction: {tran_result.get('payGateTranID')}")
        logger.debug(f"Settlement request data: {data}")
        
        try:
            response = requests.post(url, json=data, headers=headers, timeout=10)
            logger.debug(f"Settlement response status: {response.status_code}")
            logger.debug(f"Settlement response text: '{response.text}'")
            
            if response.status_code == 200:
                # Handle empty response (AsanPardakht sometimes returns empty body for successful settlement)
                if not response.text or response.text.strip() == '':
                    logger.info("Settlement successful (empty response indicates success)")
                    return True
                
                try:
                    result = response.json()
                    logger.debug(f"Settlement response: {result}")
                    
                    # Check if settlement was successful
                    if result.get('result') == True or result.get('Result') == True or result.get('IsSuccess') == True:
                        logger.info("Settlement successful")
                        return True
                    else:
                        logger.error(f"Settlement failed: {result}")
                        return False
                except ValueError as e:
                    # If JSON parsing fails but status is 200, consider it successful
                    logger.warning(f"JSON parsing failed for settlement response, but status 200 - treating as success: {e}")
                    return True
            else:
                logger.error(f"Settlement request failed: {response.status_code} - {response.text}")
                return False
                
        except Exception as e:
            logger.exception(f"Error calling Settlement API: {e}")
            return False

    def _settle_payment_fallback(self):
        """
        Fallback settlement method when we don't have full TranResult data
        Uses the reference number from the current transaction
        """
        url = 'https://ipgrest.asanpardakht.ir/v1/Settlement'
        
        # Get reference number and validate it's a numeric transaction ID
        ref_number = self.get_reference_number()
        try:
            transaction_id = int(ref_number)
        except (ValueError, TypeError):
            logger.error(f"Invalid transaction ID for settlement: {ref_number} (not numeric)")
            return False
        
        # Use the exact API specification - only merchantConfigurationId and payGateTranId
        data = {
            'merchantConfigurationId': int(self._merchant_configuration_id),
            'payGateTranId': transaction_id
        }
        headers = {
            'usr': self._username,
            'pwd': self._password,
            'Content-Type': 'application/json'
        }
        
        logger.debug(f"Fallback settling payment for transaction: {transaction_id}")
        logger.debug(f"Settlement request data: {data}")
        
        try:
            response = requests.post(url, json=data, headers=headers, timeout=10)
            logger.debug(f"Settlement response status: {response.status_code}")
            logger.debug(f"Settlement response text: '{response.text}'")
            
            if response.status_code == 200:
                # Handle empty response (AsanPardakht sometimes returns empty body for successful settlement)
                if not response.text or response.text.strip() == '':
                    logger.info("Fallback settlement successful (empty response indicates success)")
                    return True
                
                try:
                    result = response.json()
                    logger.debug(f"Settlement response: {result}")
                    
                    # Check if settlement was successful
                    if result.get('result') == True or result.get('Result') == True or result.get('IsSuccess') == True:
                        logger.info("Fallback settlement successful")
                        return True
                    else:
                        logger.error(f"Fallback settlement failed: {result}")
                        return False
                except ValueError as e:
                    # If JSON parsing fails but status is 200, consider it successful
                    logger.warning(f"JSON parsing failed for fallback settlement response, but status 200 - treating as success: {e}")
                    return True
            else:
                logger.error(f"Fallback settlement request failed: {response.status_code} - {response.text}")
                return False
                
        except Exception as e:
            logger.exception(f"Error calling fallback Settlement API: {e}")
            return False
