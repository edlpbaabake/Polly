# Resilience pipeline registry

> [!NOTE]
> This documentation supports the upcoming Polly v8 release.

The `ResiliencePipelineRegistry<TKey>` is designed to create and cache resilience pipeline instances. The registry also implements the `ResiliencePipelineProvider<TKey>`, allowing read-only access to pipelines.

The registry offers these features:

- Thread-safe retrieval and dynamic creation for both generic and non-generic resilience pipelines.
- Dynamic reloading of resilience pipelines when configurations change.
- Capability to register both generic and non-generic resilience pipeline builders, enabling dynamic pipeline instance creation.
- Automated resource management, which includes disposing of resources linked to resilience pipelines.

> [!NOTE]
> The generic `TKey` parameter sets the key type for caching individual resilience pipelines within the registry. Typically, you would use the string-based `ResiliencePipelineRegistry<string>`.

## Usage

To register pipeline builders, use the `TryAddBuilder(...)` method. This method accepts a callback argument that configures an instance of `ResiliencePipelineBuilder` for the pipeline being defined. The registry supports both generic and non-generic resilience pipelines.

Here's an example demonstrating these features:

<!-- snippet: registry-usage -->
```cs
var registry = new ResiliencePipelineRegistry<string>();

// Register builder for pipeline "A"
registry.TryAddBuilder("A", (builder, context) =>
{
    // Define your pipeline
    builder.AddRetry(new RetryStrategyOptions());
});

// Register generic builder for pipeline "A"; you can use the same key
// because generic and non-generic pipelines are stored separately
registry.TryAddBuilder<HttpResponseMessage>("A", (builder, context) =>
{
    // Define your pipeline
    builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>());
});

// Fetch pipeline "A"
ResiliencePipeline pipelineA = registry.GetPipeline("A");

// Fetch generic pipeline "A"
ResiliencePipeline<HttpResponseMessage> genericPipelineA = registry.GetPipeline<HttpResponseMessage>("A");

// Returns false since pipeline "unknown" isn't registered
registry.TryGetPipeline("unknown", out var pipeline);

// Throws KeyNotFoundException because pipeline "unknown" isn't registered
try
{
    registry.GetPipeline("unknown");
}
catch (KeyNotFoundException)
{
    // Handle the exception
}
```
<!-- endSnippet -->

Additionally, the registry allows you to add pipelines with the `GetOrAddPipeline(...)` method. In this method, there's no need to register builders. Instead, the caller provides a factory method called when the pipeline isn't cached:

<!-- snippet: registry-usage-no-builder -->
```cs
var registry = new ResiliencePipelineRegistry<string>();

// Dynamically retrieve or create pipeline "A"
ResiliencePipeline pipeline = registry.GetOrAddPipeline("A", (builder, context) =>
{
    // Define your pipeline
    builder.AddRetry(new RetryStrategyOptions());
});

// Dynamically retrieve or create generic pipeline "A"
ResiliencePipeline<HttpResponseMessage> genericPipeline = registry.GetOrAddPipeline<HttpResponseMessage>("A", (builder, context) =>
{
    // Define your pipeline
    builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>());
});
```
<!-- endSnippet -->

## Registry options

The constructor for `ResiliencePipelineRegistry<TKey>` accepts a parameter of type `ResiliencePipelineRegistryOptions<TKey>`. This parameter lets you configure the behavior of the registry. Here's a breakdown of the available properties:

| Property                | Default Value                                                   | Description                                                       |
| ----------------------- | --------------------------------------------------------------- | ----------------------------------------------------------------- |
| `BuilderFactory`        | Function returning a new `ResiliencePipelineBuilder` each time. | Allows consumers to customize builder creation.                   |
| `PipelineComparer`      | `EqualityComparer<TKey>.Default`                                | Comparer the registry uses to fetch resilience pipelines.         |
| `BuilderComparer`       | `EqualityComparer<TKey>.Default`                                | Comparer the registry uses to fetch registered pipeline builders. |
| `InstanceNameFormatter` | `null`                                                          | Delegate formatting `TKey` to instance name.                      |
| `BuilderNameFormatter`  | Function returning the `key.ToString()` value.                  | Delegate formatting `TKey` to builder name.                       |

> [>NOTE]
> The `BuilderName` and `InstanceName` are used in [telemetry](telemetry.md#metrics).

Usage example:

<!-- snippet: registry-options -->
```cs
var options = new ResiliencePipelineRegistryOptions<string>
{
    BuilderComparer = StringComparer.OrdinalIgnoreCase,
    PipelineComparer = StringComparer.OrdinalIgnoreCase,
    BuilderFactory = () => new ResiliencePipelineBuilder
    {
        InstanceName = "lets change the defaults",
        Name = "lets change the defaults",
    },
    BuilderNameFormatter = key => $"key:{key}",
    InstanceNameFormatter = key => $"instance-key:{key}",
};

var registry = new ResiliencePipelineRegistry<string>();
```
<!-- endSnippet -->

Even though the example might seem unnecessary, given that the defaults for a registry using the `string` type are suitable, it showcases the various properties of the registry and how to set them up. This is particularly helpful when you use [complex registry keys](#complex-registry-keys).

## Dynamic reloads

Dynamic reloading lets you refresh cached pipelines when the reload token, represented as a `CancellationToken`, is triggered. To enable dynamic reloads:

<!-- snippet: registry-reloads -->
```cs
var registry = new ResiliencePipelineRegistry<string>();

registry.TryAddBuilder("A", (builder, context) =>
{
    // Add the reload token. Tokens that are already canceled are ignored.
    context.AddReloadToken(cancellationToken);

    // Define the pipeline.
    builder.AddRetry(new RetryStrategyOptions());
});

// This instance remains valid even after a reload.
ResiliencePipeline pipeline = registry.GetPipeline("A");
```
<!-- endSnippet -->

- If an error occurs during reloading, the cached pipeline remains, and dynamic reloading stops.
- You should not reuse the cancellation token when the pipeline is reloaded.
- Pipelines enabled for reloads remain valid and current post-reload. The registry manages this transparently.

## Resource disposal

The registry caches and manages all pipelines and resources linked to them. When you dispose of the registry, all pipelines created by it are also disposed of and can't be used anymore. The following example illustrates this:

<!-- snippet: registry-disposed -->
```cs
var registry = new ResiliencePipelineRegistry<string>();

// This instance is valid even after reload.
ResiliencePipeline pipeline = registry
    .GetOrAddPipeline("A", (builder, context) => builder.AddTimeout(TimeSpan.FromSeconds(10)));

// Dispose the registry
registry.Dispose();

try
{
    pipeline.Execute(() => { });
}
catch (ObjectDisposedException)
{
    // Using a pipeline that was disposed by the registry
}
```
<!-- endSnippet -->

The registry also allows for the registration of dispose callbacks. These are called when a pipeline is discarded, either because of the registry's disposal or after the pipeline has reloaded. The example below works well with dynamic reloads, letting you dispose of the `CancellationTokenSource` when it's not needed anymore.

<!-- snippet: registry-reloads-and-dispose -->
```cs
var registry = new ResiliencePipelineRegistry<string>();

registry.TryAddBuilder("A", (builder, context) =>
{
    var cancellation = new CancellationTokenSource();

    // Register the source for potential external triggering
    RegisterCancellationSource(cancellation);

    // Add the reload token; note that an already cancelled token is disregarded
    context.AddReloadToken(cancellation.Token);

    // Configure your pipeline
    builder.AddRetry(new RetryStrategyOptions());

    context.OnPipelineDisposed(() => cancellation.Dispose());
});
```
<!-- endSnippet -->

Both `AddReloadToken(...)` and `OnPipelineDisposed(...)` are used to implement the `EnableReloads<TOptions>(...)` extension method that is used by the [Dependency Injection layer](dependency-injection.md#dynamic-reloads).

## Complex registry keys

Though the pipeline registry supports complex keys, we suggest you use them when defining pipelines with the [Dependency Injection](dependency-injection.md) (DI) containers. For further information, see the [section on complex pipeline keys](dependency-injection.md#complex-pipeline-keys).