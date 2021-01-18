using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Theasurus.Core.Test
{
	public class GetSynonymsAsync: TheasurusServiceTestBase
	{
		[Test]
		public async Task WithExistingWord_ReturnsCorrectSynonyms()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new TheasurusService(context);

			var result = await theasurus.GetSynonymsAsync("anger");
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation" }, result);
		}

		[Test]
		public async Task WithExistingWord_But_DifferentCase_ReturnsCorrectSynonyms()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new TheasurusService(context);

			var result = await theasurus.GetSynonymsAsync("AnGeR");
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation" }, result);
		}

		[Test]
		public async Task WithExistingWord_But_PaddedWithSpaces_ReturnsCorrectSynonyms()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new TheasurusService(context);

			var result = await theasurus.GetSynonymsAsync("   anger   ");
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation" }, result);
		}

		[Test]
		public async Task WithNonExistantWord_ThrowsException()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new TheasurusService(context);

			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.GetSynonymsAsync("love"));
		}

		[Test]
		public async Task WithNullOrWhiteSpace_ThrowsException()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new TheasurusService(context);

			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.GetSynonymsAsync(null));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.GetSynonymsAsync(""));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.GetSynonymsAsync("  "));
		}
	}
}
