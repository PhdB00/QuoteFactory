# Quote Fetcher

## Overview

Quote Fetcher is a .NET solution that demonstrates integration with a third-party Quote API. This solution was developed as part of a coding assessment where candidates were required to integrate with an existing API without prior knowledge of its behavior, quirks, or limitations.

## Background

### The Coding Assessment Context

This solution emerged from a coding assessment with the following constraints:

- **Pre-existing API**: The organization provided a hosted API endpoint that candidates needed to integrate with
- **No API Documentation**: Candidates had to discover the API's behavior through exploration and testing
- **Unknown Quirks**: The API had several undocumented challenges that needed to be identified and handled

### The Recreated API

The `QuoteFetcher.Api` project in this solution is a **recreation** of the organization's original API. It exposes the same interface and intentionally replicates the same challenges encountered during the assessment, including:

- **Insufficient Data**: The API may not have enough unique Quotations to satisfy all requests
- **Duplicate Results**: The `/quote` endpoint returns a single random Quotation, which may result in duplicates across multiple calls
- **Unusual Naming Conventions**: The API uses unconventional naming (e.g., `quote_category` instead of `categories`, `icon_url` for a field that doesn't actually return valid image URLs)
- **Invalid Image URLs**: The `icon_url` field returns URLs to images that do not exist
- **No Pagination**: The quote endpoint returns one quote at a time, requiring multiple API calls to fetch multiple quotations

## Solution Architecture

### Projects

#### Source Projects (`src/`)

1. **QuoteFetcher.Api**
   - Minimal API that recreates the original assessment API
   - Endpoints: `/quote`, `/quote_category`
   - Simulates the quirks of the original API for testing purposes

2. **QuoteFetcher.Application**
   - Core business logic and API integration
   - Uses Refit for HTTP client abstraction
   - Implements resilience patterns (retry with exponential backoff)
   - Handles duplicate detection and quote aggregation

3. **QuoteFetcher.Client.Console**
   - Interactive console application
   - Menu-driven interface for end users
   - Displays quotes fetched from the API

#### Test Projects (`tst/`)

1. **QuoteFetcher.UnitTests** - Unit tests for console application logic
2. **QuoteFetcher.Application.UnitTests** - Unit tests for application layer
3. **QuoteFetcher.Api.IntegrationTests** - Integration tests for the API
4. **QuoteFetcher.ArchitectureTests** - Architecture compliance tests using NetArchTest

## Key Design Decisions

### 1. Handling Duplicate Quotes

**Problem**: The API returns random quotes, which may result in duplicates when making multiple requests.

**Solution**:
- Implemented `IQuoteHashSet` to track unique quotes using hash-based deduplication
- The handler makes additional API calls beyond the requested number to account for duplicates
- Configurable `MaxRetryOnDuplicate` setting limits the maximum additional attempts
- Formula: `MaxRequests = RequestedQuotes + MaxRetryOnDuplicate`

**Code**: `GetQuotesStreamQueryHandler.cs:42-56`

### 2. Concurrent API Calls with Rate Limiting

**Problem**: Fetching quotes one at a time would be slow and provide poor user experience.

**Solution**:
- Queue multiple API calls using `Task.Run` and process results with `Task.WhenEach`
- Use `SemaphoreSlim` to limit concurrent API calls (configured via `MaxConcurrentApiCalls`)
- Stream results to the UI as they arrive, rather than waiting for all requests to complete

**Code**: `GetQuotesStreamQueryHandler.cs:62-111`

### 3. Resilience and Fault Tolerance

**Problem**: External APIs can be unreliable and may experience transient failures.

**Solution**:
- Implemented Polly-based resilience patterns via `AddStandardResilienceHandler`
- Retry strategy: 5 attempts with exponential backoff and jitter
- Total request timeout: 30 seconds
- Graceful degradation when API cannot satisfy full request

**Code**: `DependencyInjection.cs:28-41`

### 4. Streaming Results

**Problem**: Users shouldn't wait for all quotes to be fetched before seeing any results.

**Solution**:
- `IStreamQueryHandler<TQuery, TResponse>` abstraction for async streaming
- `IAsyncEnumerable<string>` return type allows yielding quotes as they arrive
- Improves perceived performance and user experience

**Code**: `GetQuotesStreamQueryHandler.cs:22-23`

### 5. Clean Architecture

**Decision**: Separate concerns using layered architecture.

**Structure**:
- **API Layer**: Minimal API endpoints
- **Application Layer**: Business logic, MediatR handlers, API integration
- **Presentation Layer**: Console UI with menu system
- **Abstractions**: Interfaces for dependency inversion (`IQuoteApi`, `IQuoteHashSet`)

### 6. Validation

**Decision**: Use FluentValidation for request validation.

**Implementation**:
- Validates number of quotes requested (1-9)
- Validates category is not empty
- Fails fast with clear error messages

**Code**: `GetQuotesRequestValidator.cs`

### 7. Configuration-Driven Behavior

**Decision**: Make key parameters configurable rather than hard-coded.

**Settings** (`appsettings.json`):
```json
{
  "QuoteApi": {
    "ApiHost": "https://localhost:7777",
    "MaxRetryOnDuplicate": 50,
    "MaxConcurrentApiCalls": 5
  }
}
```

### 8. Testing Strategy

**Approach**: Multi-layered testing for confidence.

- **Unit Tests**: Test business logic in isolation with mocks
- **Integration Tests**: Test API endpoints with WebApplicationFactory
- **Architecture Tests**: Enforce architectural boundaries and dependencies

## API Quirks Replicated

The recreated API intentionally includes these quirks from the original assessment:

1. **Naming Convention**: `quote_category` instead of REST-conventional `/categories`
2. **Property Names**: `icon_url` and `Value` (inconsistent casing)
3. **Non-existent Image URLs**: `icon_url` returns URLs that don't resolve to actual images
4. **Limited Dataset**: The API has a finite set of quotes per category, making it impossible to guarantee large requests
5. **Random Selection**: No deterministic ordering; duplicates are possible across calls

## Running the Solution

### Prerequisites
- .NET 9.0 SDK

### Running the API
```bash
cd QuoteFetcher.Api
dotnet run
```

### Running the Console Client
```bash
cd QuoteFetcher.Client.Console
dotnet run
```

### Running Tests
```bash
dotnet test
```

## Lessons Learned from the Assessment

1. **API Exploration**: Always test API behavior thoroughly before building integration logic
2. **Edge Cases**: Real-world APIs have quirks; defensive coding is essential
3. **User Experience**: Streaming results and providing feedback during long operations improves UX
4. **Resilience**: External dependencies will fail; build retry logic and timeouts
5. **Duplicate Handling**: When dealing with random data, deduplication is critical
6. **Configuration**: Hard-coded values make testing and tuning difficult; use configuration

## License

MIT License

Copyright (c) 2025-2026 PB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Contact

Please use GitHub Issues for all communications regarding this project.
