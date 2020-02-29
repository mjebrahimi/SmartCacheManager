# SmartCacheManager
**SmartCacheManager** is a smart caching module to cache objects with resilient and variable expiration timing that is useful for caching result of web services.

## Features
- Cache data in **Redis** (by default) using [EasyCaching](https://github.com/dotnetcore/EasyCaching) 
    - Supports caching providers (Both local caching and distributed caching) such as **In-Memory**, **Redis**, **Memcached**, **Disk**, **Sqlite** and ...
- Serialize data using extremely fast **MessagePack** serializer (by default)
    - Supports caching serializers such as **BinaryFormatter**, **MessagePack**, **Json**, **Protobuf** and ...
- Settings for cache expiration time by **minimum** and **maximum**
- **Increasing** cache expiration time based on date of search
- Calculates the **RPM (request per minute)** of a search
- **Decreasing** cache expiration time based on RPM of search
- Ability to **limit** searches based on maximum limit count in custom duration
- Fully **settingable per Supplier Type**
- Fully **async/await support** with cancellation token
- **Thread-safe** capability using async/await-friendly lock (via SemaphoreSlim)
- Logs **Sensitive Data** using [Serilog](https://github.com/serilog/serilog)
    - Logs any **errors** occures in services
    - Logs **all steps** of the search and cache process
    - Logs **incomming requests** to this module
    - Logs **outgoign request** from this module to web services
    - Logs **execution time** of service methods using [MethodTimer.Fody](https://github.com/Fody/MethodTimer)
    - Logs to **SqlServer** (by default) and supports **Console**, **Debug**, **File**, **EventViwer**, **Seq**, **ElasticSearch** and ...
- Flexible, lightweight and **highly customizable**


## Get Started

### 1. Install Package

```
PM> Install-Package SmartCacheManager
```

### 2. Implement your own service

For example: simple implementation for FlightCacheManager can be like this.

```csharp
public class FlightCacheManager : SmartCacheManager<FlightSearchModel>, IFlightCacheManager
{
    public FlightCacheManager(ICacheManager cacheManager, ILoggerFactory loggerFactory, ISystemClock systemClock,
        ICacheSettingService<CacheSetting> cacheSettingService, ISearchHistoryService<CacheSetting, LimitSetting> searchHistoryService)
        : base(cacheManager, loggerFactory, systemClock, cacheSettingService, searchHistoryService)
    {
    }
    protected override string GenerateSearchHistoryKey(FlightSearchModel searchModel)
    {
        return GenerateSearchResultKey(searchModel);
    }
    protected override string GenerateSearchResultKey(FlightSearchModel searchModel)
    {
        return $"{searchModel.Origin}-{searchModel.Destination}-{searchModel.SearchDate.ToString("yyyy-MM-dd")}";
    }
    protected override DateTime GetSearchDate(FlightSearchModel searchModel)
    {
        return searchModel.SearchDate;
    }
}
public class FlightSearchModel
{
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime SearchDate { get; set; }
}
```

### 3. Add Services

Add services and configure it in `ConfigureServices` method of `Startup.cs` using `services.AddSmartCacheManager()` and register your implementation with scoped or transient lifetime.

```csharp
public static void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddSmartCacheManager(opt => opt.UseSqlServer("Data Source=.;Initial Catalog=CacheManageDb;Integrated Security=true");
    services.AddScoped<FlightCacheManager>();
    //...
}
```

### 4. Initialize it

Initial it in `Configure` method of `Startup.cs` using `app.InitialSmartCacheManager()`.

```csharp
public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.InitialSmartCacheManager();
    //...
```

### 5. Use it

Inject your own implementation  and use it using `SearchFromCacheAsync` method.

```csharp
var supplier = FlightSupplierts.Amadeus; //supplier can be any type, string, enum or ...
var searchModel = new FlightSearchModel
{
    Origin = "MUC",
    Destination = "HAM",
    SearchDate = DateTime.Now.AddDays(10)
};
var result = await _flightCacheManager.SearchFromCacheAsync(searchModel, supplier, () =>
{
    return _amadeusFlightService.SearchAsync(searchModel);
});
```

## Contributing

Create an [issue]() if you find a BUG or have a Suggestion or Question. If you want to develop this project :

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request

## Give a Star! ⭐️

If you find this repository useful, please give it a star. Thanks!

## License

AutoMapper is Copyright © 2020 [Mohammd Javad Ebrahimi](https://github.com/mjebrahimi) under the MIT license.
