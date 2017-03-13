# Metrics.NET.Telegraf

[![Build status](https://ci.appveyor.com/api/projects/status/y85oga1l96ataoc2?svg=true)](https://ci.appveyor.com/project/KirillNikonov/metrics-net-telegraf)

Metrics.NET.Telegraf is an extension to the [Metrics.NET](https://github.com/Recognos/Metrics.NET) library that allows to report collected metrics to Telegraf.

You can also write metrics straight to InfluxDB, since the same protocol is used (Influx line protocol).

## Reporting to Telegraf via UDP

First, configure your Telegraf instance to listen for metrics.

```
[[inputs.udp_listener]]
  service_address = ":8092"
  data_format = "influx"
```

Then configure your application to send metrics to Telegraf.

```
Metric.Config
    .WithReporting(config => config
        .WithTelegraf("udp://localhost:8092/", "", TimeSpan.FromSeconds(10))
    );
```

## Reporting to Telegraf via HTTP

Add an HTTP listener to Telegraf configuration. No need to specify data format since it _only_ uses line protocol.

```
[[inputs.http_listener]]
  service_address = ":8186"
```

And the Telegraf URI changes slightly:

```
Metric.Config
    .WithReporting(config => config
        .WithTelegraf("http://localhost:8186/write", "", TimeSpan.FromSeconds(10))
    );
```

## Reporting directly to InfluxDB

Simply specify InfluxDB write URL when configuring metrics reporting.

```
Metric.Config
    .WithReporting(config => config
        .WithTelegraf("http://localhost:8086/write?db=mydb&u=admin&p=admin", "", TimeSpan.FromSeconds(10))
    );
```

## Tags and namespace

You can optionally specify additional tags and/or a namespace (a prefix), that will be attached to all reported metrics.

```
Metric.Config
    .WithReporting(config => config
        .WithTelegraf("udp://localhost:8092/", "app.perf", TimeSpan.FromSeconds(10),
            new string[] { "application:myapp", "environment:prod" })
    );
```