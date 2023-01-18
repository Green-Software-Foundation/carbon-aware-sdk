using CarbonAware.Model;

namespace CarbonAware.CLI.Model
{
    public class EmissionsDataDTO
    {
        ///<example> eastus </example>
        public string? Location { get; set; }
        ///<example> 2022-09-19T17:30:00Z </example>   
        public DateTimeOffset? Time { get; set; }
        ///<example> 140.5 </example>
        public double Rating { get; set; }
        ///<example>1.12:24:02 </example>
        public TimeSpan? Duration { get; set; }

        public static explicit operator EmissionsDataDTO(EmissionsData emissions)
        {
            EmissionsDataDTO emissionsDTO = new EmissionsDataDTO();
            emissionsDTO.Location = emissions.Location;
            emissionsDTO.Time = emissions.Time;
            emissionsDTO.Duration = emissions.Duration;
            emissionsDTO.Rating = emissions.Rating;
            return emissionsDTO;
        }
    }
}
