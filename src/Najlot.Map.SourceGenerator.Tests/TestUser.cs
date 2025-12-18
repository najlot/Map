namespace Najlot.Map.SourceGenerator.Tests;

public class TestUser
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;

	public DateTime DateRegistered { get; set; }

	public TestUserAddress? Address { get; set; }

	public List<TestUserFeature> Features { get; set; } = [];

	public Guid CurrentSessionId { get; set; }
}

public class TestUserAddress
{
	public string Street { get; set; } = string.Empty;
	public int HouseNumber { get; set; }
	public string City { get; set; } = string.Empty;
	public string ZipCode { get; set; } = string.Empty;
}

public class TestUserFeature
{
	public string FeatureCode { get; set; } = string.Empty;
	public string FeatureName { get; set; } = string.Empty;
}

public class TestUserModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;

	public DateTimeOffset DateRegistered { get; set; }

	public TestUserAddressModel? Address { get; set; }

	public List<TestUserFeatureModel> Features { get; set; } = [];
}

public class TestUserFeatureModel
{
	public string FeatureCode { get; set; } = string.Empty;
	public string FeatureName { get; set; } = string.Empty;
}

public class TestUserAddressModel
{
	public string Street { get; set; } = string.Empty;
	public int HouseNumber { get; set; }
	public string City { get; set; } = string.Empty;
	public string ZipCode { get; set; } = string.Empty;
}

public interface ITestUserService { void Do(); }
public class TestUserService : ITestUserService { public void Do() { } }

public class TestUserViewModel(ITestUserService userService) // : ViewModelBase or something similar
{
	public ITestUserService UserService => userService;

	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;

	public bool NotifyEnabled { get; set; }

	private class NotifyDisposable(TestUserViewModel vm) : IDisposable
	{
		public void Dispose() => vm.NotifyEnabled = true;
	}

	public IDisposable StopNotify()
	{
		NotifyEnabled = false;
		return new NotifyDisposable(this);
	}
}