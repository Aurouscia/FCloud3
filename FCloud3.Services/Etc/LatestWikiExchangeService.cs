using FCloud3.Entities.Identities;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using Newtonsoft.Json;

namespace FCloud3.Services.Etc
{
    public class LatestWikiExchangeService
    {
        public const string pullRoute = "/api/LatestWikiExchange/Pull";
        public const string pushRoute = "/api/LatestWikiExchange/Push";
        public const string configSectionName = "LatestWikiExchange";
        public const int itemsMaxCount = 4;
        public const int pushCooldownSecs = 60;
        private readonly WikiItemRepo _wikiItemRepo;
        private readonly UserRepo _userRepo;
        private readonly MaterialRepo _materialRepo;
        private readonly IStorage _storage;
        private readonly IConfiguration _config;
        private readonly ILogger<LatestWikiExchangeService> _logger;
        public LatestWikiExchangeService(
            WikiItemRepo wikiItemRepo,
            UserRepo userRepo,
            MaterialRepo materialRepo,
            IStorage storage,
            IConfiguration config,
            ILogger<LatestWikiExchangeService> logger)
        {
            _wikiItemRepo = wikiItemRepo;
            _userRepo = userRepo;
            _materialRepo = materialRepo;
            _storage = storage;
            _config = config;
            _logger = logger;
        }
        
        private static ExchangeConfig? _exConfig;
        private ExchangeConfig ExConfig
        {
            get
            {
                if (_exConfig is null)
                {
                    _exConfig = new();
                    _config.GetSection(configSectionName).Bind(_exConfig);
                }
                return _exConfig;
            }
        }
        private static List<ExchangeItem> Items { get; set; } = [];
        private static readonly object itemsListLockObj = new();
        public static bool Inited { get; set; }
        public static DateTime MyLastPush { get; private set; }
        public bool Enabled => ExConfig.Enabled;

        /// <summary>
        /// 本实例显示词条时，在此处获取其他实例的词条
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExchangeItem> GetItems()
        {
            if (Inited)
                return Items;
            else
            {
                Pull();
                Inited = true;
                return Items;
            }
        }

        /// <summary>
        /// 其他实例往本实例推送更新
        /// </summary>
        /// <param name="data"></param>
        public void BePushed(ExchangePushRequest data)
        {
            if (data.PusherCode is null || data.PusherDomain is null) 
            {
                _logger.LogDebug(bePushedErr + "推送参数异常");
                return;
            }
            var target = ExConfig.Targets?
                .FirstOrDefault(x => x.Domain == data.PusherDomain && x.Code == data.PusherCode);
            if (target is { })
            {
                if (CanBeUpstream(target))
                {
                    lock (itemsListLockObj)
                    {
                        Items.RemoveAll(x => x.Url is null || x.Url.StartsWith(data.PusherDomain));
                        if (data.Items is { })
                        {
                            var validItems = data.Items.Where(x => 
                                x.Url is { } && x.Url.StartsWith(data.PusherDomain));
                            Items.AddRange(validItems);
                        }
                    }
                    _logger.LogDebug(bePushedSuccess + "{domain}", data.PusherDomain);
                }
                else
                    _logger.LogDebug(bePushedErr + "{domain}" + "不允许的推送", data.PusherDomain);
            }
            else
                _logger.LogDebug(bePushedErr + "{domain}" + "未授权的推送", data.PusherDomain);
            TidyItems();
            LogStatus();
        }

        /// <summary>
        /// 其他实例从本实例拉取更新
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<ExchangeItem>? BePulled(ExchangePullRequest data)
        {
            if (data.PullerCode is null || data.PullerDomain is null)
                _logger.LogDebug(bePulledErr + "拉取参数异常");
            var target = ExConfig.Targets?
                .FirstOrDefault(x => x.Domain == data.PullerDomain && x.Code == data.PullerCode);
            if (target is { })
            {
                if (CanBeDownstream(target))
                {
                    var res = MyLatestWikis();
                    _logger.LogDebug(bePulledSuccess + "{domain}", data.PullerDomain);
                    return res;
                }
                else
                    _logger.LogDebug(bePulledErr + "{domain}" + "不允许的推送", data.PullerDomain);
            }
            else
                _logger.LogDebug(bePulledErr + "{domain}" + "未授权的推送", data.PullerDomain);
            return null;
        }
        
        
        /// <summary>
        /// 本实例往其他实例推送更新
        /// </summary>
        public void Push()
        {
            DateTime now = DateTime.Now;
            //冷却时间
            if ((now - MyLastPush).TotalSeconds < pushCooldownSecs)
                return;
            MyLastPush = now;
            var needPush = MyLatestWikis();
            if (needPush.Count == 0)
                return;
            
            if (ExConfig.MyCode is null || ExConfig.Targets is null)
            {
                _logger.LogDebug(pushErr + "：配置异常，未能运行");
                return;
            }
            RestClient rc = new();
            var domains = ExConfig.Targets
                .Where(x => CanBeDownstream(x))
                .Select(x => x.Domain);
            foreach (var domain in domains)
            {
                if (domain is { })
                {
                    RestRequest rr = new($"{domain}{pushRoute}");
                    var reqObj = new ExchangePushRequest()
                    {
                        PusherCode = ExConfig.MyCode,
                        PusherDomain = ExConfig.MyDomain,
                        Items = needPush
                    };
                    rr.AddJsonBody(reqObj);
                    try
                    {
                        var resp = rc.Post(rr);
                        if (resp.IsSuccessful && resp.Content is { })
                        {
                            _logger.LogDebug(pushSuccess + "{domain}", domain);
                        }
                        else
                            _logger.LogDebug(pushErr + "{respCode}", resp.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(pushErr + "{domain} {errmsg}", domain, ex.Message);
                    }
                }
            }
        }
        
        /// <summary>
        /// 本实例从其他实例拉取更新（仅执行初次，后面只听推送）
        /// </summary>
        private void Pull()
        {
            if (ExConfig.MyCode is null || ExConfig.Targets is null)
            {
                _logger.LogDebug(pullErr + "：配置异常，未能运行");
                return;
            }
            RestClient rc = new();
            var domains = ExConfig.Targets
                .Where(x => CanBeUpstream(x))
                .Select(x => x.Domain);
            lock (itemsListLockObj)
            {
                foreach (var domain in domains)
                {
                    if (domain is { })
                    {
                        List<ExchangeItem>? items = null;
                        RestRequest rr = new($"{domain}{pullRoute}");
                        var reqObj = new ExchangePullRequest()
                        {
                            PullerCode = ExConfig.MyCode,
                            PullerDomain = ExConfig.MyDomain
                        };
                        rr.AddJsonBody(reqObj);
                        try
                        {
                            var resp = rc.Post(rr);
                            if (resp.IsSuccessful && resp.Content is { })
                            {
                                items = JsonConvert.DeserializeObject<List<ExchangeItem>>(resp.Content);
                                _logger.LogDebug(pullSuccess + "{domain}", domain);
                            }
                            else
                                _logger.LogDebug(pullErr + "{respCode}", resp.StatusCode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(pullErr + "{domain} {errmsg}", domain, ex.Message);
                        }
                        if (items is { })
                            Items.AddRange(items);
                    }
                }
            }
            TidyItems();
            LogStatus();
        }
        
        private List<ExchangeItem> MyLatestWikis(DateTime? after = null)
        {
            var latestQ = _wikiItemRepo.ExistingAndNotSealedAndEdited;
            if (after is { } time)
            {
                latestQ = latestQ.Where(x => x.LastActive > time);
            }
            //TODO: 此处可缓存
            var latests = latestQ
                .OrderByDescending(x => x.LastActive)
                .Take(itemsMaxCount)
                .Select(x => new { x.Title, x.UrlPathName, x.OwnerUserId, x.LastActive })
                .ToList();
            var ownerIds = latests.ConvertAll(x => x.OwnerUserId);
            var avts = (
                from m in _materialRepo.Existing
                from u in _userRepo.Existing
                where u.AvatarMaterialId == m.Id
                where ownerIds.Contains(u.Id)
                select new { UserId = u.Id, Avt = m.StorePathName }).ToList();
            List<ExchangeItem> resItems = new(itemsMaxCount);
            latests.ForEach(w =>
            {
                if (string.IsNullOrWhiteSpace(w.Title) || string.IsNullOrWhiteSpace(w.UrlPathName))
                    return;
                var avt = avts.Find(a => a.UserId == w.OwnerUserId)?.Avt;
                var avtUrl = avt is { } ? _storage.FullUrl(avt) : User.defaultAvatar;
                var item = new ExchangeItem() {
                    Avt = avtUrl,
                    Text = w.Title,
                    Time = w.LastActive,
                    Url = WikiUrl(w.UrlPathName)
                };
                resItems.Add(item);
            });
            return resItems;
        }
        
        private void TidyItems()
        {
            lock (itemsListLockObj)
            {
                Items.Sort((x, y) => DateTime.Compare(y.Time, x.Time));
                if (Items.Count > itemsMaxCount)
                {
                    int exceed = Items.Count - itemsMaxCount;
                    Items.RemoveRange(itemsMaxCount, exceed);
                }
            }
        }

        private void LogStatus()
        {
            var titles = 
                string.Join('|', Items.Select(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Text))
                        return "<>";
                    if (x.Text.Length == 1)
                        return x.Text.Substring(0, 1);
                    else
                        return x.Text.Substring(0, 2);
                }));
            _logger.LogDebug("词条交换：当前{count}个外来物{titles}：", Items.Count, titles);
        }

        private const string pullErr = "词条交换：拉取失败：";
        private const string pullSuccess = "词条交换：拉取成功：";
        private const string pushErr = "词条交换：推送失败：";
        private const string pushSuccess = "词条交换：推送成功：";
        private const string bePulledErr = "词条交换：被拉取失败：";
        private const string bePulledSuccess = "词条交换：被拉取成功：";
        private const string bePushedErr = "词条交换：被推送失败：";
        private const string bePushedSuccess = "词条交换：被推送成功：";

        private bool CanBeUpstream(ExchangeTarget target)
            => (target.Role & ExchangeTargetRole.Upstream) == ExchangeTargetRole.Upstream;
        private bool CanBeDownstream(ExchangeTarget target)
            => (target.Role & ExchangeTargetRole.Downstream) == ExchangeTargetRole.Downstream;
        private string WikiUrl(string urlPathName) 
            => ExConfig.MyDomain + "/#/w/" + urlPathName;
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

    public class ExchangePushRequest
    {
        public string? PusherDomain { get; set; }
        public string? PusherCode { get; set; }
        public List<ExchangeItem>? Items { get; set; }
    }

    public class ExchangePullRequest
    {
        public string? PullerDomain { get; set; }
        public string? PullerCode { get; set; }
    }
}