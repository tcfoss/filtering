# TcfOss.Filtering.Programmatic

Filter objects for use in strongly-typed C# code, and helpers for converting them to `TcfOss.Filtering.Contracts` wire models.

These may be useful for, say, filter components in a Blazor or WPF application, where you want to construct filter objects in code and then send them to an API that uses `TcfOss.Filtering.Contracts` wire models, or the wire models may be simply mapped to `TcfOss.Filtering.Linq` models in the same application

The two-step conversion is preferable to going straight to `Linq` models, since the `Contracts`-to-`Linq` mapping offers some validations: The Basic mapper ensures that field names at least *could* be actual property names (i.e. nothing that could potentially lead to SQL-injection vulnerabilities), the Reflection mapper ensures that field names actually exist on the model type, and the Dict mapper only accepts fields that a developer has explicitly allowed.
