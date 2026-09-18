# FireManager Api
FireManager Api Library

Library is written in C# and used in Asp.Net Core 2.2 applications. This library will query Aladtec's FireManager database based on your credentials supplied by Aladtec. Retrieved XML data is deserialized into objects that can be further manipulated for the clients use.

## Register Services:

Start by registering the required services by setting the required options for the api

```c#
services.AddFireManager(options =>
{
   options.AccountKey("<AccountKey>");
   options.AccountUrl("<AccessUrl>");
   options.AccountId("<AccountId>");
   
}, bool RunTests);

```

## Schedules:
Schedules that are defined in FireManager's database.

```c#
public interface IScheduleRequest
{
   Task<Stream> StreamSchedulesAsync();
   IAsyncEnumerable<FireManagerSchedule> GetSchedulesAsync();
}
```

## Positions:
Positions that are defined in FireManager's database

```c#
public interface IPositionRequest
{
   Task<Stream> StreamPositionsAsync();
   IAsyncEnumerable<FireManagerPosition> GetPositionsAsync();
}
```

## Members:
Members that are stored in FireManagers database.

```c#
public interface IMemberRequest
{
   Task<Stream> StreamMembersAsync(bool IsActive);
   IAsyncEnumerable<FireManagerMember> GetMembersAsync(bool IsActive);
}
```

## Staffed Positions:
Staffed positions are composed of a Schedule, Position, and Member object.

```c#
public interface IStaffedPositions
{
   Task<Stream> StreamStaffedPositionsAsync(int Month, int Year);
   Task<Stream> StreamStaffedPositionsAsync(DateTime RequestDate);
   Task<Stream> StreamStaffedPositionsAsync(DateTime StartDate, DateTime EndDate);

   Task<IList<FireManagerStaffedPosition>> GetStaffedPositionsAsync(int Month, int Year);
   Task<IList<FireManagerStaffedPosition>> GetStaffedPositionsAsync(DateTime RequestDate);
   Task<IList<FireManagerStaffedPosition>> GetStaffedPositionsAsync(DateTime StartDate, DateTime EndDate);
}
```

## Testing:
By setting the RunTests parameter to true, the test suite will run everytime the application starts up. If you wish to run tests manually then set the parameter to false, and inject the interface you want to test
```c#
public interface ITestRequests
```
into a controller and call one of the interface methods:

```c#
public interface ITestRequests
{
   IAsyncEnumerable<FireManagerMember> TestMemberRequest();
   IAsyncEnumerable<FireManagerSchedule> TestScheduleRequest();
   IAsyncEnumerable<FireManagerPosition> TestPositionRequest();
   Task<IList<FireManagerStaffedPosition>> TestStaffedPositionRequest(DateTime Date);
   Task<IList<FireManagerStaffedPosition>> TestStaffedPositionRequest(int Year, int Month);
}
```

Each method tests a different portion of the api to ensure that the returned data is what you requested or you can run all tests by calling

```c#
Task<bool> RunTestSuite();
```
method.

## Timezone and error handling

Scheduling requests default to the America/Chicago (Central) department timezone,
with daily windows starting at 05:00. Configure these independently of the server:

```csharp
services.AddFireManager(options =>
{
    options.AccountKey("<AccountKey>");
    options.AccountUrl("<AccessUrl>");
    options.AccountId("<AccountId>");
    options.DepartmentTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
    options.ShiftStart = TimeSpan.FromHours(7);
}, RunTests: false);
```

The single-date overload uses the supplied calendar date in the department timezone.
The month/year overload uses department midnight at the start of each month.
For explicit ranges, unspecified `DateTime` values mean department local time;
UTC values retain their instant and `DateTimeKind.Local` values retain normal .NET
local-time conversion semantics. Ambiguous or nonexistent department times during
DST transitions are rejected; use UTC values to identify the intended instant.

**Behavior change:** returned `StartShift`, `EndShift`, `ResultRange.Begin`, and
`ResultRange.End` now contain UTC values. Convert them to the department timezone
for display with `TimeZoneInfo.ConvertTimeFromUtc`. Durations use elapsed time,
including shifts crossing daylight-saving transitions. Zone-less response times
are interpreted as UTC.

Transport failures and unsuccessful HTTP responses now throw their original
exceptions instead of returning null streams. XML `<error>` responses throw
`InvalidDataException` with the API error code and message. Missing result sections
are rejected; present but empty sections return empty collections. Callers of the
`Stream*` methods must dispose the returned stream, which also disposes its HTTP
response. The `Get*` methods manage these resources automatically.

## Regression tests

The isolated regression suite uses fake HTTP responses and no Aladtec credentials.
Run with a .NET 8 SDK:

```sh
dotnet test tests/FireManager.RegressionTests/FireManager.RegressionTests.csproj
```

It covers missing member attributes, email/phone mapping, schedule positions,
empty staffing results, request timezone boundaries, DST durations, HTTP/API
failures, and response disposal. The library itself still targets .NET 5.
