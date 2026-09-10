# Demo

A minimal working example showing the full stack: TypeScript frontend → ASP.NET Core API → in-memory LINQ query.

## Running

**Terminal 1 — backend**

```bash
cd demo/backend
dotnet run
# Listens on http://localhost:5050
```

**Terminal 2 — frontend**

```bash
cd demo/frontend
npm install
npm run dev
# Opens on http://localhost:5176
```

Then open [http://localhost:5176](http://localhost:5176) in a browser.

## What it demos

| Feature | Where |
|---|---|
| `DataRequest` → `ReflectionFilterMapper` → `ToDataResult<T>` | `POST /employees` |
| `DynamicDataRequest` → `ToDataResult<dynamic>` (field projection) | `POST /employees/dynamic` |
| Whitelist/blacklist access control | `Program.cs` |
| TypeScript `DataRequest` / `DynamicDataRequest` types | `src/main.ts` |
| Zod schemas (`DataRequestSchema`, `DynamicDataRequestSchema`) | available from `@tcfoss/filtering-contracts` |
| Composite filter (`AND` of multiple predicates) | Name + Department + Salary + IsActive controls |
| Pagination | Prev / Next buttons on standard results |
