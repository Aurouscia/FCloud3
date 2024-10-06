using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using Newtonsoft.Json;

namespace FCloud3.Services.Etc
{
    public class LatestWikiExchangeService
    {
        public const string route = "/api/LatestWikiExchange";
        public const int itemsMaxCount = 8;
        private readonly ExchangeConfig _config;
        private readonly ILogger<LatestWikiExchangeService> _logger;
        public LatestWikiExchangeService(IConfiguration config, ILogger<LatestWikiExchangeService> logger)
        {
            _config = new();
            _logger = logger;
            config.GetSection("LatestWikiExchange").Bind(_config);
        }

        public List<ExchangeItem> Items { get; set; } = [];
        public bool Inited { get; set; }
        public DateTime MyLatestWikiUpdate { get; set; }
        public bool Enabled => _config.Enabled;

        public List<ExchangeItem> GetItems()
        {
            if (Inited)
                return Items;
            if (_config.MyCode is null || _config.Targets is null)
            {
                _logger.LogDebug("词条交换：配置异常，未能运行");
                return [];
            }
            RestClient rc = new();
            var domains = _config.Targets
                .Where(x => CanBeUpstream(x))
                .Select(x => x.Domain);
            foreach(var domain in domains)
            {
                if (domain is { })
                {
                    List<ExchangeItem>? items = null;
                    RestRequest rr = new($"{domain}{route}");
                    rr.AddQueryParameter("code", _config.MyCode);
                    try
                    {
                        var resp = rc.Get(rr);
                        if (resp.IsSuccessful && resp.Content is { })
                        {
                            items = JsonConvert.DeserializeObject<List<ExchangeItem>>(resp.Content);
                            _logger.LogDebug(activeSuccess + "{domain}", domain);
                        }
                        else
                            _logger.LogDebug(activeErr + "{respCode}", resp.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(activeErr + "{domain} {errmsg}", domain, ex.Message);
                    }
                    if(items is { })
                        Items.AddRange(items);
                }
            }
            TidyItems();
            Inited = true;
            return Items;
        }

        public void Push(ExchangePushDto data)
        {
            if (data.PusherCode is null || data.PusherDomain is null)
                _logger.LogDebug(passiveErr + "推送参数异常");
            var target = _config.Targets?
                .FirstOrDefault(x => x.Domain == data.PusherDomain && x.Code == data.PusherCode);
            if (target is { })
            {
                if (CanBeUpstream(target))
                {
                    Items.AddRange(data.Items ?? []);
                    _logger.LogDebug(passiveSuccess + "{domain}", data.PusherDomain);
                }
                else
                    _logger.LogDebug(passiveErr + "不允许的推送");
            }
            else
                _logger.LogDebug(passiveErr + "未授权的推送");
            TidyItems();
        }

        private void TidyItems()
        {
            Items.Sort((x, y) => DateTime.Compare(y.Time, x.Time));
            if (Items.Count > itemsMaxCount)
            {
                int exceed = Items.Count - itemsMaxCount;
                Items.RemoveRange(itemsMaxCount, exceed);
            }
        }

        private const string activeErr = "词条交换：主动同步失败：";
        private const string activeSuccess = "词条交换：主动同步成功：";
        private const string passiveErr = "词条交换：被动同步失败：";
        private const string passiveSuccess = "词条交换：被动同步成功：";

        private bool CanBeUpstream(ExchangeTarget target)
            => (target.Role & ExchangeTargetRole.Upstream) == ExchangeTargetRole.Upstream;
        private bool CanBeDownstream(ExchangeTarget target)
            => (target.Role & ExchangeTargetRole.Downstream) == ExchangeTargetRole.Downstream;
    }

    public class ExchangeConfig
    {
        public string? MyDomain { get; set; }
        public string? MyCode { get; set; }
        public List<ExchangeTarget>? Targets { get; set; }
        public bool Enabled { get; set; }
    }
    public class ExchangeTarget
    {
        public string? Domain { get; set; }
        public string? Code { get; set; }
        public ExchangeTargetRole Role { get; set; }
    }

    [Flags]
    public enum ExchangeTargetRole : byte
    {
        Disabled = 0,
        Upstream = 1,
        Downstream = 2,
        Twoway = 3
    }
    public class ExchangeItem
    {
        public string? Avt { get; set; }
        public string? Text { get; set; }
        public string? Url { get; set; }
        public DateTime Time { get; set; }
    }

    public class ExchangePushDto
    {
        public string? PusherDomain { get; set; }
        public string? PusherCode { get; set; }
        public List<ExchangeItem>? Items { get; set; }
    }
}