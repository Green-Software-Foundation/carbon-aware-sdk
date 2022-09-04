import requests
from requests.auth import HTTPBasicAuth
from settings import *

login_url = 'https://api2.watttime.org/v2/login'
rsp = requests.get(login_url, auth=HTTPBasicAuth(wt_username, wt_password))
print(rsp.json())
