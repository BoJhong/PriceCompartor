namespace PriceCompartor.Models
{
    public class GeminiTextResponse
    {
        public Candidate[]? candidates { get; set; }
        public Promptfeedback? promptFeedback { get; set; }
    }

    public class Promptfeedback
    {
        public required Safetyrating[] safetyRatings { get; set; }
    }

    public class Safetyrating
    {
        public required string category { get; set; }
        public required string probability { get; set; }
    }

    public class Candidate
    {
        public required Content content { get; set; }
        public required string finishReason { get; set; }
        public int index { get; set; }
        public required Safetyrating1[] safetyRatings { get; set; }
    }

    public partial class Content
    {
        public required string role { get; set; }
    }

    public class Safetyrating1
    {
        public required string category { get; set; }
        public required string probability { get; set; }
    }
}
