namespace SatisfactoryModdingHelper.Models
{
    public class AppConfig
    {
        public string ConfigurationsFolder { get; set; }

        public string AppPropertiesFileName { get; set; }

        public string PrivacyStatement { get; set; }

        public string SelectedPlugin {  get; set; }

        public bool AIO_GenerateVS {  get; set; }
        public bool AIO_BuildDev {  get; set; }
        public bool AIO_BuildShipping {  get; set; }
        public bool AIO_Alpakit {  get; set; }
        public bool AIO_Launch {  get; set; }
    }
}
