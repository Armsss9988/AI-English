using System;
using System.ClientModel;
using OpenAI.Chat;

var clientOptions = new global::OpenAI.OpenAIClientOptions();
clientOptions.Endpoint = new Uri("http://localhost:9999/v1/");

var _chatClient = new ChatClient("meta/llama-3.1-70b-instruct", new ApiKeyCredential("test"), clientOptions);

try {
    var response = await _chatClient.CompleteChatAsync("Hello");
    Console.WriteLine(response.Value.Content[0].Text);
} catch (Exception ex) {
    Console.WriteLine(ex);
}
