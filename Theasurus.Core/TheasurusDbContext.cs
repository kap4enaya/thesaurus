using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Theasurus.Core
{
	public class TheasurusDbContext: DbContext
	{
		public DbSet<Word> Words { get; set; } = default!;
		public DbSet<WordSynonym> SynonymMapping { get; set; } = default!;

		public TheasurusDbContext(): base(){}
		public TheasurusDbContext([NotNullAttribute] DbContextOptions options) : base(options) { }
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

	public record WordSynonym
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }//EF doesn't allow to add Entities without primary keys

		[ForeignKey("Word")] 
		public int WordId { get; init; }

		[ForeignKey("Word")] 
		public int SynonymId { get; init; }

		public WordSynonym(int wordId, int synonymId)
		{
			WordId = wordId;
			SynonymId = synonymId;
		}
	}
}
