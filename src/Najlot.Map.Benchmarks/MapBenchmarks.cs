using BenchmarkDotNet.Attributes;
using System.Linq.Expressions;

namespace Najlot.Map.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(BenchmarksConfig))]
public class MapBenchmarks
{
	private Map _simpleMap = null!;
	private Map _factoryMap = null!;
	private Map _expressionMap = null!;
	private Map _factoryCreationMap = null!;
	private UserEntity _source = null!;
	private List<UserEntity> _sourceItems = null!;

	[Params(100)]
	public int ItemCount { get; set; }

	[GlobalSetup]
	public void GlobalSetup()
	{
		_source = new UserEntity
		{
			Id = 7,
			Name = "Max Mustermann",
			Email = "max@mustermann.de",
			Age = 36
		};

		_sourceItems = Enumerable.Range(0, ItemCount)
			.Select(index => new UserEntity
			{
				Id = index,
				Name = $"User {index}",
				Email = $"user{index}@example.com",
				Age = 20 + (index % 40)
			})
			.ToList();

		_simpleMap = CreateSimpleMap();
		_factoryMap = CreateFactoryMap();
		_expressionMap = CreateExpressionMap();
		_factoryCreationMap = CreateFactoryCreationMap();
	}

	[Benchmark(Description = "Register simple map")]
	public Map RegisterSimpleMap()
	{
		var map = new Map();
		map.Register<UserEntity, UserDto>(static (from, to) => CopyUser(from, to));
		return map;
	}

	[Benchmark(Description = "Register factory map")]
	public Map RegisterFactoryMap()
	{
		var map = new Map();
		map.Register<UserEntity, UserDto>(static from => CreateUserDto(from));
		return map;
	}

	[Benchmark(Description = "Register projection")]
	public Map RegisterProjection()
	{
		var map = new Map();
		map.RegisterExpression<UserEntity, UserDto>(static user => new UserDto
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email,
			Age = user.Age
		});
		return map;
	}

	[Benchmark(Description = "Get map method")]
	public SimpleMapMethod<UserEntity, UserDto> GetMethod()
	{
		return _simpleMap.GetMethod<UserEntity, UserDto>();
	}

	[Benchmark(Description = "Get factory method")]
	public SimpleMapFactoryMethod<UserEntity, UserDto> GetFactoryMethod()
	{
		return _factoryMap.GetFactoryMethod<UserEntity, UserDto>();
	}

	[Benchmark(Description = "Get projection")]
	public Expression<Func<UserEntity, UserDto>> GetProjection()
	{
		return _expressionMap.GetExpression<UserEntity, UserDto>();
	}

	[Benchmark(Description = "Create default instance")]
	public UserDto CreateDefaultInstance()
	{
		return _simpleMap.Create<UserDto>();
	}

	[Benchmark(Description = "Create via factory")]
	public FactoryOnlyDto CreateWithFactory()
	{
		return _factoryCreationMap.Create<FactoryOnlyDto>();
	}

	[Benchmark(Description = "Map object to new")]
	public UserDto MapObjectToNew()
	{
		return _simpleMap.From(_source).To<UserDto>();
	}

	[Benchmark(Description = "Map object with factory")]
	public UserDto MapObjectWithFactory()
	{
		return _factoryMap.From(_source).To<UserDto>();
	}

	[Benchmark(Description = "Map enumerable to list")]
	public List<UserDto> MapEnumerableToList()
	{
		return _simpleMap.From<UserEntity>(_sourceItems).ToList<UserDto>();
	}

	private static Map CreateSimpleMap()
	{
		var map = new Map();
		map.Register<UserEntity, UserDto>(static (from, to) => CopyUser(from, to));
		return map;
	}

	private static Map CreateFactoryMap()
	{
		var map = new Map();
		map.Register<UserEntity, UserDto>(static from => CreateUserDto(from));
		return map;
	}

	private static Map CreateExpressionMap()
	{
		var map = new Map();
		map.RegisterExpression<UserEntity, UserDto>(static user => new UserDto
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email,
			Age = user.Age
		});
		return map;
	}

	private static Map CreateFactoryCreationMap()
	{
		var map = new Map();
		map.RegisterFactory(static type =>
		{
			if (type == typeof(FactoryOnlyDto))
			{
				return new FactoryOnlyDto("factory");
			}

			return Activator.CreateInstance(type)!;
		}, alwaysUseFactory: true);
		return map;
	}

	private static UserDto CreateUserDto(UserEntity from)
	{
		return new UserDto
		{
			Id = from.Id,
			Name = from.Name,
			Email = from.Email,
			Age = from.Age
		};
	}

	private static void CopyUser(UserEntity from, UserDto to)
	{
		to.Id = from.Id;
		to.Name = from.Name;
		to.Email = from.Email;
		to.Age = from.Age;
	}

	public sealed class UserEntity
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public int Age { get; set; }
	}

	public sealed class UserDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public int Age { get; set; }
	}

	public sealed class FactoryOnlyDto(string origin)
	{
		public string Origin { get; } = origin;
	}
}