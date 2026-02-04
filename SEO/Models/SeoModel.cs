namespace MyProject.SEO.Models
{
    public class SeoModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required IEnumerable<string> Tags { get; set; }
        public required string SocialMediaShareImage { get; set; }
        public required string Url { get; set; }
    }
}
