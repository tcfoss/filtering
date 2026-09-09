# TcfOss.Filtering.Contracts.Newtonsoft

Newtonsoft.Json support for `TcfOss.Filtering.Contracts` filter polymorphism.

## What this package adds

- A Newtonsoft converter that can deserialize filter hierarchies (`SimpleFilter`, `CompositeFilter`, and other filter types) from wire payloads.

## Typical usage

Add the converter to `JsonSerializerSettings` when using Newtonsoft.Json for request/response handling.

## Related docs

- [Utility extension packages](https://github.com/tcfoss/filtering/blob/master/docs/UtilityExtensionPackages.md)
- [TcfOss.Filtering.Contracts](https://github.com/tcfoss/filtering/blob/master/src/TcfOss.Filtering.Contracts/README.md)
- [Repository README](https://github.com/tcfoss/filtering/blob/master/README.md)
