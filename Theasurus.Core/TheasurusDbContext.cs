using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Theasurus.Core
{
	public class TheasurusDbContext: DbContext
	{
		public DbSet<Word> Words { get; set; } = default!;
		public DbSet<WordSynonym> SynonymMapping { get; set; } = default!;
	}

	public record Word
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string Text { get; init; }

		public Word(string text)
		{
			Text = text;
		}
	};

	public record WordSynonym(int WordId, int SynonymId);
}
