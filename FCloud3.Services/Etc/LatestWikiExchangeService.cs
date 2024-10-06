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
        public HashSet<string> InitedDomains { get; set; } = [];
        public DateTime MyLatestWikiUpdate { get; set; }
        public bool Enabled => _config.Enabled;

        public List<ExchangeItem> Get()
        {
            if (_config.MyCode is null || _config.Targets is null)
            {
                _logger.LogDebug("词条交换：配置异常，未运行");
                return [];
            }
            RestClient rc = new();
            var domains = _config.Targets.Select(x => x.Domain?.Trim());
            foreach(var domain in domains)
            {
                if (domain is { } && !InitedDomains.Contains(domain))
                {
                    List<ExchangeItem>? items = null;
                    RestRequest rr = new($"{domain}{route}");
                    rr.AddQueryParameter("code", _config.MyCode);
                    try
                    {
                        var resp = rc.Get(rr);
                        if (resp.IsSuccessful && resp.Content is { })
                            items = JsonConvert.DeserializeObject<List<ExchangeItem>>(resp.Content);
                        _logger.LogDebug("词条交换：主动同步成功：{domain}", domain);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("词条交换：主动同步失败：{domain} {errmsg}", domain, ex.Message);
                    }
                    if(items is { })
                        Items.AddRange(items);
                    InitedDomains.Add(domain);
                }
            }
            TidyItems();
            return Items;
        }

        public void Set(ExchangePushDto data)
        {
            if (data.PusherCode is null || data.PusherDomain is null)
                _logger.LogDebug("词条交换：被动同步失败：接收参数异常");
            var target = _config.Targets?
                .FirstOrDefault(x => x.Domain == data.PusherDomain && x.Code == data.PusherCode);
            if (target is { } && data.Items is { })
                Items.AddRange(data.Items);
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