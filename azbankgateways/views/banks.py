import logging
from urllib.parse import unquote

from django.http import Http404
from django.shortcuts import redirect, render
from django.urls import reverse
from django.views.decorators.csrf import csrf_exempt

from azbankgateways.bankfactories import BankFactory
from azbankgateways.exceptions import AZBankGatewaysException


@csrf_exempt
def callback_view(request):
    bank_type = request.GET.get("bank_type", None)
    identifier = request.GET.get("identifier", None)

    if not bank_type:
        logging.critical("Bank type is required. but it doesnt send.")
        raise Http404

    # if bank_type == "ASANPARDAKHT":
    #     # Build the redirect URL with all query parameters
    #     try:
    #         custom_callback_url = 'https://rojafon.com/bogzin-payment/v1/asanpardakht-callback/'
    #         # Preserve all original query parameters
    #         query_string = request.GET.urlencode()
    #         if query_string:
    #             custom_callback_url += f"?{query_string}"
    #         return redirect(custom_callback_url)
    #     except Exception as e:
    #         logging.error(f"Failed to redirect to AsanPardakht callback: {e}")
    #         # Fall back to default handling if redirect fails


    factory = BankFactory()
    bank = factory.create(bank_type, identifier=identifier)
    try:
        bank.verify_from_gateway(request)
    except AZBankGatewaysException:
        logging.exception("Verify from gateway failed.", stack_info=True)
    return bank.redirect_client_callback()


@csrf_exempt
def go_to_bank_gateway(request):
    context = {"params": {}}
    for key, value in request.GET.items():
        if key == "url" or key == "method":
            context[key] = unquote(value)
        else:
            context["params"][key] = unquote(value)

    return render(request, "azbankgateways/redirect_to_bank.html", context=context)
