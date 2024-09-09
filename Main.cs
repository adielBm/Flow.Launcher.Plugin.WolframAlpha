using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.SharedCommands;

namespace Flow.Launcher.Plugin.WolframAlpha
{
    public class Main : IAsyncPlugin, IContextMenu, ISettingProvider
    {
        private Settings settings;
        private const string WolframAlphaApiUrl = "http://api.wolframalpha.com/v1/result";
        private string AppId => settings.appid;

        public Task InitAsync(PluginInitContext context)
        {
            settings = context.API.LoadSettingJsonStorage<Settings>();
            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            var results = new List<Result>();
            var querySearch = query.Search;

            if (string.IsNullOrWhiteSpace(querySearch))
            {
                return new List<Result>
                {
                    CreateResult("Empty query", "Please enter a question to search on WolframAlpha.")
                };
            }

            // check there is a question mark or exclamation mark at the end of the query
            if (!querySearch.EndsWith("?") && !querySearch.EndsWith("!"))
            {
                return new List<Result>
                {
                    CreateResult(" ", "End your question with '?' or '!'")
                };
            }
            else
            {
                // remove the question mark or exclamation mark at the end of the query
                querySearch = querySearch[..^1];
            }

            try
            {
                string answer = await GetWolframAlphaAnswer(querySearch);

                if (!string.IsNullOrEmpty(answer))
                {
                    if (answer.Length > 40)
                    {
                        results.Add(CreateResult("Answer:", answer));
                    }
                    else
                    {
                        results.Add(CreateResult(answer, ""));
                    }
                }
                else
                {
                    results.Add(CreateResult("No answer found", "Try asking a different question."));
                }
            }
            catch (Exception)
            {
                results.Add(CreateResult("Could not retrieve an answer", " "));
            }

            return results;
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            // No context menus for WolframAlpha results
            return new List<Result>();
        }

        private async Task<string> GetWolframAlphaAnswer(string query)
        {
            using var httpClient = new HttpClient();
            var requestUrl = $"{WolframAlphaApiUrl}?appid={AppId}&i={Uri.EscapeDataString(query)}";

            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            throw new Exception($"{response.StatusCode}, request: {requestUrl}");
        }

        private static Result CreateResult(string title, string subTitle, string iconPath = "Images/icon.png")
        {
            return new Result
            {
                Title = title,
                SubTitle = subTitle,
                IcoPath = iconPath
            };
        }

        public void Dispose() { }

        public Control CreateSettingPanel() => new SettingsControl(settings);
    }
}

