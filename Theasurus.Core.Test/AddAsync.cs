using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Theasurus.Core.Test
{
	public class AddAsync: TheasurusTestBase
	{
		[Test]
		public async Task WithExistingWord_DoesNothing()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddAsync("rAge");
			Assert.AreEqual(3, context.Words.Count());
			Assert.AreEqual(2, context.SynonymMapping.Count());
		}

		[Test]
		public async Task WithNonExistentWord_AddsNewWord()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddAsync("FuRy");
			CollectionAssert.Contains(context.Words.ToList(), new Word("fury") { Id = 4 });
			Assert.AreEqual(4, context.Words.Count());
			Assert.AreEqual(2, context.SynonymMapping.Count());
		}

		[Test]
		public async Task WithExistingWord_And_ExistingSynonym_AddsSynonymToMappping()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddAsync("rAge", new[] { "irRitaTion" });
			CollectionAssert.AreEquivalent(new[] { "irritation" }, await theasurus.GetSynonymsAsync("rage"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("irRitaTion"));//still empty in the other direction
			Assert.AreEqual(3, context.Words.Count());
			Assert.AreEqual(3, context.SynonymMapping.Count());
		}

		[Test]
		public async Task WithExistingWord_And_PreviouslyAddedSynonym_SkipsSynonym()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddAsync("aNgEr", new[] { "raGE" });
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation" }, await theasurus.GetSynonymsAsync("aNgEr"));//synonym not added second time
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("raGE"));//still empty to the other direction
			Assert.AreEqual(3, context.Words.Count());
			Assert.AreEqual(2, context.SynonymMapping.Count());
		}

		[Test]
		public async Task WithExistingWord_And_NonExistentSynonym_AddsSynonymToMapppingAndToWords()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddAsync("aNgEr", new[] { "AnnOyanCe" });
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation", "annoyance" }, await theasurus.GetSynonymsAsync("anger"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("annoyance"));//still empty to the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("annoyance") { Id = 4 });
			Assert.AreEqual(4, context.Words.Count());
			Assert.AreEqual(3, context.SynonymMapping.Count());
		}

		[Test]
		public async Task WithNonExistentWord_And_ExistingSynonym_AddsSynonymToMappping_And_AddsNewWord()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddAsync("FuRy", new[] { "rAgE" });
			CollectionAssert.AreEquivalent(new[] { "rage" }, await theasurus.GetSynonymsAsync("fury"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("rage"));//still empty in the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("fury") { Id = 4 });
			Assert.AreEqual(4, context.Words.Count());
			Assert.AreEqual(3, context.SynonymMapping.Count());
		}

		[Test]
		public async Task WithNonExistentWord_And_NonExistentSynonym_AddsSynonymToMappping_And_AddsBothWords()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddAsync("  LOVE  ", new[] { "  ADORATION  " });
			CollectionAssert.AreEquivalent(new[] { "adoration" }, await theasurus.GetSynonymsAsync("love"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("adoration"));//still empty in the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("love") { Id = 4 });
			CollectionAssert.Contains(context.Words.ToList(), new Word("adoration") { Id = 5 });
			Assert.AreEqual(5, context.Words.Count());
			Assert.AreEqual(3, context.SynonymMapping.Count());
		}

		[Test]
		public async Task WithNullOrWhiteSpace_ThrowsException()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync(null, new[] { "love" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync("", new[] { "love" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync("  ", new[] { "love" }));
			Assert.ThrowsAsync<ArgumentNullException>(async () => await theasurus.AddAsync("love", null));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync("love", new string[] { null }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync("love", new[] { "" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync("love", new[] { "   " }));

			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync(null));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync(""));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddAsync("  "));
		}
	}
}
