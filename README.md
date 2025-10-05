# AutoMapperly
AutoMapperly is meant as a bridge for [AutoMapper](https://github.com/LuckyPennySoftware/AutoMapper) users to switch to [Mapperly](https://github.com/riok/mapperly).

### AutoMapper
When using AutoMapper you inject an object of the interface IMapper<From, To>. In AutoMapper you would use your generated mappers like this.
``````csharp
var dto = mapper.Map<TestDto>(input);
``````
### Mapperly
When using Mapperly you would use the mapper directly. Either as a instance object, static method or extension method.  
In mapperly you would for example do this:
``````csharp
var mapper = new TestDtoMapper();
var car = new Test { Value = 42 };
var dto = mapper.CarToCarDto(car);
``````
### AutoMapperly 
While using Mapperly with AutoMapperly you can both have the benefits of the Mapperly source generation and compile time checks and the more abstract usage patterns of the IMapper interface.
``````csharp
var provider = new ServiceCollection()
        .AddMappers()
        .BuildServiceProvider();

var mapper = provider.GetRequiredService<IMapper<Test, TestDto>>();

var test = new Test { Value = 42 };

var dto = mapper.Map(test);
``````
In the example ".AddMappers()" will add all the mappers and it will use ServiceProvider to get the service based on the AutoMapperly IMapper<From,To> interface.

## How does it work
AutoMapperly is a source generator that detects Mapperly mapping methods and adds a IMap<From,To> interface to the instance class that calls Mapperlys generated implementation of the mapping.  
For static mappers it will create an instance class with the IMap<From,To> interface and call Mapperlys static method from there.
The same is done for extension methods.  
When using ".AddMappers()" with ServiceCollection from Microsofts Dependency injection package, it will add the open IMapper<,> interface and all the instances of the IMap<,> interface. Then when IMapper<,> is resolved it will use the ServiceProvider instance to get the matching IMap<,> instance and do the mapping.  

---
## Notes
I made this because it was fun and I saw an opportunity to try source generators. I think there might be scenarios where you can tighten you code by having a common interface for your mapping.

```csharp

public Task<TResult> DoStuff<TQueryRequest, TResult, TDto>(TQueryRequest query)
{
    var result = await mediator.Send(query);
    var dto = _mapper.Map<TResult, TDto>(result);
    return dto;
}

```