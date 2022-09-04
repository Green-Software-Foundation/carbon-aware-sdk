import requests
from settings import *

register_url = 'https://api2.watttime.org/v2/register'
params = {'username': wt_username,
         'password': wt_password,
         'email': wt_email,
         'org': wt_org}
rsp = requests.post(register_url, json=params)
print(rsp.text)