using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Phema.Localization.Tests
{
	public class ServiceCollectionTests
	{
		private readonly IServiceCollection services;

		public ServiceCollectionTests()
		{
			services = new ServiceCollection();
		}

		[Fact]
		public void AddLocalization()
		{
			services.AddPhemaLocalization(localization => {});

			Assert.Single(services.Where(x => x.ServiceType == typeof(ILocalizer)));
		}

		private interface ITestLocalizationComponent : ILocalizationComponent
		{
			LocalizationTemplate Test { get; }
		}

		private class TestLocalizationComponent : ITestLocalizationComponent
		{
			public TestLocalizationComponent()
			{
				Test = new LocalizationTemplate("template");
			}
			
			public LocalizationTemplate Test { get; }
		}
		
		[Fact]
		public void LocalizationConfiguration()
		{
			services.AddPhemaLocalization(localization =>
			{
				localization.AddCultures(new[] { CultureInfo.InvariantCulture }, culture =>
				{
					culture.AddComponent<ITestLocalizationComponent, TestLocalizationComponent>();
				});
			});

			var provider = services.BuildServiceProvider();
			
			Assert.NotNull(provider.GetService<TestLocalizationComponent>());

			var options = provider.GetService<IOptions<PhemaLocalizationOptions>>();
			
			Assert.NotNull(options);

			var (cultureInfo, map) = Assert.Single(options.Value.Components);
			
			Assert.Equal(CultureInfo.InvariantCulture, cultureInfo);
			
			var (type, factory) = Assert.Single(map);
			
			Assert.Equal(typeof(ITestLocalizationComponent), type);

			var component = factory(provider);

			Assert.IsType<TestLocalizationComponent>(component);
		}
		
		[Fact]
		public void Localize()
		{
			services.AddPhemaLocalization(localization =>
			{
				localization.AddCultures(new[] { CultureInfo.InvariantCulture }, c =>
				{
					c.AddComponent<ITestLocalizationComponent, TestLocalizationComponent>();
				});
			});

			var provider = services.BuildServiceProvider();

			var localizer = provider.GetRequiredService<ILocalizer>();

			var result = localizer.Localize<ITestLocalizationComponent>(c => c.Test);
			
			Assert.Equal("template", result);
		}
	}
}