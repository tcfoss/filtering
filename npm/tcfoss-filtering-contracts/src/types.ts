import type { FilterOperator, LogicalOperator, QuantifiedOperator, SortDirection } from './operators.js';

export interface SimpleFilter {
  filterType: 'simple';
  field: string;
  operator: FilterOperator;
  /** Omit for operators that require no value (IsNull, IsNotNull). */
  value?: string;
}

export interface CompositeFilter {
  filterType: 'composite';
  logicalOperator: LogicalOperator;
  filters: Filter[];
}

export interface SetFilter {
  filterType: 'set';
  field: string;
  negated: boolean;
  values: string[];
}

export interface RangeFilter {
  filterType: 'range';
  field: string;
  valueFrom: string;
  valueTo: string;
  /** When true, uses strict inequalities (> and <). Defaults to false (>= and <=). */
  exclusive?: boolean;
  /** When true, negates the range condition. Defaults to false. */
  negated?: boolean;
}

export interface QuantifiedFilter {
  filterType: 'quantified';
  field: string;
  operator: QuantifiedOperator;
  subFilter: Filter;
  /** When true, negates the quantified condition. Defaults to false. */
  isNegated?: boolean;
  /** When true, null-checks the collection before applying the quantifier. Defaults to false. */
  isNullable?: boolean;
}

export type Filter = SimpleFilter | CompositeFilter | SetFilter | RangeFilter | QuantifiedFilter;

export interface SortComponent {
  field: string;
  direction: SortDirection;
}

export interface DataRequest {
  filter?: Filter;
  sorts?: SortComponent[];
  page?: number;
  pageSize?: number;
}

export interface DynamicDataRequest extends DataRequest {
  requestedFields: string[];
}

export interface DataResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
