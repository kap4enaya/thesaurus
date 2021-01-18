using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Theasurus.Core.Test
{
	public class AddSynonymsAsync: TheasurusTestBase
	{
		[Test]
		public async Task WithExistingWord_And_ExistingSynonym_AddsSynonymToMappping()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddSynonymsAsync("rAge", new[] { "irRitaTion" });
			CollectionAssert.AreEquivalent(new[] { "irritation" }, await theasurus.GetSynonymsAsync("rage"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("irRitaTion"));//still empty in the other direction
		}

		[Test]
		public async Task WithExistingWord_And_NonExistentSynonym_AddsSynonymToMapppingAndToWords()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddSynonymsAsync("aNgEr", new[] { "AnnOyanCe" });
			CollectionAssert.AreEquivalent(new[] { "rage", "irritation", "annoyance" }, await theasurus.GetSynonymsAsync("anger"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("annoyance"));//still empty to the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("annoyance") { Id = 4 });
		}

		[Test]
		public async Task WithNonExistentWord_And_ExistingSynonym_AddsSynonymToMappping_And_AddsNewWord()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddSynonymsAsync("FuRy", new[] { "rAgE" });
			CollectionAssert.AreEquivalent(new[] { "rage" }, await theasurus.GetSynonymsAsync("fury"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("rage"));//still empty in the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("fury") { Id = 4 });
		}

		[Test]
		public async Task WithNonExistentWord_And_NonExistentSynonym_AddsSynonymToMappping_And_AddsBothWords()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			await theasurus.AddSynonymsAsync("  LOVE  ", new[] { "  ADORATION  " });
			CollectionAssert.AreEquivalent(new[] { "adoration" }, await theasurus.GetSynonymsAsync("love"));//new synonym added
			CollectionAssert.AreEquivalent(new string[0], await theasurus.GetSynonymsAsync("adoration"));//still empty in the other direction
			CollectionAssert.Contains(context.Words.ToList(), new Word("love") { Id = 4 });
			CollectionAssert.Contains(context.Words.ToList(), new Word("adoration") { Id = 5 });
		}

		[Test]
		public async Task WithNullOrWhiteSpace_ThrowsException()
		{
			using var context = new TheasurusDbContext(ContextOptions);
			var theasurus = new Theasurus(context);

			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync(null, new[] { "love" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("", new[] { "love" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("  ", new[] { "love" }));
			Assert.ThrowsAsync<ArgumentNullException>(async () => await theasurus.AddSynonymsAsync("love", null));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("  ", new string[] { null }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("  ", new[] { "" }));
			Assert.ThrowsAsync<ArgumentException>(async () => await theasurus.AddSynonymsAsync("  ", new[] { "   " }));
		}
	}
}
