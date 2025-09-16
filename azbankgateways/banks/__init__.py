# Import only BaseBank directly to avoid dependency issues
from .banks import BaseBank  # noqa

# Lazy imports to avoid importing banks with missing dependencies
def _lazy_import(module_name, class_name):
    """Lazy import function to avoid dependency issues"""
    def _import():
        module = __import__(f'azbankgateways.banks.{module_name}', fromlist=[class_name])
        return getattr(module, class_name)
    return _import

# Only import banks that don't have external dependencies or handle gracefully
try:
    from .bahamta import Bahamta  # noqa
except ImportError:
    pass

try:
    from .bmi import BMI  # noqa
except ImportError:
    pass

try:
    from .idpay import IDPay  # noqa
except ImportError:
    pass

try:
    from .mellat import Mellat  # noqa
except ImportError:
    pass

try:
    from .payV1 import PayV1  # noqa
except ImportError:
    pass

try:
    from .sep import SEP  # noqa
except ImportError:
    pass

try:
    from .zarinpal import Zarinpal  # noqa
except ImportError:
    pass

try:
    from .zibal import Zibal  # noqa
except ImportError:
    pass

try:
    from .asanpardakht import AsanPardakht  # noqa
except ImportError:
    pass