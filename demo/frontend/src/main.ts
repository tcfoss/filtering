import {
  FilterOperators,
  SortDirections,
  LogicalOperators,
} from '@tcflanagan/filtering-contracts';
import type {
  DataRequest,
  DynamicDataRequest,
  DataResult,
  Filter,
} from '@tcflanagan/filtering-contracts';

const API = 'http://localhost:5050';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function inspect(label: string, req: unknown, res: unknown) {
  (document.getElementById('inspector') as HTMLPreElement).textContent =
    `// ${label}\n\nREQUEST:\n${JSON.stringify(req, null, 2)}\n\nRESPONSE:\n${JSON.stringify(res, null, 2)}`;
}

function renderTable(container: HTMLElement, data: Record<string, unknown>[]) {
  if (data.length === 0) {
    container.innerHTML = '<p><em>No results.</em></p>';
    return;
  }
  const cols = Object.keys(data[0]);
  const rows = data
    .map(row => `<tr>${cols.map(c => `<td>${row[c] ?? ''}</td>`).join('')}</tr>`)
    .join('');
  container.innerHTML = `<table><thead><tr>${cols.map(c => `<th>${c}</th>`).join('')}</tr></thead><tbody>${rows}</tbody></table>`;
}

// ---------------------------------------------------------------------------
// Standard request
// ---------------------------------------------------------------------------

let currentPage = 1;

async function search(page = 1) {
  currentPage = page;

  const nameVal = (document.getElementById('nameFilter') as HTMLInputElement).value.trim();
  const deptSelect = document.getElementById('deptFilter') as HTMLSelectElement;
  const selectedDepts = Array.from(deptSelect.selectedOptions).map(o => o.value);
  const minSalaryVal = (document.getElementById('minSalary') as HTMLInputElement).value.trim();
  const maxSalaryVal = (document.getElementById('maxSalary') as HTMLInputElement).value.trim();
  const activeOnly = (document.getElementById('activeOnly') as HTMLInputElement).checked;
  const sortField = (document.getElementById('sortField') as HTMLSelectElement).value;
  const sortDir = (document.getElementById('sortDir') as HTMLSelectElement).value as 'asc' | 'desc';
  const pageSize = parseInt((document.getElementById('pageSize') as HTMLSelectElement).value);

  const filters: Filter[] = [];

  if (nameVal) {
    filters.push({ type: 'simple', field: 'Name', operator: FilterOperators.Contains, value: nameVal });
  }
  if (selectedDepts.length === 1) {
    filters.push({ type: 'simple', field: 'Department.Name', operator: FilterOperators.EqualTo, value: selectedDepts[0] });
  } else if (selectedDepts.length > 1) {
    filters.push({ type: 'set', field: 'Department.Name', values: selectedDepts, negated: false });
  }
  if (minSalaryVal && maxSalaryVal) {
    filters.push({ type: 'range', field: 'Salary', valueFrom: minSalaryVal, valueTo: maxSalaryVal });
  } else if (minSalaryVal) {
    filters.push({ type: 'simple', field: 'Salary', operator: FilterOperators.GreaterThanOrEqualTo, value: minSalaryVal });
  } else if (maxSalaryVal) {
    filters.push({ type: 'simple', field: 'Salary', operator: FilterOperators.LessThanOrEqualTo, value: maxSalaryVal });
  }
  if (activeOnly) {
    filters.push({ type: 'simple', field: 'IsActive', operator: FilterOperators.EqualTo, value: 'true' });
  }

  const request: DataRequest = {
    filter: filters.length === 1
      ? filters[0]
      : filters.length > 1
        ? { type: 'composite', logicalOperator: LogicalOperators.And, filters }
        : undefined,
    sorts: [{ field: sortField, direction: sortDir === 'asc' ? SortDirections.Ascending : SortDirections.Descending }],
    page,
    pageSize,
  };

  const response = await fetch(`${API}/employees`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  const result: DataResult<Record<string, unknown>> = await response.json();

  inspect('POST /employees', request, result);
  renderTable(document.getElementById('result')!, result.data);
  renderPagination(result, pageSize);
}

function renderPagination(result: DataResult<unknown>, pageSize: number) {
  const pages = Math.ceil(result.totalCount / pageSize);
  const pag = document.getElementById('pagination')!;
  pag.innerHTML = `
    <button ${currentPage <= 1 ? 'disabled' : ''} id="prevBtn">◀ Prev</button>
    <span>Page ${result.page} / ${pages} &nbsp;(${result.totalCount} total)</span>
    <button ${currentPage >= pages ? 'disabled' : ''} id="nextBtn">Next ▶</button>
  `;
  document.getElementById('prevBtn')?.addEventListener('click', () => search(currentPage - 1));
  document.getElementById('nextBtn')?.addEventListener('click', () => search(currentPage + 1));
}

document.getElementById('searchBtn')!.addEventListener('click', () => search(1));

// ---------------------------------------------------------------------------
// Dynamic request
// ---------------------------------------------------------------------------

async function dynamicSearch() {
  const rawFields = (document.getElementById('dynFields') as HTMLInputElement).value;
  const requestedFields = rawFields.split(',').map(f => f.trim()).filter(Boolean);
  const deptVal = (document.getElementById('dynDept') as HTMLSelectElement).value;
  const cityVal = (document.getElementById('dynCity') as HTMLInputElement).value.trim();

  const filters: Filter[] = [];
  if (deptVal) {
    filters.push({ type: 'simple', field: 'Department.Name', operator: FilterOperators.EqualTo, value: deptVal });
  }
  if (cityVal) {
    filters.push({ type: 'simple', field: 'Department.Office.City', operator: FilterOperators.Contains, value: cityVal });
  }

  const request: DynamicDataRequest = {
    requestedFields,
    filter: filters.length === 1
      ? filters[0]
      : filters.length > 1
        ? { type: 'composite', logicalOperator: LogicalOperators.And, filters }
        : undefined,
    sorts: [{ field: 'Name', direction: SortDirections.Ascending }],
    page: 1,
    pageSize: 20,
  };

  const response = await fetch(`${API}/employees/dynamic`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  const result: DataResult<Record<string, unknown>> = await response.json();

  inspect('POST /employees/dynamic', request, result);
  renderTable(document.getElementById('dynResult')!, result.data);
}

document.getElementById('dynSearchBtn')!.addEventListener('click', dynamicSearch);

// ---------------------------------------------------------------------------
// Initial load
// ---------------------------------------------------------------------------

search();
