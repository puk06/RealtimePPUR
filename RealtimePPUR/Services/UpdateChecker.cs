using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace RealtimePPUR.Services;

public static class UpdateChecker
{
    private static readonly HttpClient _httpClient = new();
    private static readonly string UpdateCheckURL = "https://update.pukosrv.net/simplecheck/realtimeppur";
    public static async Task<string?> CheckUpdate()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(UpdateCheckURL);
            if (response == null) return null;
            
            response = response.Trim();

            var current = RealtimePPCalculator.CURRENT_VERSION;
            if (response == current) return string.Empty;

            return response;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return null;
        }
    }
}
