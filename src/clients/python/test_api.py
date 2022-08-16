import time
import openapi_client
from pprint import pprint
from openapi_client.api import carbon_aware_api
from openapi_client.model.emissions_data import EmissionsData
from  dateutil.parser import parse
# Defining the host is optional and defaults to http://localhost
# See configuration.py for a list of all supported configuration parameters.
configuration = openapi_client.Configuration(
        host = "https://carbon-aware-sdk.azurewebsites.net"
        #host = "http://localhost:5073"
)



# Enter a context with an instance of the API client
with openapi_client.ApiClient(configuration) as api_client:
    # Create an instance of the API class
    api_instance = carbon_aware_api.CarbonAwareApi(api_client)
    location = "westus" # str |  (optional)
    time = parse('2022-07-22T10:30:00.00Z') # datetime |  (optional)
    to_time = parse('2022-07-22T11:00:00.00Z') # datetime |  (optional)
    duration_minutes = 0 # int |  (optional) (default to 0)

    try:
        api_response = api_instance.get_emissions_data_for_location_by_time(location=location, time=time, to_time=to_time, duration_minutes=duration_minutes)

        pprint(api_response)
    except openapi_client.ApiException as e:
        print("Exception when calling CarbonAwareApi->emissions_bylocation_get: %s\n" % e)

