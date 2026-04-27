using EnglishCoach.Infrastructure.AI.OpenAI;
using System.ClientModel.Primitives;

namespace EnglishCoach.IntegrationTests.AI.OpenAI;

public sealed class NimClientOptionsFactoryTests
{
    [Fact]
    public void Create_Configures_Nim_Client_For_Fast_Failure()
    {
        var options = NimClientOptionsFactory.Create(new OpenAIOptions
        {
            Endpoint = "https://integrate.api.nvidia.com/v1"
        });

        Assert.Equal(new Uri("https://integrate.api.nvidia.com/v1"), options.Endpoint);
        Assert.Equal(TimeSpan.FromSeconds(20), options.NetworkTimeout);
        Assert.NotSame(ClientRetryPolicy.Default, options.RetryPolicy);
    }
}
