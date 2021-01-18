using NUnit.Framework;
using System.Threading.Tasks;

namespace Theasurus.Core.Test
{
	public class GetWordsAsync: TheasurusTestBase
	{
		[Test]
		public async Task WithTakeMoreThanTotal_ReturnsEverything()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(10, 0);
			CollectionAssert.AreEquivalent(new[] { "anger", "rage", "irritation" }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.IsNull(result.NextSkip);
		}

		[Test]
		public async Task WithTakeLessThanTotal_ReturnsFirstPage()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(2, 0);
			CollectionAssert.AreEquivalent(new[] { "anger", "rage" }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.AreEqual(2, result.NextSkip);
		}

		[Test]
		public async Task SkipFirstPage_ReturnsSecondPage()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(2, 2);
			CollectionAssert.AreEquivalent(new[] { "irritation" }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.IsNull(result.NextSkip);
		}

		[Test]
		public async Task TakeExactlyAll_ReturnsEverything()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(3, 0);
			CollectionAssert.AreEquivalent(new[] { "anger", "rage", "irritation" }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.IsNull(result.NextSkip);
		}

		[Test]
		public async Task WithSkipMoreThanTotal_ReturnsNothing()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(3, 10);
			CollectionAssert.AreEquivalent(new string[] { }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.IsNull(result.NextSkip);
		}

		[Test]
		public async Task WithTakeZero_ReturnsNothing()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(0, 0);
			CollectionAssert.AreEquivalent(new string[] { }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.AreEqual(0, result.NextSkip);
		}
	}
}
