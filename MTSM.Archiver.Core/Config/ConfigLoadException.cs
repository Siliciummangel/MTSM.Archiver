namespace MTSM.Archiver.Core.Config
{
    public sealed class ConfigLoadException : Exception
    {
        public ConfigLoadException(string message) : base(message) { }

        public ConfigLoadException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
