using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gandalan.IDAS.Client.Contracts.Contracts;
using Gandalan.IDAS.Logging;
using Gandalan.IDAS.WebApi.DTO;
using Newtonsoft.Json;

namespace Gandalan.IDAS.WebApi.Client.Settings
{
    public static class WebApiConfigurations
    {
        private static readonly string[] _environments = new[] { "dev", "staging", "produktiv" };
        private static string _settingsPath;
        private static Dictionary<string, IWebApiConfig> _settings;
        private static string _appTokenString;
        private static bool _isInitialized;

        public static async Task Initialize(Guid appToken)
        {
            _settings = new Dictionary<string, IWebApiConfig>(StringComparer.OrdinalIgnoreCase);
            _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gandalan");
            _appTokenString = appToken.ToString().Trim('{', '}');

            await SetupEnvironments(appToken);
            SetupLocalEnvironment(appToken);

            _isInitialized = true;
        }

        public static IWebApiConfig ByName(string name)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("WebApiConfigurations not initialized - call Initialize() first");

            if (_settings.TryGetValue(name, out var byName))
            {
                return byName;
            }

            return null;
        }

        public static List<IWebApiConfig> GetAll()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("WebApiConfigurations not initialized - call Initialize() first");

            return new List<IWebApiConfig>(_settings.Values);
        }

        public static void Save(IWebApiConfig settings)
        {
            if (settings == null)
                return;

            var configPath = Path.Combine(_settingsPath, settings.FriendlyName);
            var configFile = Path.Combine(configPath, "AuthToken_" + _appTokenString + ".json");
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            try
            {
                File.WriteAllText(configFile, JsonConvert.SerializeObject(new SavedAuthToken()
                {
                    UserName = settings.UserName,
                    AuthTokenGuid = settings.AuthToken?.Token ?? Guid.Empty
                }));
                _settings[settings.FriendlyName] = settings;
            }
            catch (Exception)
            {
                // Save went wrong, maybe rights missing. Ignore, no user info for now
            }
        }

        private static void SetupLocalEnvironment(Guid appToken)
        {
            var localEnvPath = Path.Combine(_settingsPath, "Local");
            if (Directory.Exists(localEnvPath))
            {
                var files = Directory.GetFiles(localEnvPath, "*.json");
                foreach (var file in files.Where(fn => !fn.Contains("AuthToken_")))
                {
                    var friendlyName = Path.GetFileNameWithoutExtension(file);
                    try
                    {
                        IWebApiConfig localEnvironment = JsonConvert.DeserializeObject<WebApiSettings>(File.ReadAllText(file));
                        if (localEnvironment != null)
                        {
                            localEnvironment.FriendlyName = friendlyName;
                            localEnvironment.AppToken = appToken;
                            _settings.Add(friendlyName, localEnvironment);
                            InternalLoadSavedAuthToken(friendlyName, localEnvironment);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogConsoleDebug($"Loading local env {friendlyName}: {ex}");
                    }
                }
            }
        }

        private static async Task SetupEnvironments(Guid appToken)
        {
            var hub = new ConnectHub();
            foreach (var env in _environments)
            {
                var response = await hub.GetEndpoints("2.1", env, "win");
                IWebApiConfig environment = null;
                if (response != null)
                {
                    environment = new WebApiSettings
                    {
                        Url = response.IDAS,
                        CMSUrl = response.CMS,
                        DocUrl = response.Docs,
                        FeedbackUrl = response.Feedback,
                        NotifyUrl = response.Notify,
                        HelpCenterUrl = response.HelpCenter,
                        StoreUrl = response.Store,
                        WebhookServiceUrl = response.WebhookService,
                        FriendlyName = env,
                        AppToken = appToken
                    };
                    InternalLoadSavedAuthToken(env, environment);
                }

                if (environment != null)
                    _settings.Add(env, environment);
            }
        }

        private static void InternalLoadSavedAuthToken(string env, IWebApiConfig environment)
        {
            var savedAuthToken = InternalLoadSavedAuthToken(env);
            if (savedAuthToken != null)
            {
                environment.AuthToken = new UserAuthTokenDTO()
                {
                    Token = savedAuthToken.AuthTokenGuid,
                    AppToken = environment.AppToken
                };
                environment.UserName = savedAuthToken.UserName;
            }
        }

        private static SavedAuthToken InternalLoadSavedAuthToken(string env)
        {
            var configFile = Path.Combine(_settingsPath, env, "AuthToken_" + _appTokenString + ".json");
            if (File.Exists(configFile))
            {
                try
                {
                    return JsonConvert.DeserializeObject<SavedAuthToken>(File.ReadAllText(configFile));
                }
                catch (Exception)
                {
                    // damaged file, ignore saved token
                }
            }

            return null;
        }

    }
}
