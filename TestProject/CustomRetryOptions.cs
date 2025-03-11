using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace TestProject;

public class CustomRetryStrategyOptions : HttpRetryStrategyOptions
{
    public CustomRetryStrategyOptions()
    {
        ShouldHandle = (args => new ValueTask<bool>(IsCustomTransient(args.Outcome)));
        BackoffType = DelayBackoffType.Exponential;
        ShouldRetryAfterHeader = true;
        MaxRetryAttempts = 3;
        UseJitter = true;
    }
    
    public static bool IsCustomTransient(Outcome<HttpResponseMessage> outcome)
    {
        Outcome<HttpResponseMessage> outcome1 = outcome;
        HttpResponseMessage result = outcome1.Result;
        Console.WriteLine("ResponseStatus: " + outcome1.Result?.StatusCode);
        Console.WriteLine("Exception: " + outcome1.Exception?.Message);
        Console.WriteLine("Headers: " + outcome1.Result?.Headers);
        if (outcome1.Exception != null || !(result?.IsSuccessStatusCode ?? false))
            return true;
        return false;
    }
}