# TcfOss.Filtering.Contracts.Newtonsoft

Newtonsoft.Json support for `TcfOss.Filtering.Contracts` filter polymorphism.

## What this package adds

- A Newtonsoft converter that can deserialize filter hierarchies (`SimpleFilter`, `CompositeFilter`, and other filter types) from wire payloads.

## Typical usage

Add the converter to `JsonSerializerSettings` when using Newtonsoft.Json for request/response handling.

## Related docs

- [../../docs/UtilityExtensionPackages.md](../../docs/UtilityExtensionPackages.md)
- [../TcfOss.Filtering.Contracts/README.md](../TcfOss.Filtering.Contracts/README.md)
- [../../README.md](../../README.md)
