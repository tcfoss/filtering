/**
 * Short operator keys — must match C# FilterOperators consts exactly.
 */
export const FilterOperators = {
  EqualTo: 'eq',
  NotEqualTo: 'neq',
  IsNull: 'nu',
  IsNotNull: 'nnu',
  GreaterThan: 'gt',
  GreaterThanOrEqualTo: 'gte',
  LessThan: 'lt',
  LessThanOrEqualTo: 'lte',
  StartsWith: 'sw',
  DoesNotStartWith: 'nsw',
  EndsWith: 'ew',
  DoesNotEndWith: 'new',
  Contains: 'cn',
  DoesNotContain: 'ncn',
  Like: 'lk',
  NotLike: 'nlk',
} as const;

export type FilterOperator = (typeof FilterOperators)[keyof typeof FilterOperators];

/**
 * Logical operator keys — must match C# LogicalOperators consts exactly.
 */
export const LogicalOperators = {
  And: 'and',
  Or: 'or',
} as const;

export type LogicalOperator = (typeof LogicalOperators)[keyof typeof LogicalOperators];

/**
 * Sort direction keys — must match C# SortDirections consts exactly.
 */
export const SortDirections = {
  Ascending: 'asc',
  Descending: 'desc',
} as const;

export type SortDirection = (typeof SortDirections)[keyof typeof SortDirections];

/**
 * Quantified operator keys — must match C# QuantifiedOperators consts exactly.
 */
export const QuantifiedOperators = {
  Any: 'any',
  All: 'all',
} as const;

export type QuantifiedOperator = (typeof QuantifiedOperators)[keyof typeof QuantifiedOperators];

/**
 * Filter type discriminator keys — must match C# FilterTypes consts exactly.
 */
export const FilterTypes = {
  Simple: 'simple',
  Composite: 'composite',
  Set: 'set',
  Range: 'range',
  Quantified: 'quantified',
} as const;

export type FilterType = (typeof FilterTypes)[keyof typeof FilterTypes];
