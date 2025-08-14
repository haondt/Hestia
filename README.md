# Hestia

A Recipe book that includes costs and macronutrients.

![](./docs/image.png)

## Deployment

The easiest way to run Hestia is with docker compose.

```yml
services:
  hestia:
    image: registry.gitlab.com/haondt/cicd/registry/hestia:latest
    ports:
      - 8080:8080
    volumes:
      - hestia-data:/data
volumes:
  hestia-data:
```

## Features & Configuration

Hestia is configured using .NET appsettings, meaning it can be configured through environment variables or by mounting an `appsettings.Production.json` in the `/app` directory.

If using environment variables, use double underscores to flatten the json structure. For example, this configuration:

```json
{
  "Logging": [
    {
      "Name": "ToEmail",
      "Args": {
        "ToAddress": "SRE@example.com"
      }
    }
  ]
}
```

becomes

```shell
Logging__0__Name=ToEmail
Logging__0__Args__ToAddress=SRE@example.com
```

See the [ASP.NET Core docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-9.0#naming-of-environment-variables) for more information.

### Persistence Settings

By default Hestia will save data in an sqlite database at `/data/hestia/hestia.db` and any internal files at `/data/hestia`.

```json
{
  "PersistenceSettings": {
    "Driver": "Sqlite", // can be one of Sqlite, Memory
    "DropDatabaseOnStartup": false,
    "Sqlite": {
      // only needed if using Sqlite driver
      "FilePath": "hestia.db" // relative to FileDataPath
    },
    "FileDataPath": "/data/hestia"
  }
}
```

### Scanner Settings

This is a feature that enables Hestia to scan and process pictures of packaging and nutrition labels to extract information. It requires configuration for an ocr tool to extract the text and an llm to parse the data. If you are using a Google Cloud service, you might also want to set `GOOGLE_APPLICATION_CREDENTIALS`. Hestia is expecting to use [Application Default Credentials](https://cloud.google.com/docs/authentication/application-default-credentials).

```json
{
  "ScannerSettings": {
    "Enabled": false, // whether the feature is enabled
    "LlmProvider": "OpenRouter", // can be one of OpenRouter
    "OcrProvider": "CloudVision", // can be one of CloudVision, DocumentAI
    "OpenRouter": {
      // only needed if using LlmProvider = OpenRouter
      "ApiKey": "your openrouter api key",
      "Model": "google/gemini-2.5-flash-lite" // recommended to use a fast model
    },
    "DocumentAI": {
      // only needed if using OcrProvider = DocumentAI
      "ProjectId": "project-1234",
      "ProcessorLocationId": "us",
      "ProcessorId": "abcd1234"
    }
  }
}
```

## Development

### Scripts

- make db migrations

```sh
cd Hestia
.\makemigration MyMigration
```

- install dcdn deps

```sh
dcdn install
```
