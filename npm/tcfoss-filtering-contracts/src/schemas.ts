import { z } from 'zod';
import { FilterOperators, LogicalOperators, QuantifiedOperators, SortDirections } from './operators.js';

export const FilterOperatorSchema = z.enum([
  FilterOperators.EqualTo,
  FilterOperators.NotEqualTo,
  FilterOperators.IsNull,
  FilterOperators.IsNotNull,
  FilterOperators.GreaterThan,
  FilterOperators.GreaterThanOrEqualTo,
  FilterOperators.LessThan,
  FilterOperators.LessThanOrEqualTo,
  FilterOperators.StartsWith,
  FilterOperators.DoesNotStartWith,
  FilterOperators.EndsWith,
  FilterOperators.DoesNotEndWith,
  FilterOperators.Contains,
  FilterOperators.DoesNotContain,
  FilterOperators.Like,
  FilterOperators.NotLike,
]);

export const LogicalOperatorSchema = z.enum([LogicalOperators.And, LogicalOperators.Or]);

export const QuantifiedOperatorSchema = z.enum([QuantifiedOperators.Any, QuantifiedOperators.All]);

export const SortDirectionSchema = z.enum([SortDirections.Ascending, SortDirections.Descending]);

export const SimpleFilterSchema = z.object({
  filterType: z.literal('simple'),
  field: z.string().min(1),
  operator: FilterOperatorSchema,
  value: z.string().optional(),
});

// Defined with z.lazy to support recursive CompositeFilter.filters and QuantifiedFilter.subFilter
export type FilterSchema = z.ZodType<import('./types.js').Filter>;

export const FilterSchema: FilterSchema = z.lazy(() =>
  z.discriminatedUnion('filterType', [SimpleFilterSchema, CompositeFilterSchema, SetFilterSchema, RangeFilterSchema, QuantifiedFilterSchema])
);

export const CompositeFilterSchema = z.object({
  filterType: z.literal('composite'),
  logicalOperator: LogicalOperatorSchema,
  filters: z.array(FilterSchema).min(1),
});

export const SetFilterSchema = z.object({
  filterType: z.literal('set'),
  field: z.string().min(1),
  negated: z.boolean(),
  values: z.array(z.string().min(1)).min(1),
});

export const RangeFilterSchema = z.object({
  filterType: z.literal('range'),
  field: z.string().min(1),
  valueFrom: z.string().min(1),
  valueTo: z.string().min(1),
  exclusive: z.boolean().optional(),
  negated: z.boolean().optional(),
});

export const QuantifiedFilterSchema = z.object({
  filterType: z.literal('quantified'),
  field: z.string().min(1),
  operator: QuantifiedOperatorSchema,
  subFilter: FilterSchema,
  isNegated: z.boolean().optional(),
  isNullable: z.boolean().optional(),
});

export const SortComponentSchema = z.object({
  field: z.string().min(1),
  direction: SortDirectionSchema,
});

export const DataRequestSchema = z.object({
  filter: FilterSchema.optional(),
  sorts: z.array(SortComponentSchema).optional(),
  page: z.number().int().positive().optional(),
  pageSize: z.number().int().positive().optional(),
});

export const DynamicDataRequestSchema = DataRequestSchema.extend({
  requestedFields: z.array(z.string().min(1)).min(1),
});

export type SimpleFilterInput = z.input<typeof SimpleFilterSchema>;
export type CompositeFilterInput = z.input<typeof CompositeFilterSchema>;
export type SetFilterInput = z.input<typeof SetFilterSchema>;
export type RangeFilterInput = z.input<typeof RangeFilterSchema>;
export type QuantifiedFilterInput = z.input<typeof QuantifiedFilterSchema>;
export type SortComponentInput = z.input<typeof SortComponentSchema>;
export type DataRequestInput = z.input<typeof DataRequestSchema>;
export type DynamicDataRequestInput = z.input<typeof DynamicDataRequestSchema>;

export function DataResultSchema<T>(itemSchema: z.ZodType<T>) {
  return z.object({
    data: z.array(itemSchema),
    totalCount: z.number().int().nonnegative(),
    page: z.number().int().positive(),
    pageSize: z.number().int().positive(),
  });
}
