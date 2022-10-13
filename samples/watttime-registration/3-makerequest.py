import requests
from requests.auth import HTTPBasicAuth
from settings import *

login_url = 'https://api2.watttime.org/v2/login'
token = requests.get(login_url, auth=HTTPBasicAuth(wt_username, wt_password)).json()['token']

data_url = 'https://api2.watttime.org/v2/data'
headers = {'Authorization': 'Bearer {}'.format(token)}
params = {'ba': 'CAISO_NORTH', 
          'starttime': '2019-02-20T16:00:00-0800', 
          'endtime': '2019-02-20T16:15:00-0800'}
rsp = requests.get(data_url, headers=headers, params=params)
print(rsp.text)