# @tcf-oss/filtering-contracts

TypeScript mirror of the `TcfOss.Filtering.Contracts` wire models and constants.

## Install

```bash
npm install @tcf-oss/filtering-contracts
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
import { FilterOperators, SortDirections } from '@tcf-oss/filtering-contracts';
import type { DataRequest } from '@tcf-oss/filtering-contracts';

const request: DataRequest = {
  filter: { filterType: 'simple', field: 'name', operator: FilterOperators.Contains, value: 'smith' },
  sorts: [{ field: 'name', direction: SortDirections.Ascending }],
  page: 1,
  pageSize: 20,
};
```

## Related docs

- [../../docs/ContractKeys.md](../../docs/ContractKeys.md)
- [../../README.md](../../README.md)
