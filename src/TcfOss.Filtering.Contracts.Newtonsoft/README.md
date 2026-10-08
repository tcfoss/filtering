# TcfOss.Filtering.Contracts.Newtonsoft

[Documentation](https://tcfoss.github.io/filtering/) | [Repository](https://github.com/tcfoss/filtering)

Newtonsoft.Json support for `TcfOss.Filtering.Contracts` filter polymorphism.

## What this package adds

- A Newtonsoft converter that can deserialize filter hierarchies (`SimpleFilter`, `CompositeFilter`, and other filter types) from wire payloads.

## Typical usage

Add the converter to `JsonSerializerSettings` when using Newtonsoft.Json for request/response handling.

## Related docs

- [Utility extension packages](https://tcfoss.github.io/filtering/UtilityExtensionPackages/)
- [TcfOss.Filtering.Contracts](https://tcfoss.github.io/filtering/packages/contracts/)
- [Repository README](https://github.com/tcfoss/filtering/blob/master/README.md)
