# EmissionsForecastBatchDTO


## Properties
Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**requested_at** | **datetime** | For historical forecast requests, this value is the timestamp used to access the most  recently generated forecast as of that time. | 
**location** | **str** | The location of the forecast | 
**data_start_at** | **datetime** | Start time boundary of forecasted data points. Ignores forecast data points before this time.  Defaults to the earliest time in the forecast data. | [optional] 
**data_end_at** | **datetime** | End time boundary of forecasted data points. Ignores forecast data points after this time.  Defaults to the latest time in the forecast data. | [optional] 
**window_size** | **int** | The estimated duration (in minutes) of the workload.  Defaults to the duration of a single forecast data point. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


