using System.ClientModel.Primitives;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

public static class NimClientOptionsFactory
{
    public static global::OpenAI.OpenAIClientOptions Create(OpenAIOptions options)
    {
        var clientOptions = new global::OpenAI.OpenAIClientOptions
        {
            NetworkTimeout = TimeSpan.FromSeconds(20),
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0)
        };

        if (!string.IsNullOrWhiteSpace(options.Endpoint))
            clientOptions.Endpoint = new Uri(options.Endpoint);

        return clientOptions;
    }
}
