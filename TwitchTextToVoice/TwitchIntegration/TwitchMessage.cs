namespace TwitchTextToVoice.TwitchIntegration
{
    public class TwitchMessage
    {
        public TwitchMessage()
        {
            Tags = string.Empty;
            Source = string.Empty;
            Command = string.Empty;
            Parameters = string.Empty;
        }
        public string Tags { get; set; }
        public string Source { get; set; }
        public string Command { get; set; }
        public string Parameters { get; set; }
    }
}
