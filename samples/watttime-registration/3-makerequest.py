import requests
from requests.auth import HTTPBasicAuth
from settings import *
from datetime import date

login_url = 'https://api2.watttime.org/v2/login'
token = requests.get(login_url, auth=HTTPBasicAuth(wt_username, wt_password)).json()['token']

data_url = 'https://api2.watttime.org/v2/data'
headers = {'Authorization': 'Bearer {}'.format(token)}

last_year = str(date.today().year - 1)
starttime = last_year + '-02-20T16:00:00-0800'
endtime   = last_year + '-02-20T16:15:00-0800'

params = {'ba': 'CAISO_NORTH', 
          'starttime': starttime, 
          'endtime': endtime}
rsp = requests.get(data_url, headers=headers, params=params)
print(rsp.text)