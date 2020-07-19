[![NuGet](https://img.shields.io/nuget/v/SmartCacheManager.svg)](https://www.nuget.org/packages/SmartCacheManager)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/mjebrahimi/SmartCacheManager/workflows/.NET%20Core/badge.svg)](https://github.com/mjebrahimi/SmartCacheManager)

# SmartCacheManager
**SmartCacheManager** is a response caching module which cache objects with resilient and variable expiration time that is useful for caching the result of web services and other calculations.

## Features
- • Caching objects using [EasyCaching](https://github.com/dotnetcore/EasyCaching) 
    - Supports various caching providers such as **In-Memory**, **Redis**, **Memcached**, **Disk**, **Sqlite**, ...
- • Serializing data
    - Supports various caching serializers such as **BinaryFormatter**, **MessagePack**, **Json**, **Protobuf**, ...
- • Settings for cache expiration time by specifying the **minimum** and **maximum** duration
- • Increasing and decreasing cache expiration time automatically based on the date of search and RPM
- • Calculates the **RPM (request per minute)** of a search
- • Ability to **limit** searches based on maximum limit count in a specified time range
- • Ability to configuring cache-key dynamically based on your needs
- • **Async** support with cancellation tokens
- • **Thread-safety** support
- • Flexible, lightweight and **highly customizable**
- • Logs **Sensitive Data** using [Serilog](https://github.com/serilog/serilog)
    - Logs all errors occures in services
    - Logs all steps of the search and caching process
    - Logs all incomming requests to this module
    - Logs all outgoing request from this module to external services
    - Logs the execution time of service methods using [MethodTimer.Fody](https://github.com/Fody/MethodTimer)
    - Logs to the **Sql-Server** (by default) and supports **Console**, **Debug**, **File**, **EventViwer**, **Seq**, **ElasticSearch** and ...


## Getting Started

### 1. Install Package

> For the .NET Core 2.2 use [v1.0.0](https://www.nuget.org/packages/SmartCacheManager/1.0.0) and for the .NET Core 3.1 use [v2.0.0](https://www.nuget.org/packages/SmartCacheManager/2.0.0) of SmartCacheManager:
```
PM> Install-Package SmartCacheManager
```

### 2. Implement your own cache-manager service

For example: A simple implementation of a FlightCacheManager could be like this:

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

### 3. Register services

Register SmartCacheManager in your DI container by calling `services.AddSmartCacheManager()` and also, register your own cache-manager implementation:

```csharp
public static void ConfigureServices(IServiceCollection services)
{
    services.AddSmartCacheManager(opt => opt.UseSqlServer("Data Source=.;Initial Catalog=CacheManageDb;Integrated Security=true");
    services.AddScoped<FlightCacheManager>();
    //...
}
```

### 4. Initialize it

Initialize SmartCacheManager using `app.InitialSmartCacheManager()` in the `Configure` method of `Startup.cs`:

```csharp
public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.InitialSmartCacheManager();
    //...
```

### 5. Use it

Inject your own implementation of cache-manager to your services and use it by calling `SearchFromCacheAsync()` method:

```csharp
var supplier = FlightSupplierts.Amadeus; // Supplier could be any type: string, enum or ...
var searchModel = new FlightSearchModel
{
    Origin = "AMS",
    Destination = "HAM",
    SearchDate = DateTime.Now.AddDays(10)
};

var result = await _flightCacheManager.SearchFromCacheAsync(searchModel, supplier, () =>
{
    return _amadeusFlightService.SearchAsync(searchModel);
});
```

## Contributing

Create an [issue]() if you find a bug or have a suggestion or question. If you want to develop this project:

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request

## Give a Star! ⭐️

If you find this repository useful, please give it a star. Thanks!

## License/Copyright 

SmartCacheManager is Copyright © 2020 [Mohammd Javad Ebrahimi](https://github.com/mjebrahimi) under the [GNU GPLv3 License](/LICENSE).
