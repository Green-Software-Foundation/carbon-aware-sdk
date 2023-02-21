package foundation.greensoftware.carbonawaresdk.samples.java;

import java.net.URL;
import java.net.MalformedURLException;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.List;

import org.openapitools.client.ApiClient;
import org.openapitools.client.ApiException;
import org.openapitools.client.Configuration;
import org.openapitools.client.model.*;
import org.openapitools.client.api.CarbonAwareApi;


public class WebApiClient{

  private final CarbonAwareApi apiInstance;

  public WebApiClient(URL endpoint){
    ApiClient defaultClient = Configuration.getDefaultApiClient();
    defaultClient.setBasePath(endpoint.toString());

    apiInstance = new CarbonAwareApi(defaultClient);
  }

  /**
   * Call /emissions/bylocations/best .
   * Shows result of the best emission data for west/central/east US yesterday.
   */
  public void processBestEmissionsDataByLocations() throws ApiException{
    OffsetDateTime time = OffsetDateTime.now()
                                        .withHour(0)
                                        .withMinute(0)
                                        .withSecond(0)
                                        .withNano(0)
                                        .minusDays(1L);
    OffsetDateTime toTime = time.withHour(23)
                                .withMinute(59)
                                .withSecond(59);
    List<String> locations = new ArrayList<>();
    locations.add("westus");
    locations.add("centralus");
    locations.add("eastus");
    List<EmissionsData> data = apiInstance.getBestEmissionsDataForLocationsByTime(locations, time, toTime);

    data.forEach(System.out::println);
  }

  /**
   * Call /emissions/bylocations .
   * Shows top of result of emission data for west/central/east US yesterday.
   */
  public void processEmissionsDataByLocations() throws ApiException{
    OffsetDateTime time = OffsetDateTime.now()
                                        .withHour(0)
                                        .withMinute(0)
                                        .withSecond(0)
                                        .withNano(0)
                                        .minusDays(1L);
    OffsetDateTime toTime = time.withHour(23)
                                .withMinute(59)
                                .withSecond(59);
    List<String> locations = new ArrayList<>();
    locations.add("westus");
    locations.add("centralus");
    locations.add("eastus");
    List<EmissionsData> data = apiInstance.getEmissionsDataForLocationsByTime(locations, time, toTime);

    System.out.println(data.get(0));
  }

  /**
   * Call /emissions/bylocation .
   * Shows top of result of emission data for westus yesterday.
   */
  public void processEmissionsDataByLocation() throws ApiException{
    OffsetDateTime time = OffsetDateTime.now()
                                        .withHour(0)
                                        .withMinute(0)
                                        .withSecond(0)
                                        .withNano(0)
                                        .minusDays(1L);
    OffsetDateTime toTime = time.withHour(23)
                                .withMinute(59)
                                .withSecond(59);
    List<EmissionsData> data = apiInstance.getEmissionsDataForLocationByTime("westus", time, toTime);

    System.out.println(data.get(0));
  }

  /**
   * Call /emissions/forecasts/current .
   * Shows top of forecast data for westus.
   */
  public void processCurrentForecastData() throws ApiException{
    OffsetDateTime startTime = OffsetDateTime.now()
                                             .withSecond(0)
                                             .withNano(0)
                                             .plusHours(1L);
    OffsetDateTime endTime = startTime.plusHours(1L);
    List<String> locations = new ArrayList<>();
    locations.add("westus");
    List<EmissionsForecastDTO> data = apiInstance.getCurrentForecastData(locations, startTime, endTime, 10);

    System.out.println(data.get(0));
  }

  /**
   * Call /emissions/forecasts/batch .
   * Shows forecast data for westus.
   */
  public void processForecastBatchData() throws ApiException{
    OffsetDateTime requestTime = OffsetDateTime.now().withNano(0);
    OffsetDateTime startTime = requestTime.plusMinutes(10L);
    OffsetDateTime endTime = startTime.plusMinutes(10L);
    EmissionsForecastBatchParametersDTO batchParam = new EmissionsForecastBatchParametersDTO();
    batchParam.setLocation("westus");
    batchParam.setRequestedAt(requestTime);
    batchParam.setDataStartAt(startTime);
    batchParam.setDataEndAt(endTime);
    List<EmissionsForecastBatchParametersDTO> params = new ArrayList<>();
    params.add(batchParam);
    List<EmissionsForecastDTO> data = apiInstance.batchForecastDataAsync(params);

    System.out.println(data.get(0));
  }

  /**
   * Call /emissions/average-carbon-intensity .
   * Shows average data for westus yesterday.
   */
  public void processAverageData() throws ApiException{
    OffsetDateTime startTime = OffsetDateTime.now()
                                             .withHour(0)
                                             .withMinute(0)
                                             .withSecond(0)
                                             .withNano(0)
                                             .minusDays(1L);
    OffsetDateTime endTime = startTime.withHour(23)
                                      .withMinute(59)
                                      .withSecond(59);
    CarbonIntensityDTO intensity = apiInstance.getAverageCarbonIntensity("westus", startTime, endTime);

    System.out.println(intensity);
  }

  /**
   * Call /emissions/average-carbon-intensity/batch .
   * Shows average data for westus yesterday.
   */
  public void processAverageBatchData() throws ApiException{
    OffsetDateTime startTime = OffsetDateTime.now()
                                             .withHour(0)
                                             .withMinute(0)
                                             .withSecond(0)
                                             .withNano(0)
                                             .minusDays(1L);
    OffsetDateTime endTime = startTime.withHour(23)
                                      .withMinute(59)
                                      .withSecond(59);
    CarbonIntensityBatchParametersDTO batchParam = new CarbonIntensityBatchParametersDTO();
    batchParam.setLocation("westus");
    batchParam.setStartTime(startTime);
    batchParam.setEndTime(endTime);
    List<CarbonIntensityBatchParametersDTO> params = new ArrayList<>();
    params.add(batchParam);
    List<CarbonIntensityDTO> data = apiInstance.getAverageCarbonIntensityBatch(params);

    data.forEach(System.out::println);
  }

  public static void main(String[] args) throws ApiException, MalformedURLException{
    WebApiClient client = new WebApiClient(new URL(args[0]));

    System.out.println("--- /emissions/bylocations/best ---");
    client.processBestEmissionsDataByLocations();
    System.out.println();

    System.out.println("--- /emissions/bylocations ---");
    client.processEmissionsDataByLocations();
    System.out.println();

    System.out.println("--- /emissions/bylocation ---");
    client.processEmissionsDataByLocation();
    System.out.println();

    System.out.println("--- /emissions/forecasts/current ---");
    client.processCurrentForecastData();
    System.out.println();

    System.out.println("--- /emissions/forecasts/batch ---");
    client.processForecastBatchData();
    System.out.println();

    System.out.println("--- /emissions/average-carbon-intensity ---");
    client.processAverageData();
    System.out.println();

    System.out.println("--- /emissions/average-carbon-intensity/batch ---");
    client.processAverageBatchData();
    System.out.println();
  }

}
