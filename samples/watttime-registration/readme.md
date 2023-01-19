# WattTime account creation

In order to create an account for watttime, we have set up these sample requests
to help you get set up very quickly.

> Note these steps reflect the documentation at
> <https://www.watttime.org/api-documentation/#best-practices-for-api-usage>
> This sample is in python

Please follow these steps to get started quickly:

## 1 - Update settings

Copy the template settings file to a new `settings.py` file and edit the
parameters.

### 1.1 Copy

To copy you can use:

```sh
cp ./settings.py.template settings.py
```

### 1.2 Update settings.py

Update the file to use your desired username, password and other values needed.

## 2 - Register.py

```sh
python ./1-register.py
```

Success should look like the following:

```json
{ "user": "gsf_myusername", "ok": "User created" }
```

## 3 - Test

Your account should now have been created and you should be able to run the
following tests.

### 3.1 - Request token

```sh
python ./2-requesttoken.py
```

Success should look like the following(token was truncated in result below):

```json
{ "token": "eyJhbGci...." }
```

### 3.2 - Make a request

You could make a request for data from watttime. Run:

```sh
python ./3-makerequest.py
```

Success should look like the following:

```json
[
  {
    "point_time": "2019-02-21T00:15:00.000Z",
    "value": 1062.0,
    "frequency": null,
    "market": "RTM",
    "ba": "CAISO_NORTH",
    "datatype": "MOER",
    "version": "3.0"
  },
  {
    "point_time": "2019-02-21T00:10:00.000Z",
    "value": 1050.0,
    "frequency": null,
    "market": "RTM",
    "ba": "CAISO_NORTH",
    "datatype": "MOER",
    "version": "3.0"
  },
  {
    "point_time": "2019-02-21T00:05:00.000Z",
    "value": 1019.0,
    "frequency": null,
    "market": "RTM",
    "ba": "CAISO_NORTH",
    "datatype": "MOER",
    "version": "3.0"
  },
  {
    "point_time": "2019-02-21T00:00:00.000Z",
    "value": 927.0,
    "frequency": null,
    "market": "RTM",
    "ba": "CAISO_NORTH",
    "datatype": "MOER",
    "version": "3.0"
  }
]
```
