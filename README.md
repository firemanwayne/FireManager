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
