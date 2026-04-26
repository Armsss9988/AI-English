namespace EnglishCoach.Infrastructure.AI.OpenAI;

public class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty; // e.g. https://integrate.api.nvidia.com/v1
    public string ChatModel { get; set; } = "stepfun-ai/step-1-8k"; // Default to NIM's stepfun
    public string AudioModel { get; set; } = "whisper-1";
}
