using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Theasurus.Core.Test
{
	[TestFixture]
	public class TheasurusTests
	{
		private DbContextOptions<TheasurusDbContext> _contextOptions;

		[SetUp]
		public void Setup()
		{
			_contextOptions = new DbContextOptionsBuilder<TheasurusDbContext>().UseSqlite("Filename=Test.db").Options;

			using var context = new TheasurusDbContext(_contextOptions);

			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			var anger = new Word("anger");
			var rage = new Word("rage");
			var irritation = new Word("irritation");

			var angerAdded = context.Add(anger);
			var rageAdded = context.Add(rage);
			var irritationAdded = context.Add(irritation);

			context.SaveChanges();

			var synonym1 = new WordSynonym(angerAdded.Entity.Id, rageAdded.Entity.Id);
			var synonym2 = new WordSynonym(angerAdded.Entity.Id, irritationAdded.Entity.Id);

			context.AddRange(synonym1, synonym2);

			context.SaveChanges();
		}

		#region GetSynonymsAsync

		[Test]
		public async Task GetSynonymsAsync_WithExistingWord_ReturnsCorrectSynonyms()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetSynonymsAsync("anger");
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation" }, result);
		}

		[Test]
		public async Task GetSynonymsAsync_WithExistingWord_But_DifferentCase_ReturnsCorrectSynonyms()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetSynonymsAsync("AnGeR");
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation" }, result);
		}

		[Test]
		public async Task GetSynonymsAsync_WithExistingWord_But_PaddedWithSpaces_ReturnsCorrectSynonyms()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetSynonymsAsync("   anger   ");
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation" }, result);
		}

		[Test]
		public async Task GetSynonymsAsync_WithNonExistantWord_ThrowsException()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.GetSynonymsAsync("love"));
		}

		[Test]
		public async Task GetSynonymsAsync_WithNullOrWhiteSpace_ThrowsException()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.GetSynonymsAsync(null));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.GetSynonymsAsync(""));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.GetSynonymsAsync("  "));
		}

		#endregion

		#region AddSynonymsAsync

		[Test]
		public async Task AddSynonymsAsync_WithExistingWord_And_ExistingSynonym_AddsSynonymToMappping()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddSynonymsAsync("rAge", new[] { "irRitaTion" });
			CollectionAssert.AreEquivalent(new[] { "irritation" }, await theasurus.GetSynonymsAsync("rage"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("irritation"));//still empty in the other direction
		}

		[Test]
		public async Task AddSynonymsAsync_WithExistingWord_And_NonExistentSynonym_AddsSynonymToMapppingAndToWords()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddSynonymsAsync("aNgEr", new[] { "AnnOyanCe" });
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation", "annoyance" }, await theasurus.GetSynonymsAsync("anger"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("annoyance"));//still empty to the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("annoyance") { Id = 4 });
		}

		[Test]
		public async Task AddSynonymsAsync_WithNonExistentWord_And_ExistingSynonym_AddsSynonymToMappping_And_AddsNewWord()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddSynonymsAsync("FuRy", new[] { "rAgE" });
			CollectionAssert.AreEquivalent(new[] { "rage" }, await theasurus.GetSynonymsAsync("fury"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("rage"));//still empty in the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("fury") { Id = 4 });
		}

		[Test]
		public async Task AddSynonymsAsync_WithNonExistentWord_And_NonExistentSynonym_AddsSynonymToMappping_And_AddsBothWords()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddSynonymsAsync("  LOVE  ", new[] { "  ADORATION  " });
			CollectionAssert.AreEquivalent(new[] { "adoration" }, await theasurus.GetSynonymsAsync("love"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("adoration"));//still empty in the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("love") { Id = 4 });
			CollectionAssert.Contains(context.Words.ToList(), new Word("adoration") { Id = 5 });
		}

		[Test]
		public async Task AddSynonymsAsync_WithNullOrWhiteSpace_ThrowsException()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync(null, new[] { "love" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("", new[] { "love" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("  ", new[] { "love" }));
			Assert.ThrowsAsync<ArgumentNullException>(async () => await theasurus.AddSynonymsAsync("love", null));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("  ", new string[] { null }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("  ", new[] { "" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("  ", new[] { "   " }));
		}

		#endregion

		#region GetWordsAsync

		[Test]
		public async Task GetWordsAsync_WithTakeMoreThanTotal_ReturnsEverything()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(10, 0);
			CollectionAssert.AreEquivalent(new[] { "anger", "rage", "irritation" }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.IsNull(result.NextSkip);
		}

		[Test]
		public async Task GetWordsAsync_WithTakeLessThanTotal_ReturnsFirstPage()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(2, 0);
			CollectionAssert.AreEquivalent(new[] { "anger", "rage" }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.AreEqual(2, result.NextSkip);
		}

		[Test]
		public async Task GetWordsAsync_SkipFirstPage_ReturnsSecondPage()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(2, 2);
			CollectionAssert.AreEquivalent(new[] { "irritation" }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.IsNull(result.NextSkip);
		}

		[Test]
		public async Task GetWordsAsync_TakeExactlyAll_ReturnsEverything()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(3, 0);
			CollectionAssert.AreEquivalent(new[] { "anger", "rage", "irritation" }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.IsNull(result.NextSkip);
		}

		[Test]
		public async Task GetWordsAsync_WithSkipMoreThanTotal_ReturnsNothing()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(3, 10);
			CollectionAssert.AreEquivalent(new string[] { }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.IsNull(result.NextSkip);
		}

		[Test]
		public async Task GetWordsAsync_WithTakeZero_ReturnsNothing()
		{
			using var context = new TheasurusDbContext(_contextOptions);
			var theasurus = new Theasurus(context);

			var result = await theasurus.GetWordsAsync(0, 0);
			CollectionAssert.AreEquivalent(new string[] { }, result.Words);
			Assert.AreEqual(3, result.TotalResults);
			Assert.AreEqual(0, result.NextSkip);
		}

		#endregion
	}
}