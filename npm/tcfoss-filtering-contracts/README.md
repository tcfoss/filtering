# @tcfoss/filtering-contracts

[Documentation](https://tcfoss.github.io/filtering/) | [Repository](https://github.com/tcfoss/filtering)

TypeScript mirror of the `TcfOss.Filtering.Contracts` wire models and constants.

## Install

```bash
npm install @tcfoss/filtering-contracts
```

Optional schema validation:

```bash
npm install zod
```

## What this package includes

- Contract types (`DataRequest`, `DynamicDataRequest`, `DataResult`).
- Shared constants (`FilterOperators`, `LogicalOperators`, `SortDirections`, `QuantifiedOperators`).
- Optional Zod schemas for validating incoming payloads.

## Basic usage

```ts
import { FilterOperators, SortDirections } from '@tcfoss/filtering-contracts';
import type { DataRequest } from '@tcfoss/filtering-contracts';

const request: DataRequest = {
  filter: { filterType: 'simple', field: 'name', operator: FilterOperators.Contains, value: 'smith' },
  sorts: [{ field: 'name', direction: SortDirections.Ascending }],
  page: 1,
  pageSize: 20,
};
```

## Related docs

- [Contract keys](https://tcfoss.github.io/filtering/ContractKeys/)
- [Repository README](https://github.com/tcfoss/filtering/blob/master/README.md)
