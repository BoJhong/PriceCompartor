﻿using Microsoft.Extensions.Options;
using PriceCompartor.Models.GeminiModels;

namespace PriceCompartor.Models
{
    public class GeminiTextRequest : AIHttpClient
    {
        public List<Content> contents { get; set; } = new List<Content>();

        public async Task<GeminiTextResponse> SendMsg(string msg)
        {
            GeminiTextRequest geminiTextRequest = new();
            contents.Add(
                new Content
                {
                    role = "user",
                    parts = [new Part  {text = msg}]
                }
            );

            geminiTextRequest.contents = contents;

            GeminiTextResponse geminiTextResponse = await PostAsync<GeminiTextRequest, GeminiTextResponse>(geminiTextRequest, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro-latest:generateContent?key={ApiKeys[ApiKeyCycle]}");

            ApiKeyCycle = ++ApiKeyCycle % ApiKeys.Length;

            if (geminiTextResponse != null && geminiTextResponse?.candidates?.Length > 0)
                contents.Add(geminiTextResponse.candidates[0].content);

            return geminiTextResponse!;
        }
    }

    public partial class Content
    {
        public required Part[] parts { get; set; }
}

    public class Part
    {
        public required string text { get; set; }
    }
}